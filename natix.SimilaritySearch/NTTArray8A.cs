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
//   Original filename: natix/Sets/Tthreshold/NTTArray8.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// NTT array. Numerical t-threshold algorithm. Based on array
	/// </summary>
	
	public class NTTArray8A 
	{
		int N;

		public NTTArray8A (int N)
		{
			this.N = N;
		}

		public NTTArray8A () : this (-1)
		{
		}

		public int CompCounter {
			get {
				return 0;
			}
		}
		
		int GetMax (IList<int[]> lists)
		{
			int n = 1;
			foreach (var L in lists) {
				var c = L.Length;
				if (c > 0) {
					var maxL = L [c - 1];
					if (n <= maxL) {
						n = maxL + 1;
					}
				}
			}
			return n;
		}
		/// <summary>
		/// Returns items in at least MinT lists. The output is not necessarily sorted.
		/// </summary>

		public void SearchTT (IList<int[]> lists, int MinT, IResult cand)
		{
			int n = this.N;
			if (n < 0) {
				n = this.GetMax (lists);
			}
			var L = new byte[n];
			var output_set = new HashSet<int> ();
			foreach (var list in lists) {
				foreach (var item in list) {
					L [item]++;
					if (L [item] == MinT) {
						output_set.Add (item);
					}
				}
			}
			foreach (var item in output_set) {
				cand.Push(item, -L[item]);
			}
		}
	}
}

