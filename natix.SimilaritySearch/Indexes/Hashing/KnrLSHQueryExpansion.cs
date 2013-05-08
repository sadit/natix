// 
//  Copyright 2012  sadit
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
	public class KnrLSHQueryExpansion : KnrLSH
	{
		public KnrLSHQueryExpansion () : base()
		{
		}

		public KnrLSHQueryExpansion (KnrLSH other) : base()
		{
			this.DB = other.DB;
			this.K = other.K;
			this.R = other.R;
			this.TABLE = other.TABLE;
		}

		public virtual IEnumerable<int> ExpandHashKnr (object q)
		{
			this.internal_numdists-=this.R.Cost.Internal;
			var near = this.R.SearchKNN(q, this.K);
			var list = new List<int> (Fun.Map<ResultPair,int>(near, (pair) => pair.docid ));
			this.internal_numdists+=this.R.Cost.Internal;
			var max_pos = list.Count - 1;
			var first = this.EncodeKnr(list);
			yield return first;
			for (int i = 0; i < list.Count; ++i) {
				for (int j = 0; j < max_pos; ++j) {
					swap (j, list);
					var h = this.EncodeKnr(list);
					if (h != first) {
						yield return h;
					}
				}
			}
		}

		void swap(int i, List<int> list)
		{
			var item = list[i];
			list [i] = list [i + 1];
			list [i + 1] = item;
		}

		public override HashSet<int> GetNear(object q)
		{
			var near = new HashSet<int>();
			foreach (var hash in this.ExpandHashKnr(q)) {
				Console.WriteLine ("=== hash {0}", hash);
				List<int> L;
				if (this.TABLE.TryGetValue(hash, out L)) {
					near.UnionWith(L);
				}
			}
			return near;
		}
	}
}