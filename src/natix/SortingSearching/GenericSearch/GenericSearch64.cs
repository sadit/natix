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
//   Original filename: natix/SortingSearching/GenericSearch64.cs
// 
using System;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class GenericSearch64
	{
		/// <summary>
		/// Given a sorted array, it finds the last entry u where u <= query.
		/// It returns min-1 if u > query for every u in the array.
		/// </summary>
		public static long FindLast<T> (T query, ListGenerator64<T> data) where T: IComparable
		{
			return FindLast<T> (query, data, 0, data.Count);
		}

		public static long FindFirst<T> (T query, ListGenerator64<T> data) where T : IComparable
		{
			return FindFirst<T> (query, data, 0, data.Count);
		}

		/// <summary>
		/// Find the last entry u where u <= query. It returns min-1 if u > query for every u in the array.
		/// </summary>

		public static long FindLast<T> (T query, ListGenerator64<T> data, long min, long max) where T : IComparable
		{
			return FindLast<T> (query, data, min, max, (x, y) => x.CompareTo (y));
		}
		
		public static long FindFirst<T> (T query, ListGenerator64<T> data, long min, long max) where T : IComparable
		{
			return FindFirst<T> (query, data, min, max, (x, y) => x.CompareTo (y));
		}

		/// <summary>
		/// Find the last entry u where u <= query. It returns min-1 if u > query for every u in the array.
		/// </summary>
		public static long FindLast<T> (T query, ListGenerator64<T> data, Comparison<T> cmpfun)
		{
			return FindLast<T> (query, data, 0, data.Count, cmpfun);
		}
		
		public static long FindFirst<T> (T query, ListGenerator64<T> data, Comparison<T> cmpfun)
		{
			return FindFirst<T> (query, data, 0, data.Count, cmpfun);
		}

		/// <summary>
		/// Find the last u where L[u] <= query. It returns min-1 if u > query for every u in the array.
		/// </summary>
		public static long FindLast<T> (T query, ListGenerator64<T> data, long min, long max, Comparison<T> cmpfun)
		{
			int cmp = 0;
			long mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if ((min & max & 1L) == 1L) {
					mid++;
				}
				cmp = cmpfun (query, data [mid]);
				if (cmp >= 0) {
					min = mid + 1L;
				} else {
					max = mid;
				}
				// Console.WriteLine ("===== min: {0}, mid: {1}, max: {2}, data[mid]: {3}", min, mid, max, data[mid]);
			} while (min < max);
			if (cmp < 0) {
				mid--;
			}
			return mid;
		}
		
		/// <summary>
		/// Finds u such that data[u] <= query, if u has repetitions, then the first entry is retrieved.
		/// </summary>
		public static long FindFirst<T> (T query, ListGenerator64<T> data, long min, long max, Comparison<T> cmpfun)
		{
			//int _min = min;
			//int _max = max;
			int cmp = 0;
			long mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if ((min & max & 1L) == 1L) {
					mid++;
				}
				cmp = cmpfun (query, data[mid]);
				if (cmp > 0) {
					min = mid + 1L;
				} else {
					max = mid;
				}
				// Console.WriteLine ("===== min: {0}, mid: {1}, max: {2}, data[mid]: {3}", min, mid, max, data[mid]);
			} while (min < max);
			if (cmp < 0) {
				mid--;
			} else if (cmp > 0) {
				if (mid < max) {
					if (mid + 1L < data.Count && cmpfun (query, data[mid + 1L]) == 0) {
						mid++;
					}
				}
			}
			return mid;
		}
	}
}
