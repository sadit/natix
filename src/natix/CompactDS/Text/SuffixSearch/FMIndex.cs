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
		public Bitmap newF;
		public Sequence seqIndex;
		public Bitmap SA_marked;
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

		public void Build (string sa_name, SequenceBuilder seq_builder = null)
        {
            using (var Input = new BinaryReader (File.OpenRead (sa_name + ".structs"))) {
                this.newF = GenericIO<Bitmap>.Load (Input);
                int len = (int)this.newF.Count1;
                this.charT = new int[len];
                PrimitiveIO<int>.LoadVector (Input, len, this.charT);
            }
            if (seq_builder == null) {
                // seq_builder = SequenceBuilders.GetWT_BinaryCoding(BitmapBuilders.GetRRR_wt(16));
                seq_builder = SequenceBuilders.GetSeqXLB_DiffSet64();
            }
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".bwt"))) {
				var L = new ListIFS ();
				L.Load (Input);
				this.seqIndex = seq_builder (L, this.charT.Length);
			}
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".samples"))) {
				this.SA_sample_step = Input.ReadInt16 ();
				this.SA_marked = GenericIO<Bitmap>.Load (Input);
				var _samples = new ListIFS ();
				_samples.Load (Input);
				var _invsamples = new ListIFS ();
				_invsamples.Load (Input);
				this.SA_samples = _samples;
				this.SA_invsamples = _invsamples;
			}
		}
		
		public long TextLength{
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
				GenericIO<Bitmap>.Save (Output, this.newF);
				PrimitiveIO<int>.SaveVector (Output, this.charT);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".bwt-index"))) {
				Console.WriteLine ("Saving bwt-index {0}", this.seqIndex);
				GenericIO<Sequence>.Save (Output, this.seqIndex);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".structs-samples"))) {
				Output.Write ((short)this.SA_sample_step);
				GenericIO<Bitmap>.Save (Output, this.SA_marked);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".samples"))) {
				this.SA_samples.Save (Output);
				this.SA_invsamples.Save (Output);
			}	
		}
		
		public void Load (string basename)
		{
			using (var Input = new BinaryReader (File.OpenRead (basename + ".structs"))) {
				this.newF = GenericIO<Bitmap>.Load (Input);
				this.charT = new int[this.newF.Count1];
				PrimitiveIO<int>.LoadVector (Input, this.charT.Length, this.charT);
			}
			// this.seqIndex = new WaveletTree ();
			// this.seqIndex.Load (Input);
			using (var Input = new BinaryReader (File.OpenRead (basename + ".bwt-index"))) {
				this.seqIndex = GenericIO<Sequence>.Load (Input);
			}
			using (var Input = new BinaryReader (File.OpenRead (basename + ".structs-samples"))) {
				this.SA_sample_step = Input.ReadInt16 ();
				this.SA_marked = GenericIO<Bitmap>.Load (Input);
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
			var sp = 0;
			var ep = (int)this.newF.Count - 1;
			for (int m = query.Count - 1; m >= 0; m--) {
				var c = this.GetCharId (query[m]);
				if (c < 0) {
					return false;
				}
				var abs_pos = (int)this.newF.Select1 (c + 1);
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
			int p = (int)this.newF.Select1 (c + 1) - 1;
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
				// Console.WriteLine ("i: {0}, L[i]: {1}, start_pos: {2}", i, L[i], start_pos);
				++start_pos;
				++i;
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
			int sample = (int)this.SA_samples[(int)this.SA_marked.Rank1 (suffix) - 1];
			return sample + power;
		}
		
		public IList<int> Extract (int start_pos, int len)
		{
			len = Math.Min (len, (int)this.N - start_pos);
//			var O = new int[len];
			// var O = new List<int>(len);
			var O = new int[len];
			int end_pos = start_pos + len - 1;
			int end_index = (int)Math.Ceiling ((end_pos+1) / ((float)this.SA_sample_step));
			int gap;
			if (end_index + 1 == this.SA_invsamples.Count) {
				gap = this.newF.Count - end_pos - 1;
			} else {
				gap = this.SA_sample_step - end_pos % this.SA_sample_step;
			}
			//int gap = this.SA_sample_step - end_pos % this.SA_sample_step;
			/*Console.WriteLine ("XXXX===> CEIL_SAMPLE: {0}, end_pos: {1}, diff: {2}",
			                   end_index * this.SA_sample_step, end_pos,
			                   end_index * this.SA_sample_step - end_pos
			                   );
			Console.WriteLine ("XXXX start_pos: {0}, end_pos: {1}, len: {2}, this.SA_sample_step: {3}, numinvsamples: {4}, gap: {5}",
			                   start_pos, end_pos, len, this.SA_sample_step, this.SA_invsamples.Count, gap);
			                   */
			//this._Extract (O, gap, this.SA_invsamples [end_index], len);
			// var end_index = end_pos / this.SA_sample_step;
			var start = this.SA_invsamples[ end_index ];
			for (int i = 0; i < gap; ++i) {
				start = this.LF (start);
			}
			for (int i = 0; i < len; i++) {
				if (start == 0) {
					break;
				}
				int num_char = this.newF.Rank1 (start);
				// O.Add (this.charT[num_char - 1]);
				O[ len - i - 1 ] = this.charT[num_char - 1];
				start = this.LF (start);
			}
			return O;
			// return this._Reverse(O);
		}
		
//		void _Extract (IList<int> O, int gap, int start, int len)
//		{
//		}
		
		IList<int> _Reverse (IList<int> O)
		{
			int mid = O.Count / 2;
			for (int left = 0; left < mid; left++) {
				var u = O[left];
				var right = O.Count - 1 - left;
				O[left] = O[right];
				O[right] = u;
			}
			return O;
		}
	}
}