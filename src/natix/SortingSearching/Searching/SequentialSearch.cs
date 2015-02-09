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
//   Original filename: natix/SortingSearching/Searching/SequentialSearch.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	
	public class SequentialSearch<T>: ISearchAlgorithm<T> where T: IComparable 
	{
		protected int Counter = 0;
		public int CompCounter {
			get {
				return this.Counter;
			}
		}
		
		public SequentialSearch ()
		{
			
		}
		
		public virtual bool Search (T data, IList<T> A, out int occPosition, int min, int max)
		{
			for (; min < max; min++) {
				this.Counter++;
				int cmp = data.CompareTo (A[min]);
				if (cmp <= 0) {
					if (cmp == 0) {
						occPosition = min;
						return true;
					} else {
						break;
					}
				}
			}
			occPosition = min;
			return false;
		}
	}
}
