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
//   Original filename: natix/SortingSearching/Searching/BinarySearch.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	
	public class BinarySearch<T> : ISearchAlgorithm<T> where T: IComparable
	{
		public int CompCounter { get; set; }

		public BinarySearch () 
		{
		}

		public bool Search (T data, IList<T> A, out int occPosition, int min, int max)
		{
			if (max <= min) {
				occPosition = max;
				return false;
			}
			int mid = 0;
			int cmp;
			do {
				// mid = (max >> 1) + (min >> 1);
				mid = (max + min) >> 1;
				this.CompCounter++;
				cmp = data.CompareTo (A[mid]);
				if (cmp < 0) {
					max = mid;
				} else {
					if (cmp == 0) {
						occPosition = mid;
						return true;
					}
					min = mid + 1;
				}
			} while (min < max);
			occPosition = max;
			return false;
		}
	}
}
