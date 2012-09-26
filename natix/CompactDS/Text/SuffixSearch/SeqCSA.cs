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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class SeqCSA : ITextIndex
	{
		public int[] charT;
		public IRankSelect newF;
		public IRankSelectSeq SeqPsi;
		public IRankSelect SA_marked;
		public ListIFS SA_samples;
		public ListIFS SA_invsamples;
		public short SA_sample_step;

		
		public SeqCSA ()
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
		
		int AlphabetSize {
			get {
				return this.charT.Length;
			}
		}
		
		public void Build (string sa_name, SequenceBuilder seq_builder = null, BitmapFromBitStream bitmap_builder = null)
		{
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqXLB_DiffSetRL2_64(16, 63);
			}
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".structs"))) {
				this.newF = RankSelectGenericIO.Load (Input);
				if (bitmap_builder != null) {
					var newF_stream = new BitStream32();
					for (int i = 0; i < this.newF.Count; ++i) {
						newF_stream.Write (this.newF.Access(i));
					}
					this.newF = bitmap_builder(new FakeBitmap(newF_stream));
				}
				int len = this.newF.Count1;
				this.charT = new int[len];
				// Console.WriteLine ("*****>> charT => {0} bytes", this.charT.Length * 4);
				PrimitiveIO<int>.ReadFromFile (Input, len, this.charT);
			}
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".psi"))) {
				int seqlen = this.newF.Count;
				var seq = new int[seqlen];
				var L = new List<int>(this.N/this.Sigma + 1);
				int curr = 0;
				for (int i = 1; i <= this.AlphabetSize; i++) {
					int next;
					if (i == this.AlphabetSize) {
						next = this.newF.Count;
					} else {
						next = this.newF.Select1 (i + 1);
					}
					int len = next - curr;
					L.Clear();
					PrimitiveIO<int>.ReadFromFile (Input, len, L);
					for (int j = 0; j < len; ++j) {
						var x = L[j];
						try {
							seq[ x ] = i - 1;
						} catch (Exception e) {
							Console.WriteLine ("== i: {0}, j: {1}, x: {2}, seq-count: {3}, len: {4}",
							                   i, j, x, seq.Length, len);
							throw e;
						}
					}
					curr = next;
				}
				this.SeqPsi = seq_builder(seq, this.AlphabetSize);
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
				
		public void Save (string basename)
		{
			using (var Output = new BinaryWriter (File.Create (basename + ".idx"))) {
				RankSelectGenericIO.Save (Output, this.newF);
				PrimitiveIO<int>.WriteVector (Output, this.charT);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".psi"))) {
				RankSelectSeqGenericIO.Save(Output, this.SeqPsi);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".samples"))) {
				Output.Write ((short)this.SA_sample_step);
				RankSelectGenericIO.Save (Output, this.SA_marked);
				this.SA_samples.Save (Output);
				this.SA_invsamples.Save (Output);
			}
		}
		
		public void Load (string basename)
		{
			using (var Input = new BinaryReader (File.OpenRead (basename + ".idx"))) {
				this.newF = RankSelectGenericIO.Load (Input);
				this.charT = new int[this.newF.Count1];
				PrimitiveIO<int>.ReadFromFile (Input, this.charT.Length, this.charT);
			}
			using (var Input = new BinaryReader (File.OpenRead (basename + ".psi"))) {
				this.SeqPsi = RankSelectSeqGenericIO.Load (Input);
			}
			using (var Input = new BinaryReader (File.OpenRead (basename + ".samples"))) {
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

		public IList<int> GetText (IList<int> Output)
		{
			int next = this.SeqPsi.Select(0,1);
			this.GetSuffix (Output, next, this.newF.Count - 1);
			return Output;
		}
		
		public IList<int> GetSuffix (IList<int> Output, int suffix_index, int len)
		{
			int next = suffix_index;
			for (int i = 0; i < len && next > 0; i++) {
				int num_char = this.newF.Rank1 (next);
				// Console.WriteLine ("i: {0}, num_char: {1}, next: {2}, sigma: {3}", i, num_char, next, this.charT.Length);
				Output.Add (this.charT[num_char - 1]);
				next = this.SeqPsi.Select(num_char - 1, next - this.newF.Select1 (num_char) + 1);
			}
			return Output;
		}
		
		public IEnumerable<int> GetSuffixLazy (int suffix_index, int len)
		{
			int next = suffix_index;
			for (int i = 0; i < len && next > 0; i++) {
				int num_char = this.newF.Rank1 (next);
				yield return this.charT[num_char - 1];
				next = this.SeqPsi.Select(num_char - 1, next - this.newF.Select1 (num_char) + 1);
			}
		}
		
		
		IList<int> _get_suffix_lazy (int suffix)
		{
			var E = this.GetSuffixLazy (suffix, this.N).GetEnumerator ();
			E.MoveNext ();
			// E.Reset ();
			var L = new ListGen<int> ((int x) =>
			{
				var u = E.Current;
				E.MoveNext ();
				return u;
			}, this.N);
			L.FinalizeInstance = () =>
			{
				E.Dispose ();
				return 0;
			};
			return L;
		}
		
		int _lex_cmp (IList<int> a, IList<int> b)
		{
			int cmp = 0;
			int i = 0;
			int j = 0;
			for (; i < a.Count && j < b.Count; i++,j++) {
				cmp = a[i].CompareTo (b[j]);
				if (cmp != 0) {
					return cmp;
				}
			}
			return 0;
			// return a.Count - b.Count;
		}
		
		int GetCharId (int c)
		{
			int e = GenericSearch.FindLast<int> (c, this.charT);
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
			// backward search
			end_pos = start_pos = -1;
			int m = query.Count - 1;
			var c = this.GetCharId (query [m]);
			if (c < 0) {
				return false;
			}
			int sp = this.newF.Select1 (c + 1);
			int ep = sp + this.SeqPsi.Rank(c, this.SeqPsi.Count - 1) - 1;
			for (--m; m >= 0; --m) {
				c = this.GetCharId (query [m]);
				if (c < 0) {
					return false;
				}
				var L = this.SeqPsi.Unravel(c);
				int abs_pos = this.newF.Select1 (c + 1);
				if (L.Count <= sp) {
					sp = L.Count - 1;
				}
				if (L.Count <= ep) {
					ep = L.Count - 1;
				}
				sp = abs_pos + L.Rank1 (sp - 1);
				ep = abs_pos + L.Rank1 (ep) - 1;
			}
			start_pos = sp;
			end_pos = ep;
			return true;
		}
		
//		public bool ForwardSearch (IList<int> query, out int start_pos, out int end_pos)
//		{
//			// WARNING: there is a possible bug in _lex_cmp, the result is wrong when some suffix is smaller than the query
//			// but anyway this approach should be avoided in favor of Backward
//			var L = new ListGen<IList<int>> ((int suffix_id) => {
//				return this._get_suffix_lazy (suffix_id);
//			}, this.newF.Count);
//			start_pos = GenericSearch.FindFirst<IList<int>> (query, L, 0, this.newF.Count, this._lex_cmp);
//			end_pos = GenericSearch.FindLast<IList<int>> (query, L, start_pos, this.newF.Count, this._lex_cmp);
//			return true;
//		}
	
		public int CountOcc (IList<int> query)
		{
			int start_pos;
			int end_pos;
			if (this.Search (query, out start_pos, out end_pos)) {
				return end_pos - start_pos + 1;
			} else {
				return 0;
			}
		}
		
		public int GetPsi (int pos)
		{
			int num_char = this.newF.Rank1 (pos);
			return this.SeqPsi.Select(num_char - 1, pos - this.newF.Select1 (num_char) + 1);
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
				suffix = this.GetPsi (suffix);
			}
			int sample = (int)this.SA_samples[this.SA_marked.Rank1 (suffix) - 1];
			return sample - power;
		}
		
		public IList<int> Extract ( int start_pos, int len)
		{
			var Output = new List<int> ();
			int psi_start = (int)this.SA_invsamples [start_pos / this.SA_sample_step];
			for (int i = start_pos % this.SA_sample_step; i > 0; i--) {
				psi_start = this.GetPsi (psi_start);
			}
			for (int i = 0; i < len; i++) {
				int num_char = this.newF.Rank1 (psi_start);
				Output.Add (this.charT [num_char - 1]);
				psi_start = this.SeqPsi.Select(num_char - 1, psi_start - this.newF.Select1 (num_char) + 1);
				if (psi_start == 0) {
					break;
				}
			}
			return Output;
		}
	}
}