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
//   Original filename: natix/Sets/Intersection/SmallAdaptive.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	public class SmallAdaptive<T> : IIntersection<T> where T: IComparable
	{
		ISearchAlgorithm<T> SearchAlgorithm;
		public int CompCounter {
			get;
			set;
		}
		public SmallAdaptive (ISearchAlgorithm<T> SearchAlgorithm)
		{
			this.SearchAlgorithm = SearchAlgorithm;
		}
		
		public IList<T> Intersection (IList<IList<T>> postings)
		{
			var L = new int[postings.Count];
			Comparison<int> SortByLength = delegate(int a, int b) {
				int aL = (postings[a].Count - L[a]);
				int bL = (postings[b].Count - L[b]);
				int cmp = aL - bL;
				if (cmp == 0) {
					return a - b;
				}
				return cmp;
			};
			int[] I = new int[L.Length];
			for (int i = 0; i < L.Length; i++) {
				L[i] = 0;
				I[i] = i;
			}
			List<T> res = new List<T> ();
			while (true) {
				Sorting.Sort<int> (I, SortByLength);
				int k = 1;
				int smallest = I[0];
				//Console.WriteLine ("sL: {0}, pL: {1}", L[smallest], postings[smallest].Length);
				if (L[smallest] == postings[smallest].Count) {
					break;
				}
				var e = postings[smallest][L[smallest]];
				for (int i = 1; i < L.Length; i++) {
					var currIndex = I[i];
					var currLen = L[currIndex];
					var currList = postings[currIndex];
					if (this.SearchAlgorithm.Search (e, currList, out currLen, currLen, currList.Count)) {
						k++;
						L[currIndex] = currLen + 1;
					} else {
						break;
					}
				}
				// this can be used to retrieve the t-threshold modifying the previous
				// if statement to provide the desired behaviour
				if (k == L.Length) {
					res.Add (e);
				}
				L[smallest]++;
			}
			this.CompCounter = this.SearchAlgorithm.CompCounter;			
			return res;
		}

	}
}

