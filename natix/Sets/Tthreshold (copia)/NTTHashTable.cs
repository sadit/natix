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
//   Original filename: natix/Sets/Tthreshold/NTTHashTable.cs
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
	
	public class NTTHashTable : ITThresholdAlgorithm
	{
		bool sorted_output;
		public NTTHashTable (bool sorted_output)
		{
			this.sorted_output = sorted_output;
		}
		
		public NTTHashTable (): this(false)
		{
		}
		
		public int CompCounter {
			get {
				return 0;
			}
		}
		
		/// <summary>
		/// Returns items in at least MinT lists. The output is not necessarily sorted.
		/// </summary>

		public void SearchTThreshold (IList<IList<int>> lists, int MinT, out IList<int> docs, out IList<short> cardinalities)
		{
			var L = new Dictionary<int,short> ();
			var output_set = new HashSet<int> ();
			int i = 0;
			foreach (var list in lists) {
				foreach (var item in list) {
					short counter;
					if (L.TryGetValue (item, out counter)) {
						counter++;
					} else {
						/*if (MinT < i) {
							continue;
						}*/
						counter = 1;
					}
					if (counter == MinT) {
						output_set.Add (item);
					}
					L [item] = counter;
				}
				i++;
			}
			docs = new int[ output_set.Count ];
			cardinalities = new short[ docs.Count ];
			i = 0;
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

