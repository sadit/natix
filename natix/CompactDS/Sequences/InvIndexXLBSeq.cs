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
//   Original filename: natix/CompactDS/Sequences/InvIndexXLBSeq.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace natix.CompactDS
{

	public class InvIndexXLBSeq : IRankSelectSeq
	{
		/// <summary>
		/// Inverted index
		/// </summary>
		// public IList< IList< int > > InvIndex;
		public IList<IRankSelect> InvIndex;
		public IRankSelect Lens;
		IPermutation Perm;
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
		
		
		public InvIndexXLBSeq ()
		{
		}

		/// <summary>
		/// Builds the index for the sequence
		/// </summary>
		public void Build (IList<int> sequence, int alphabet_size, int t = 16,
		                   BitmapFromList rowbuilder = null, BitmapFromBitStream lenbuilder = null)
		{
			if (rowbuilder == null) {
				rowbuilder = BitmapBuilders.GetSArray ();
			}
			if (lenbuilder == null) {
				lenbuilder = BitmapBuilders.GetGGMN_wt (12);
			}
			var invindex = new IList<int>[alphabet_size];
			for (int i = 0; i < alphabet_size; i++) {
				invindex [i] = new List<int> ();
			}
			int pos = 0;
			foreach (var c in sequence) {
				invindex [c].Add (pos);
				pos++;
			}
			pos = 0;
			this.N = sequence.Count;
			this.InvIndex = new IRankSelect[alphabet_size];
			var lens = new BitStream32 ();
			for (int i = 0; i < alphabet_size; i++) {
				if (i % 1000 == 0) {
					if (i % 10000 == 0) {					
						Console.WriteLine ();
						Console.Write ("*** InvIndexXLBSeq {0}/{1}", i, alphabet_size);
					} else {
						Console.Write (", {0}", i);
					}
				}
				this.InvIndex [i] = rowbuilder (invindex [i]);
				lens.Write (true);
				lens.Write (false, invindex [i].Count);
				invindex [i] = null;
			}
			lens.Write (true);
			Console.WriteLine ();
			Console.WriteLine ("done, now saving permutation and the Len bitmap");
			this.Lens = lenbuilder (new FakeBitmap (lens));
			var p = new ListGen_MRRR ();
			p.Build (this.GetNotIdxPERM (), t, null);
			Console.WriteLine ("done");
			this.Perm = p;
		}
		
		public IList<int> GetNotIdxPERM ()
		{
			long n = this.Count;
			var gen = new ListGen<int> (delegate(int i) {
				var pos = this.Lens.Select0 (i + 1);
				// pos + 1 = rank0 + rank1
				// rank1 = pos + 1 - rank0 = pos + 1 - (i - 1) = pos - i => symbol = rank1 - 1
				var symbol = pos - i - 1;
				pos = this.Lens.Select1(symbol + 1);
				var rank0 = pos - symbol;
				return this.InvIndex[symbol].Select1(i - rank0 + 1);
			}, (int)n);
			return gen;
		}
		
		public IPermutation GetPERM ()
		{
			return this.Perm;
		}

		public int Access (int pos)
		{
			
			var i = this.Perm.Inverse (pos);
			var _rank0 = i + 1;
			var _pos = this.Lens.Select0 (_rank0);
			var _rank1 = _pos + 1 - _rank0;
			return _rank1 - 1;
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
			int vocsize = this.InvIndex.Count;
			Output.Write ((int)this.N);
			Output.Write ((int)vocsize);
			foreach (var L in this.InvIndex) {
				RankSelectGenericIO.Save (Output, L);
			}
			RankSelectGenericIO.Save (Output, this.Lens);
			this.Perm.Save (Output);
		}

		public void Load (BinaryReader Input)
		{
			this.N = Input.ReadInt32 ();
			int vocsize = Input.ReadInt32 ();
			this.InvIndex = new IRankSelect[vocsize];
			for (int i = 0; i < vocsize; i++) {
				this.InvIndex [i] = RankSelectGenericIO.Load (Input);
			}
			this.Lens = RankSelectGenericIO.Load (Input);
			var p = new ListGen_MRRR ();
			p.Load (Input);
			p.SetPERM (this.GetNotIdxPERM ());
			this.Perm = p;
		}

        public IList<int> GetRawSeq ()
        {
            return RankSelectSeqGenericIO.ToIntArray(this, false);
        }

	}
}
