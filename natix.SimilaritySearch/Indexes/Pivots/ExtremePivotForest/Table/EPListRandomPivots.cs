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
	public class EPListRandomPivots : EPList
	{			
		public EPListRandomPivots () : base()
		{
		}

		protected virtual void ComputeDistRow(DynamicSequentialOrdered idxseq, Random rand, HashSet<int> already_pivot, List<EPivot> pivs, List<ItemPair> _items)
		{
			_items.Clear ();
			int n = idxseq.DB.Count;
			int piv;
			do {
				piv = rand.Next(0, n);
			} while (already_pivot.Contains(piv));
			already_pivot.Add (piv);
			var stats = new DynamicSequential.Stats ();
			idxseq.ComputeDistances (idxseq.DB [piv], _items, out stats);
			int pivID = pivs.Count;
			pivs.Add(new EPivot(piv, stats.stddev, stats.mean, stats.min, stats.max, 0, 0));
			if (this.Items == null) {
				this.Items = new ItemPair[n];
				for (int objID = 0; objID < n; ++objID) {
					this.Items[objID] = new ItemPair(0, _items[objID].dist); 
				}
			} else {
				for (int objID = 0; objID < n; ++objID) {
					var new_piv = pivs[pivID];
					var new_dist = _items[objID].dist;
					var old_piv = pivs[ this.Items[objID].objID ];
					var old_dist = this.Items[objID].dist;
					if (Math.Abs(old_dist - old_piv.mean) < Math.Abs (new_dist - new_piv.mean)) {
						this.Items[objID] = new ItemPair(pivID, _items[objID].dist);
					}
				}
			}
		}

		public EPListRandomPivots (MetricDB DB, int seed, int num_pivots)
		{
			this.Items = null;
			var already_pivot = new HashSet<int> ();
			var pivs = new List<EPivot> (32);
			var rand = new Random (seed);
			var idxseq = new DynamicSequentialOrdered ();
			idxseq.Build (DB, RandomSets.GetIdentity (DB.Count));
			var tmp_items = new List<ItemPair> (DB.Count);

			for (int i = 0; i < num_pivots; ++i) {
				this.ComputeDistRow (idxseq, rand, already_pivot, pivs, tmp_items);
				double sum = 0;
				for (int objID = 0; objID < this.Items.Length; ++objID) {
					var u = this.Items[objID];
					sum += Math.Abs( u.dist - pivs[u.objID].mean );
				}
				if (i % 10 == 0) Console.WriteLine("XXXXXX {0} sum: {1}, i: {2}", this, sum, i);
			}
			this.Pivs = pivs.ToArray ();
			Console.WriteLine("Number of pivots per group: {0}", this.Pivs.Length);
		}
	}
}

