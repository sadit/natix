//
//   Copyright 2013 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
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
using System;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.SortingSearching
{
	public partial class Search
	{
		/// <summary>
		/// Find the last entry u where u <= query. It returns min-1 if query < L[u] for every L[u] in the array.
		/// </summary>
		public static int FindLast (int query, ListIFS data)
		{
			return FindLast (query, data, 0, data.Count);
		}
		
		public static int FindFirst (int query, ListIFS data)
		{
			return FindFirst (query, data, 0, data.Count);
		}

		/// <summary>
		/// Find the last u where L[u] <= query. It returns min-1 if query < L[u] for every L[u] in the array.
		/// </summary>
		public static int FindLast (int query, ListIFS data, int min, int max)
		{
			int cmp = 0;
			int mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if ((min & max & 1) == 1) {
					mid++;
				}
				cmp = query.CompareTo( data[mid] );
				if (cmp >= 0) {
					min = mid + 1;
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
		/// Finds u such that data[u] <= query, if data[u] is duplicated, then the first entry is retrieved.
		/// </summary>
		public static int FindFirst (int query, ListIFS data, int min, int max)
		{
			//int _min = min;
			//int _max = max;
			int cmp = 0;
			int mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if ((min & max & 1) == 1) {
					mid++;
				}
				cmp = query.CompareTo( data[mid] );
				if (cmp > 0) {
					min = mid + 1;
				} else {
					max = mid;
				}
				// Console.WriteLine ("===== min: {0}, mid: {1}, max: {2}, data[mid]: {3}", min, mid, max, data[mid]);
			} while (min < max);
			if (cmp < 0) {
				mid--;
			} else if (cmp > 0) {
				if (mid < max) {
					if (mid + 1 < data.Count && query.CompareTo( data[mid + 1] ) == 0) {
						mid++;
					}
				}
			}
			return mid;
		}

	}
}
