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

namespace natix.Sets
{
	/// <summary>
	/// NTT array. Numerical t-threshold algorithm. Based on array
	/// </summary>
	
	public class NTTArray8 : ITThresholdAlgorithm
	{
		int N;
		bool sorted_output;

		public NTTArray8 (int N, bool sorted_output)
		{
			this.N = N;
			this.sorted_output = sorted_output;
		}

		public NTTArray8 (int N) : this(N, false)
		{
		}
		
		public NTTArray8 () : this (-1, false)
		{
		}

		public int CompCounter {
			get {
				return 0;
			}
		}
		
		int GetMax (IList<IList<int>> lists)
		{
			int n = 1;
			foreach (var L in lists) {
				var c = L.Count;
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

		public void SearchTThreshold (IList<IList<int>> lists, int MinT, out IList<int> docs, out IList<short> cardinalities)
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
			docs = new int[ output_set.Count ];
			cardinalities = new short[ docs.Count ];
			int i = 0;
			foreach (var item in output_set) {
				docs [i] = item;
				cardinalities [i] = L [item];
				i++;
			}
			if (this.sorted_output) {
				Sorting.Sort<int,short> (docs, cardinalities);
			}
		}
	}
}

