//
//  Copyright 2013     Eric Sadit Tellez Avila
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
// 

using System;
using System.IO;
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class EPListMeanPivots : EPList
	{			
		public EPListMeanPivots () : base()
		{
		}

		protected virtual DynamicSequential.Stats ComputeDistRow(int piv, DynamicSequentialOrdered idxseq, Random rand, List<EPivot> pivs, List<ItemPair> _items)
		{
			_items.Clear ();
			int n = idxseq.DB.Count;
			var stats = new DynamicSequential.Stats ();
			idxseq.ComputeDistances (idxseq.DB [piv], _items, out stats);
			int pivID = pivs.Count;
			pivs.Add(new EPivot(piv, stats.stddev, stats.mean, stats.min, stats.max, 0, 0));
			if (this.Items == null) {
				this.Items = new ItemPair[n];
				for (int objID = 0; objID < n; ++objID) {
					this.Items[objID] = new ItemPair(0, _items[objID].Dist); 
				}
			} else {
				for (int objID = 0; objID < n; ++objID) {
					var new_piv = pivs[pivID];
					var new_dist = _items[objID].Dist;
					var old_piv = pivs[ this.Items[objID].ObjID ];
					var old_dist = this.Items[objID].Dist;
					if (Math.Abs(old_dist - old_piv.mean) < Math.Abs (new_dist - new_piv.mean)) {
						this.Items[objID] = new ItemPair(pivID, _items[objID].Dist);
					}
				}
			}
			return stats;
		}

		public EPListMeanPivots (MetricDB DB, int seed, int num_pivs)
		{
			this.Items = null;
			var pivs = new List<EPivot> (32);
			var rand = new Random (seed);
			var n = DB.Count;
			var idxseq = new DynamicSequentialOrdered ();
			idxseq.Build (DB, RandomSets.GetIdentity (DB.Count));
			var tmp_items = new List<ItemPair> (DB.Count);
			int next_piv = rand.Next (0, n);
			for (int i = 0; i < num_pivs; ++i) {
				var varX = 0.0;
				double min_diff = double.MaxValue;
				this.ComputeDistRow (next_piv, idxseq, rand, pivs, tmp_items);
				for (int objID = 0; objID < this.Items.Length; ++objID) {
					var u = this.Items [objID];
					var diff = Math.Abs (u.Dist - pivs [u.ObjID].mean);
					if (diff < min_diff) {
						min_diff = diff;
						next_piv = objID;
					}
					varX += diff * diff / n;
				}
				++i;

				Console.WriteLine ("XXXXXX i: {0}, variance: {1}", i, varX);
			}
			this.Pivs = pivs.ToArray ();
			Console.WriteLine("Number of pivots per group: {0}", this.Pivs.Length);
		}

	}
}

