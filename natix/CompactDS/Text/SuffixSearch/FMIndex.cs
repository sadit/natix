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
//   Original filename: natix/CompactDS/Text/SuffixSearch/FMIndex.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class FMIndex : ITextIndex
	{
		public int[] charT;
		public IRankSelect newF;
		public IRankSelectSeq seqIndex;
		public IRankSelect SA_marked;
		public ListIFS SA_samples;
		public ListIFS SA_invsamples;
		short SA_sample_step;
		
		public FMIndex ()
		{
		}
		
		public int N {
			get {
				return this.newF.Count - 1;
			}
		}
		 public int Sigma {
			get {
				return this.charT.Length - 1;
			}
		}
				
		public SequenceBuilder SeqBuilder {			
			get;
			set;
		}

		public void Build (string sa_name)
		{
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".structs"))) {
				this.newF = RankSelectGenericIO.Load (Input);
				int len = this.newF.Count1;
				this.charT = new int[len];
				PrimitiveIO<int>.ReadFromFile (Input, len, this.charT);
			}
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".bwt"))) {
				var L = new ListIFS ();
				L.Load (Input);
				this.seqIndex = this.SeqBuilder (L, this.charT.Length);
			}
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".samples"))) {
				this.SA_sample_step = Input.ReadInt16 ();
				this.SA_marked = RankSelectGenericIO.Load (Input);
				var _samples = new ListIFS ();
				_samples.Load (Input);
				var _invsamples = new ListIFS ();
				_invsamples.Load (Input);
				this.SA_samples = _samples;
				this.SA_invsamples = _invsamples;
			}
		}
		
		public int TextLength{
			get { return this.newF.Count - 1; }
		}

		public int AlphabetSize
		{
			get {
				return this.charT.Length;
			}
		}
		
		public void Save (string basename)
		{
			using (var Output = new BinaryWriter (File.Create (basename + ".structs"))) {
				RankSelectGenericIO.Save (Output, this.newF);
				PrimitiveIO<int>.WriteVector (Output, this.charT);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".bwt-index"))) {
				Console.WriteLine ("Saving bwt-index {0}", this.seqIndex);
				RankSelectSeqGenericIO.Save (Output, this.seqIndex);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".structs-samples"))) {
				Output.Write ((short)this.SA_sample_step);
				RankSelectGenericIO.Save (Output, this.SA_marked);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".samples"))) {
				this.SA_samples.Save (Output);
				this.SA_invsamples.Save (Output);
			}	
		}
		
		public void Load (string basename)
		{
			using (var Input = new BinaryReader (File.OpenRead (basename + ".structs"))) {
				this.newF = RankSelectGenericIO.Load (Input);
				this.charT = new int[this.newF.Count1];
				PrimitiveIO<int>.ReadFromFile (Input, this.charT.Length, this.charT);
			}
			// this.seqIndex = new WaveletTree ();
			// this.seqIndex.Load (Input);
			using (var Input = new BinaryReader (File.OpenRead (basename + ".bwt-index"))) {
				this.seqIndex = RankSelectSeqGenericIO.Load (Input);
			}
			using (var Input = new BinaryReader (File.OpenRead (basename + ".structs-samples"))) {
				this.SA_sample_step = Input.ReadInt16 ();
				this.SA_marked = RankSelectGenericIO.Load (Input);
			}
			using (var Input = new BinaryReader (File.OpenRead (basename + ".samples"))) {
				var _samples = new ListIFS ();
				_samples.Load (Input);
				var _invsamples = new ListIFS ();
				_invsamples.Load (Input);
				this.SA_samples = _samples;
				this.SA_invsamples = _invsamples;
			}
		}
		
		int GetCharId (int c)
		{
			int e = GenericSearch.FindLast<int> (c, this.charT, 1, this.charT.Length);
			if (e < 0) {
				return -1;
			}
			if (c != this.charT[e]) {
				return -1;
			}
			return e;
		}
		
		public bool Search (IList<int> query, out int start_pos, out int end_pos)
		{
			start_pos = end_pos = -1;
			int sp = 0;
			int ep = this.newF.Count - 1;
			for (int m = query.Count - 1; m >= 0; m--) {
				var c = this.GetCharId (query[m]);
				if (c < 0) {
					return false;
				}
				var abs_pos = this.newF.Select1 (c + 1);
				sp = abs_pos + this.seqIndex.Rank (c, sp - 1);
				ep = abs_pos + this.seqIndex.Rank (c, ep) - 1;
			}
			start_pos = sp;
			end_pos = ep;
			return true;
		}

		public int CountOcc (IList<int> query)
		{
			int sp;
			int ep;
			if (this.Search (query, out sp, out ep)) {
				return ep - sp + 1;
			}
			return 0;
		}
		
		int LF (int i)
		{
			int c = this.seqIndex.Access (i);
			int p = this.newF.Select1 (c + 1) - 1;
			return p + this.seqIndex.Rank (c, i);
		}
		
		public IList<int> Locate (IList<int> query)
		{
			int start_pos;
			int end_pos;
			
			if (!this.Search (query, out start_pos, out end_pos)) {
				return new int[0];
			}
			var L = new int[end_pos - start_pos + 1];
			int i = 0;
			while (start_pos <= end_pos) {
				L [i] = this.Locate (start_pos);
				start_pos++;
			}
			return L;
		}

		public int Locate (int suffix)
		{
			int power = 0;
			while (true) {
				if (this.SA_marked.Access(suffix)) {
					break;
				}
				++power;
				suffix = this.LF (suffix);
			}
			int sample = (int)this.SA_samples[this.SA_marked.Rank1 (suffix) - 1];
			return sample + power;
		}
		
		public IList<int> Extract (int start_pos, int len)
		{
			var Output = new List<int> (len);
			int end_pos = start_pos + len - 1;
			int end_index = (int)Math.Ceiling (end_pos / ((float)this.SA_sample_step));
			int gap;
			if (end_index + 1 == this.SA_invsamples.Count) {
				gap = this.newF.Count - end_pos - 1;
			} else {
				gap = this.SA_sample_step - end_pos % this.SA_sample_step;
			}
			this._Extract (Output, gap, this.SA_invsamples [end_index], len);
			return _Reverse (Output);
		}
		
		void _Extract (IList<int> Output, int gap, int start, int len)
		{
			for (int i = 0; i < gap; ++i) {
				start = this.LF (start);
			}
			for (int i = 0; i < len; i++) {
				if (start == 0) {
					break;
				}
				int num_char = this.newF.Rank1 (start);
				Output.Add (this.charT[num_char - 1]);
				start = this.LF (start);
			}
		}
		
		IList<int> _Reverse (IList<int> Output)
		{
			int mid = Output.Count / 2;
			for (int left = 0; left < mid; left++) {
				var u = Output[left];
				var right = Output.Count - 1 - left;
				Output[left] = Output[right];
				Output[right] = u;
			}
			return Output;
		}
	}
}