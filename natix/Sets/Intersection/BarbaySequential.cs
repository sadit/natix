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
//   Original filename: natix/Sets/Intersection/BarbaySequential.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	public class BarbaySequential<T> : IIntersection<T> where T: IComparable
	{
		ISearchAlgorithm<T> SearchAlgorithm;
		public int CompCounter {
			get;
			set;
		}
		
		public BarbaySequential (ISearchAlgorithm<T> searchalg)
		{
			this.SearchAlgorithm = searchalg;
		}
		
		public IList<T> Intersection (IList<IList<T>> postings)
		{
			int k = postings.Count;
			int[] L = new int[k];
			int i;
			for (i = 0; i < k; i++) {
				L[i] = 0;
			}
			T e = postings[0][0];
			// i with another meaning
			i = 1;
			int c = 1;
			L[0] = 1;
			var res = new List<T> ();
			while (true) {
				var currPosition = L[i];
				var currList = postings[i];
				var doRestart = true;
				if (currPosition >= currList.Count) {
					break;
				}
				if (this.SearchAlgorithm.Search (e, currList, out currPosition, currPosition, currList.Count)) {
					c++;
					currPosition++;
					if (c == k) {
						res.Add (e);
					} else {
						doRestart = false;
						// L[i] = currPosition is performed below (in the else statement)
					}
				}
				if (doRestart) {
					c = 1;
					if (currPosition >= currList.Count) {
						break;
					}
					e = currList[currPosition];
					currPosition++;
					L[i] = currPosition;
				} else {
					L[i] = currPosition;
				}
				//if (currPosition >= currList.Count) {
				// break;
				// }
				i++;
				if (i == k) {
					i = 0;
				}
				// Console.WriteLine ("===> e: {0}, currList.Len: {1}, c: {2}, i: {3}, k: {4}, currPosition: {5}", e, currList.Length.ToString ().PadLeft (5), c, i, k, currPosition);
			}
			
			this.CompCounter = this.SearchAlgorithm.CompCounter;
			return res;
		}
	}
}
