// 
//  Copyright 2012-2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.IO;
using System.Collections.Generic;
using natix.CompactDS;
using natix.Sets;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class AdaptiveNeighborhoodHash: MultiNeighborhoodHash
	{
		public AdaptiveNeighborhoodHash (MultiNeighborhoodHash m) : base()
		{
			this.A = m.A;
			this.DB = m.DB;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var evaluated = new HashSet<int> ();
			var exp = A [0].NeighborhoodExpansion - A[0].SymbolsPerHash + 1;
			var arrayOfRankedList = new List<int>[A.Length];
			for (int i = 0; i < A.Length; ++i) {
				arrayOfRankedList [i] = A [i].RankRefs (q);
			}

			for (int start = 0; start < exp; ++start) {
				double prev = res.CoveringRadius;
				for (int i = 0; i < A.Length; ++i) {
					var idx = A [i];
					var hashList = idx.FetchPostingLists (arrayOfRankedList[i], start);
					idx.InternalSearchKNN (q, res, hashList, evaluated);
				}
				var curr = res.CoveringRadius;
				if (prev == curr) {
					break;
				}
			}
			return res;
		}
	}
}