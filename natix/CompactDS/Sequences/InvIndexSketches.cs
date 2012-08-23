//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Original filename: natix/CompactDS/Sequences/InvIndexSketches.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using natix.SortingSearching;

namespace natix.CompactDS
{

	public class InvIndexSketches : IRankSelectSeq
	{
		/// <summary>
		///  The size of the block for the sketch
		/// </summary>
		public int AlphabetBlock;
		/// <summary>
		/// An sketch of the text
		/// </summary>
		public ListSDiff64 Sketch;
		/// <summary>
		/// Inverted index
		/// </summary>
		// public IList< IList< int > > InvIndex;
		public IList<IRankSelect> InvIndex;
		/// <summary>
		/// Vocabulary sorted by frequency
		/// </summary>
		public IList< int > FreqPerm;
		/// <summary>
		/// The size in words (entities) of the text
		/// </summary>
		public int N;
		
		public int Count {
			get {
				return this.N;
			}
		}
		
		public int Sigma {
			get {
				return this.InvIndex.Count;
			}
		}
		
		public BitmapFromList BitmapBuilder {
			get;
			set;
		}
		
		public InvIndexSketches ()
		{
			this.BitmapBuilder = BitmapBuilders.GetSArray ();
		}

		public struct sort_pair
		{
			public IRankSelect invlist;
			public string word;
			
			public sort_pair (IRankSelect invlist, string word)
			{
				this.invlist = invlist;
				this.word = word;
			}
		}

		void PermSortByFreq (IList<IList<int>> invindex, int _maxvalue)
		{
			Console.WriteLine ("Permuting vocabulary and invlists by decreasing frequency. alphabet_size: {0}",
				invindex.Count);
			int numbits = (int)Math.Ceiling (Math.Log (_maxvalue, 2));
			long maxvalue = 1 << numbits;
			Console.WriteLine ("*** numbits: {0}, maxvalue: {1}, _maxvalue: {2}", numbits, maxvalue, _maxvalue);
			this.FreqPerm = new int[invindex.Count];
			for (int i = 0; i < invindex.Count; i++) {
				this.FreqPerm[i] = i;
			}
			Func<IList<int>, long> int_cast = delegate(IList<int> L) {
				return maxvalue - L.Count;
			};
			var sorted = new FastSortSqrt<IList<int>, int> (numbits, int_cast);
			sorted.Sort (invindex, this.FreqPerm);
		}
	
		public void CreateSketch (IList<IList<int>> invindex, int maxvalue)
		{
			// after PermSortByFreq the invindex is permutated too
			var L = new IList<int>[invindex.Count];
			for (int i = 0; i < L.Length; i++) {
				L [i] = invindex [i];
			}
			this.PermSortByFreq (L, maxvalue);
			this.Sketch = new ListSDiff64 ();
			Console.WriteLine ("Creating sketch of the text");
			Chronos C = new Chronos ();
			C.Start ();
			var S = new InvIndexSketchBuilder (L, this.N);
			S.Build (this.Sketch, this.AlphabetBlock);
			C.End ();
			C.PrintStats ();
		}
		
		/// <summary>
		/// Builds the index for the sequence
		/// </summary>
		public void Build (IList<int> sequence, int alphabet_size, int alphabet_block)
		{
			this.AlphabetBlock = alphabet_block;
			var invindex = new IList<int>[alphabet_size];
			for (int i = 0; i < alphabet_size; i++) {
				invindex [i] = new List<int> ();
			}
			int pos = 0;
			foreach (var c in sequence) {
				invindex [c].Add (pos);
				pos++;
			}
			int max_freq = 0;
			pos = 0;
			foreach (var list in invindex) {
				var freq = list.Count;
				if (freq > max_freq) {
					max_freq = freq;
				}
			}
			this.N = sequence.Count;
			this.CreateSketch (invindex, max_freq + 1);
			this.InvIndex = new IRankSelect[alphabet_size];
			for (int i = 0; i < alphabet_size; i++) {
				Console.WriteLine ("*** compressing invlist {0}/{1}", i, alphabet_size);
				this.InvIndex [i] = this.BitmapBuilder (invindex [i]);
				invindex [i] = null;
			}
			invindex = null;
		}
		
		public int AccessFromSketch (int pos, int sketch_block)
		{
			var start = sketch_block * this.AlphabetBlock;
			int end = Math.Min (start + this.AlphabetBlock, this.InvIndex.Count);
			for (; start < end; start++) {
				//Console.WriteLine ("*** pos: {0}, sketch: {1}, start: {2}, end: {3}, alpha_block: {4}",
				//	pos, sketch_block, start, end, this.AlphabetBlock);
				var m = this.FreqPerm[start];
				var invlist = this.InvIndex[m];
				if (pos < invlist.Count && invlist.Access (pos)) {
					return m;
				}
			}
			throw new ArgumentOutOfRangeException ();
		}

		public IEnumerable<int> AccessRangeGeneric (int start, int length)
		{
			int end = Math.Min (length + start, this.N);
			while (start < end) {
				yield return this.Access (start);
				start++;
			}
		}
		
		public IEnumerable<int> AccessRange (int start, int length)
		{
			length = Math.Min (length, this.N - start);
			// var L = this.Sketch.ExtractFrom (start, length);
			// var newline = this.SearchWordId ("\n") >> this.VocBlockSize;
			// int reference = this.TextSketch.dset.ExtractUntilDiff (start, L, newline+1);
			int i = 0;
			foreach (var v in this.Sketch.ExtractFrom (start, length)) {
				yield return this.AccessFromSketch (v, start + i);
				i++;
			}
		}

		public int Access (int pos)
		{
			return this.AccessFromSketch(pos, this.Sketch[pos]);
		}
		
		public int Rank (int symbol, int pos)
		{
			if (pos < 0) {
				return 0;
			}
			var invlist = this.InvIndex[symbol];
			pos = Math.Min (invlist.Count - 1, pos);
			return invlist.Rank1 (pos);
		}
		
		public int Select (int symbol, int rank)
		{
			if (rank < 1) {
				return -1;
			}
			return this.InvIndex[symbol].Select1 (rank);
		}
		
		public IRankSelect Unravel (int symbol)
		{
			return this.InvIndex[symbol];
		}		

		// **** IO methods

		/// <summary>
		/// Save the index
		/// </summary>
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.AlphabetBlock);
			int vocsize = this.InvIndex.Count;
			
			// Console.WriteLine ("Saving sketch: {0} MB", this.Sketch.Stream.CountBits / 8.0 / 1024.0 / 1024.0)
			Output.Write ((int)this.N);
			double size = Output.BaseStream.Length;
			this.Sketch.Save (Output);
			size = Output.BaseStream.Length - size;
			Console.WriteLine ("***=== Sketch size: {0} MB", size / (1 << 20));
			Output.Write ((int)vocsize);
			size = Output.BaseStream.Length;
			PrimitiveIO<int>.WriteVector (Output, this.FreqPerm);
			size = Output.BaseStream.Length - size;
			Console.WriteLine ("***=== FreqPerm size: {0} MB", size / (1 << 20));
			Console.WriteLine ("*** Saving {0} inverted lists", vocsize);
			size = Output.BaseStream.Length;
			foreach (var L in this.InvIndex) {
				RankSelectGenericIO.Save (Output, L);
			}
			size = Output.BaseStream.Length - size;
			Console.WriteLine ("***=== InvIndex size: {0} MB", size / (1 << 20));
		}

		public void Load (BinaryReader Input)
		{
			this.AlphabetBlock = Input.ReadInt32 ();
			Console.WriteLine ("Loading sketch");
			long start_size = Input.BaseStream.Position;
			this.N = Input.ReadInt32 ();
			this.Sketch = new ListSDiff64 ();
			this.Sketch.Load (Input);
			Console.WriteLine ("*** sketch-size-bytes:", Input.BaseStream.Position - start_size);
			start_size = Input.BaseStream.Position;
			Console.WriteLine ("Loading vocabulary");
			int vocabulary_size;
			vocabulary_size = Input.ReadInt32 ();
			this.FreqPerm = new int[vocabulary_size];
			PrimitiveIO<int>.ReadFromFile (Input, vocabulary_size, this.FreqPerm);
			Console.WriteLine ("*** perms-size-bytes:", Input.BaseStream.Position - start_size);
			start_size = Input.BaseStream.Position;
			this.InvIndex = new IRankSelect[vocabulary_size];
			for (int i = 0; i < this.InvIndex.Count; i++) {
				this.InvIndex[i] = RankSelectGenericIO.Load (Input);
			}
			Console.WriteLine ("*** invindex-size-bytes:", Input.BaseStream.Position - start_size);
		}
	}
}
