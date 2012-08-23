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
//   Original filename: natix/Sets/Intersection/SvS.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	public class SvS<T> : IIntersection<T> where T : IComparable
	{
		ISearchAlgorithm<T> SearchAlgorithm;
		public int CompCounter {
			get;
			set;
		}
		public SvS (ISearchAlgorithm<T> SearchAlgorithm)
		{
			this.SearchAlgorithm = SearchAlgorithm;
		}
		
		protected void SvS_Fast (IList<T> A, IList<T> B, IList<T> Out)
		{
			Out.Clear ();
			for (int a = 0, b = 0; a < A.Count && b < B.Count; a++) {
				if (this.SearchAlgorithm.Search (A[a], B, out b, b, B.Count)) {
					Out.Add (A[a]);
					b++;
				}
			}
			this.CompCounter = this.SearchAlgorithm.CompCounter;
		}

		public IList<T> Intersection (IList<IList<T>> postings)
		{
			var pLen = postings.Count;
			Sorting.Sort< IList<T> >(postings, (IList<T> a, IList<T> b) => a.Count - b.Count);
			// postings.Sort ((int[] a, int[] b) => a.Length - b.Length);
			List<T> res = new List<T> ();
			List<T> tmp = null;
			List<T> swaptmp = null;
			// we always have at least two lists (see Search)
			this.SvS_Fast (postings[0], postings[1], res);
			if (pLen > 2) {
				tmp = new List<T> ();
			}
			for (int i = 2; i < pLen; i++) {
				this.SvS_Fast (res, postings[i], tmp);
				swaptmp = res;
				res = tmp;
				tmp = swaptmp;
			}
			return res;
		}
	}
}
