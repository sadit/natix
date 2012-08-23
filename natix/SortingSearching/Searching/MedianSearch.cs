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
//   Original filename: natix/SortingSearching/Searching/MedianSearch.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	
	public class MedianSearch<T> : ISearchAlgorithm<T> where T: IComparable
	{
		int compcounter = 0;
		ReverseDoublingSearch<T> BackwardSearch;
		DoublingSearch<T> ForwardSearch;
		
		public int CompCounter {
			get {
				return this.compcounter + this.BackwardSearch.CompCounter + this.ForwardSearch.CompCounter;
			}
		}

		public MedianSearch ()
		{
			this.BackwardSearch = new ReverseDoublingSearch<T> ();
			this.ForwardSearch = new DoublingSearch<T> ();
		}

		public bool Search (T data, IList<T> A, out int occPosition, int min, int max)
		{
			int mid = 0;
			int cmp;
			mid = (max + min) >> 1;
			this.compcounter++;
			cmp = data.CompareTo (A[mid]);
			if (cmp < 0) {
				return this.BackwardSearch.Search (data, A, out occPosition, min, mid);
			} else {
				if (cmp == 0) {
					occPosition = mid;
					return true;
				}
				return this.ForwardSearch.Search (data, A, out occPosition, mid + 1, max);
			}
		}
	}
}
