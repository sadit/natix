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
//   Original filename: natix/SortingSearching/Searching/ReverseDoublingSearch.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class ReverseDoublingSearch<T> : ISearchAlgorithm<T> where T: IComparable
	{
		public short SkipFactor = 1;
		public int SkipFirst = 0;
		public ISearchAlgorithm<T> SecondSearch;
		int compcounter = 0;
		public int CompCounter {
			get {
				return this.compcounter + this.SecondSearch.CompCounter;
			}
		}
		
		public ReverseDoublingSearch ()
		{
			this.SecondSearch = new BinarySearch<T> ();
		}
		
		public ReverseDoublingSearch (int skipFirst, short skipFactor, ISearchAlgorithm<T> secondSearch)
		{
			this.SkipFirst = skipFirst;
			this.SkipFactor = skipFactor;
			this.SecondSearch = secondSearch;
		}
		
		public bool Search (T data, IList<T> A, out int occPosition, int min, int max)
		{
			int skipFactor = this.SkipFactor;
			int pos = max - 1 - this.SkipFirst;
			int currentSkip = 0;
			int skipCounter = 0;
			int cmp;
			do {
				pos = Math.Max (min, pos - currentSkip);
				this.compcounter++;
				cmp = data.CompareTo(A[pos]);
				if (cmp > 0) {
					return this.SecondSearch.Search (data, A, out occPosition, pos, max);
				}
				if (cmp == 0) {
					occPosition = pos;
					return true;
				}
				max = pos;
				currentSkip = skipFactor << skipCounter;
				skipCounter++;
			} while (pos > min);
			occPosition = max;
			return false;
		}
	}
}
