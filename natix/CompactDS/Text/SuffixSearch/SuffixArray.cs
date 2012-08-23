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
//   Original filename: natix/CompactDS/Text/SuffixSearch/SuffixArray.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;
using natix.SortingSearching;

namespace natix.CompactDS
{	
	/// <summary>
	/// A simple and plain suffix-array
	/// </summary>
	public class SuffixArray
	{
		/// <summary>
		/// Data of the suffix array
		/// </summary>
		public IList<int> Text;
		public int[] SA;
		public IList<int> charT;
		public IRankSelect newF;
		
		public SuffixArray ()
		{
		}

		/// <summary>
		/// Suffix array built in
		/// </summary>
		public void Build (IList<int> text, int alphabet_size)
		{
			this.Text = text;
			var SS = new SuffixSorter (text, alphabet_size);
			SS.Sort ();
			this.SA = SS.SA;
			this.charT = SS.charT;
			this.newF = SS.newF;
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.SA.Length);
			PrimitiveIO<int>.WriteVector (Output, this.SA);
		}

		public void Save_CSA_BWT (string sa_name, int sample_step)
		{
			Console.WriteLine ("Save_CSA_BWT destroys the SA, if you need the plain SA");
			Console.WriteLine ("first save it and reload it after the call");
			using (var Output = new BinaryWriter (File.Create (sa_name + ".structs"))) {
				RankSelectGenericIO.Save (Output, this.newF);
				PrimitiveIO<int>.WriteVector (Output, this.charT);
			}
			using (var Output = new BinaryWriter (File.Create (sa_name + ".samples"))) {
				Output.Write ((short)sample_step);
				var B = new BitStream32 ();
				int numbits = (int)Math.Ceiling (Math.Log (this.SA.Length, 2));
				var SA_samples = new List<int> ();
				var SA_invsamples = new List<int> ();
				for (int i = 0; i < this.SA.Length; i++) {
					var s = this.SA[i];
					if ((s + 1 == this.SA.Length) || (s % sample_step == 0)) {
						B.Write (true);
						SA_samples.Add (s);
						SA_invsamples.Add (i);
					} else {
						B.Write (false);
					}
				}
				GGMN G = new GGMN ();
				G.Build (B, 8);
				RankSelectGenericIO.Save (Output, G);
				{
					var _SA_samples = new ListIFS (numbits);
					foreach (var u in SA_samples) {
						_SA_samples.Add (u);
					}
					_SA_samples.Save (Output);
				}
				{
					Sorting.Sort<int, int> (SA_samples, SA_invsamples);
					var _SA_invsamples = new ListIFS (numbits);
					foreach (var u in SA_invsamples) {
						_SA_invsamples.Add (u);
					}
					_SA_invsamples.Save (Output);
					SA_samples = null;
					SA_invsamples = null;
				}
			}
			// building bwt
			using (var Output = new BinaryWriter (File.Create (sa_name + ".bwt"))) {
				int alphabet_numbits = (int)Math.Ceiling (Math.Log (this.charT.Count + 1, 2));
				var L = new ListIFS (alphabet_numbits);
				int bwt_len = this.SA.Length;
				for (int i = 0; i < bwt_len; i++) {
					var v = this.SA[i];
					if (v == 0) {
						L.Add (0);
					} else {
						// Output.Write ("{0} ", (int)this.Text[v - 1]);
						var c = this.Text[v - 1];
						var u = GenericSearch.FindLast<int> (c, this.charT, 1, this.charT.Count);
						L.Add (u);
					}
				}
				L.Save (Output);
//				for (int i = 0; i < bwt_len; i++) {
//					var v = this.SA[i];
//					if (v == 0) {
//						Output.Write ("{0} ", -1);
//					} else {
//						// Output.Write ("{0} ", (int)this.Text[v - 1]);
//						var c = this.Text[v - 1];
//						var u = GenericSearch.FindLast<int> (c, this.charT);
//						Output.Write ("{0} ", u);
//					}
//				}
				// PrimitiveIO<byte>.WriteVector (Output, BWT);
			}
			// building psi
			using (var Output = new BinaryWriter (File.Create (sa_name + ".psi"))) {
				var INV = new int[this.SA.Length];
				for (int i = 0; i < INV.Length; i++) {
					INV[this.SA[i]] = i;
				}
				var PSI = this.SA;
				for (int i = 0; i < PSI.Length; i++) {
					var p = (PSI[i] + 1) % PSI.Length;
					PSI[i] = INV[p];
				}
				PrimitiveIO<int>.WriteVector (Output, PSI);
				/*Console.Write ("charT => ");
				for (int i = 0; i < this.charT.Count; i++) {
					Console.Write ("[{0}] ", (char)this.charT[i]);
				}
				Console.WriteLine ();
				Console.Write ("newF => ");
				for (int i = 0; i < this.newF.Count1; i++) {
					Console.Write ("{0} ", this.newF.Select1(i+1));
				}
				Console.WriteLine ();
				Console.Write ("PSI => ");
				for (int i = 0; i < PSI.Length; i++) {
					Console.Write ("{0} ", PSI[i]);
				}
				Console.WriteLine ();
				 */
				INV = null;
			}
			this.SA = null;
		}
		
		/// <summary>
		/// Load a suffix array from disk
		/// </summary>
		public void Load (BinaryReader Input)
		{
			int len = Input.ReadInt32 ();
			this.SA = new int[len];
			PrimitiveIO<int>.ReadFromFile (Input, len, this.SA);
		}
		
		/// <summary>
		/// Lower bound using binary search
		/// </summary>
		public void LowerBound (IList<int> query, int qstart, int qlen, int min, int max, out int lower)
		{
			int cmp = 0;
			int mid;
			int L;
			//Lower bound
			do {
				//mid = (min + max) / 2;
				mid = (min >> 1) + (max >> 1);
				int I = this.SA[mid];
				//L = Math.Min (I + query.Length, this.Data.Length);
				//cmp = this.Compare (query, 0, query.Length, this.Data, I, L);
				L = Math.Min (I + qlen, this.Text.Count);
				cmp = LexicographicCompare (query, qstart, qlen, this.Text, I, L);
				if (cmp == 0) {
					// lower limit
					max = mid;
					// upper limit
					// min = mid + 1;
				} else if (cmp > 0) {
					min = mid + 1;
				} else {
					max = mid;
				}
			} while (min < max);
			if (cmp > 0) {
				mid++;
			}
			lower = mid;
		}

		/// <summary>
		/// Upper bound using binary search
		/// </summary>
		public void UpperBound (IList<int> query, int qstart, int qlen, int min, int max, out int upper)
		{
			int cmp = 0;
			int mid;
			int L;
			//Lower bound
			do {
				// mid = (min + max) / 2;
				mid = (min >> 1) + (max >> 1);
				int I = this.SA[mid];
				//L = Math.Min (I + query.Length, this.Data.Length);
				//cmp = this.Compare (query, 0, query.Length, this.Data, I, L);
				L = Math.Min (I + qlen, this.Text.Count);
				cmp = LexicographicCompare (query, qstart, qlen, this.Text, I, L);

				if (cmp == 0) {
					// lower limit
					// max = mid;
					// upper limit
					min = mid + 1;
				} else if (cmp > 0) {
					min = mid + 1;
				} else {
					max = mid;
				}
			} while (min < max);
			if (cmp < 0) {
				mid--;
			}
			upper = mid;
		}
		
		/// <summary>
		/// Search for occurrences of a simple pattern
		/// </summary>
		public virtual int[] Search (IList<int> query)
		{
			int l, u;
			this.LowerBound (query, 0, query.Count, 0, this.Text.Count, out l);
			this.UpperBound (query, 0, query.Count, l, this.Text.Count, out u);
			if (l > u) {
				return null;
			}
			return new int[] { l, u };
		}
						
		/// <summary>
		/// Compare to arrays lexicographically, returns an integer representing something like a - b
		/// </summary>
		public static int LexicographicCompare (IList<int> a, int aStart, int aEnd, IList<int> b, int bStart, int bEnd)
		{
			int cmp = 0;
			for (int i = aStart, j = bStart; i < aEnd && j < bEnd; i++,j++) {
				cmp = a [i].CompareTo (b [j]);
				if (cmp != 0) {
					return cmp;
				}
			}
			return (aEnd - aStart) - (bEnd - bStart);
		}
	}
}
