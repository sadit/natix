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
	public class CSA : ITextIndex
	{
		public int[] charT;
		public Bitmap newF;
		public IList<int> Psi;
		public Bitmap SA_marked;
		public ListIFS SA_samples;
		public ListIFS SA_invsamples;
		public short SA_sample_step;

		
		public CSA ()
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
		
		public void Build (string sa_name, ListIBuilder list_builder = null)
		{
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".structs"))) {
				this.newF = GenericIO<Bitmap>.Load (Input);
				int len = this.newF.Count1;
				this.charT = new int[len];
				// Console.WriteLine ("*****>> charT => {0} bytes", this.charT.Length * 4);
				PrimitiveIO<int>.LoadVector (Input, len, this.charT);
			}
			using (var Input = new BinaryReader (File.OpenRead (sa_name + ".psi"))) {
				var seq = PrimitiveIO<int>.LoadVector(Input, this.N+1, null);
				if (list_builder == null) {
					list_builder = ListIBuilders.GetListIDiffs(63);
				}
				this.Psi = list_builder(seq, seq.Count-1);
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
				
		public void Save (string basename)
		{
			using (var Output = new BinaryWriter (File.Create (basename + ".idx"))) {
				GenericIO<Bitmap>.Save (Output, this.newF);
				PrimitiveIO<int>.SaveVector (Output, this.charT);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".psi"))) {
				ListIGenericIO.Save(Output, this.Psi);
			}
			using (var Output = new BinaryWriter (File.Create (basename + ".samples"))) {
				Output.Write ((short)this.SA_sample_step);
				GenericIO<Bitmap>.Save (Output, this.SA_marked);
				this.SA_samples.Save (Output);
				this.SA_invsamples.Save (Output);
			}
		}
		
		public void Load (string basename)
		{
			using (var Input = new BinaryReader (File.OpenRead (basename + ".idx"))) {
				this.newF = GenericIO<Bitmap>.Load (Input);
				this.charT = new int[this.newF.Count1];
				PrimitiveIO<int>.LoadVector (Input, this.charT.Length, this.charT);
			}
			using (var Input = new BinaryReader (File.OpenRead (basename + ".psi"))) {
				this.Psi = ListIGenericIO.Load(Input);
			}
			using (var Input = new BinaryReader (File.OpenRead (basename + ".samples"))) {
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

		public IList<int> GetText (IList<int> Output)
		{
			int next = this.Psi[0];
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
				try {
					next = this.Psi[next];
				} catch (Exception e) {
					Console.WriteLine("=== next: {0}, len: {1}, num_char: {2}, psi-count: {3}", next, len, num_char, this.Psi.Count);
					Console.WriteLine("suffix_index: {0}", suffix_index);
					throw e;
				}
			}
			return Output;
		}
		
		public IEnumerable<int> GetSuffixLazy (int suffix_index, int len)
		{
			int next = suffix_index;
			for (int i = 0; i < len && next > 0; i++) {
				int num_char = this.newF.Rank1 (next);
				yield return this.charT[num_char - 1];
				next = this.Psi[next];
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
			int ep = sp + this.newF.Select1(c+2) - this.newF.Select1(c+1);
			for (--m; m >= 0; --m) {
				c = this.GetCharId (query [m]);
				if (c < 0) {
					return false;
				}
				int c_sp = this.newF.Select1(c + 1);
				int c_ep = this.newF.Select1(c + 2);
				var L = new ListGen<int>((int i) => this.Psi[i+c_sp], c_ep - c_sp);
				int abs_pos = c_sp - 1;
				sp = abs_pos + GenericSearch.FindFirst<int>(sp, L);
				ep = abs_pos + GenericSearch.FindLast<int>(ep, L);
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
				try {
					suffix = this.Psi[suffix];
				} catch (Exception e) {
					Console.WriteLine("=== power: {0}, suffix: {1}, psi.count: {2}", power, suffix, Psi.Count);
					throw e;
				}
			}
			int sample = (int)this.SA_samples[this.SA_marked.Rank1 (suffix) - 1];
			return sample - power;
		}
		
		public IList<int> Extract ( int start_pos, int len)
		{
			var Output = new List<int> ();
			int psi_start = (int)this.SA_invsamples [start_pos / this.SA_sample_step];
			for (int i = start_pos % this.SA_sample_step; i > 0; i--) {
				psi_start = this.Psi[psi_start];
			}
			for (int i = 0; i < len; i++) {
				int num_char = this.newF.Rank1 (psi_start);
				Output.Add (this.charT [num_char - 1]);
				psi_start = this.Psi[psi_start];
				if (psi_start == 0) {
					break;
				}
			}
			return Output;
		}
	}
}