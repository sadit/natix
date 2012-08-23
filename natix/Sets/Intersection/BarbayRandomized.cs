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
//   Original filename: natix/Sets/Intersection/BarbayRandomized.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	public class BarbayRandomized<T> : IIntersection<T> where T: IComparable
	{
		ISearchAlgorithm<T> SearchAlgorithm;
		public int CompCounter {
			get;
			set;
		}
		Random r = new Random ();
		
		public BarbayRandomized (ISearchAlgorithm<T> searchalg)
		{
			this.SearchAlgorithm = searchalg;
		}
		
		private void UnSortList(IList<int> I)
		{
			Sorting.Sort<int> (I, (int a, int b) => (this.r.Next () % 3) - 1);
		}
		
		public IList<T> Intersection (IList<IList<T>> postings)
		{
			int k = postings.Count;
			int[] L = new int[k];
			int[] I = new int[k];
			for (int i = 0; i < k; i++) {
				L[i] = 0;
				I[i] = i;
			}
			T e = postings[0][0];
			// i with another meaning
			int m = 1;
			int c = 1;
			var res = new List<T> ();
			var eliminatorBelongsTo = 0;
			while (true) {
				var i = I[m];
				// Console.WriteLine ("i: {0}, m: {1}", i, m);
				if (eliminatorBelongsTo == i) {
					goto end_while_increment_m;
				}
				var currPosition = L[i];
				var currList = postings[i];
				var doRestart = true;
				var doUnsort = false;
				if (currPosition >= currList.Count) {
					break;
				}
				if (this.SearchAlgorithm.Search (e, currList, out currPosition, currPosition, currList.Count)) {
					c++;
					currPosition++;
					if (c == k) {
						res.Add (e);
						doUnsort = true;
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
					eliminatorBelongsTo = i;
				} else {
					L[i] = currPosition;
				}
				if (doUnsort) {
					this.UnSortList (I);
				}
			end_while_increment_m:
				m++;
				if (m == k) {
					m = 0;
				}
			}
			
			this.CompCounter = this.SearchAlgorithm.CompCounter;
			return res;
		}
	}
}
