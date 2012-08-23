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
//   Original filename: natix/Sets/Intersection/InOrderTreeIntersection.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	public class InOrderTreeIntersection<T> : IIntersection<T> where T: IComparable
	{
		protected ISearchAlgorithm<T> SearchAlgorithm;
		public int CompCounter {
			get; set;
		}
		
		public InOrderTreeIntersection (ISearchAlgorithm<T> searchalg)
		{
			this.SearchAlgorithm = searchalg;
		}
		
		protected void InOrderI (IList<T> A, int startA, int endA, IList<T> B, int startB, int endB, IList<T> Out)
		{
			var sizeA = endA - startA;
			var sizeB = endB - startB;
			if (sizeA <= 0 || sizeB <= 0) {
				return;
			}
			if (sizeA > sizeB) {
				this._InOrderI (B, startB, endB, A, startA, endA, Out);
			} else {
				this._InOrderI (A, startA, endA, B, startB, endB, Out);
			}
		}
		
		void _InOrderI (IList<T> A, int startA, int endA, IList<T> B, int startB, int endB, IList<T> Out)
		{
			int midA = (startA + endA) >> 1;
			T   medA = A[midA];
			int posMedAinB;
			if (this.SearchAlgorithm.Search (medA, B, out posMedAinB, startB, endB)) {
				this.InOrderI (A, startA, midA, B, startB, posMedAinB, Out);
				Out.Add (medA);
				this.InOrderI (A, midA + 1, endA, B, posMedAinB + 1, endB, Out);
			} else {
				this.InOrderI (A, startA, midA, B, startB, posMedAinB, Out);
				this.InOrderI (A, midA + 1, endA, B, posMedAinB, endB, Out);
			}
		}

		public virtual IList<T> Intersection (IList< IList<T> > postings)
		{
			int k = postings.Count;
			Sorting.Sort< IList<T> >(postings, (IList<T> a, IList<T> b) => a.Count - b.Count);
			List<T> res = new List<T> ();
			List<T> tmp = null;
			List<T> swapAux = null;
			this.InOrderI(postings[0], 0, postings[0].Count, postings[1], 0, postings[1].Count, res);
			if (k > 2) {
				tmp = new List<T> ();
			}
			for (int i = 2; i < k; i++) {
				tmp.Clear();
				this.InOrderI (res, 0, res.Count, postings[i], 0, postings[i].Count, tmp);
				swapAux = res;
				res = tmp;
				tmp = swapAux;
			}
			
			this.CompCounter = this.SearchAlgorithm.CompCounter;
			return res;
		}

	}
}
