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
	public class EPListOptimizedB : EPList
	{			
		public EPListOptimizedB () : base()
		{
		}

		protected virtual DynamicSequential.Stats ComputeDistRow(int piv, DynamicSequentialOrdered idxseq, Random rand, List<EPivot> pivs, List<ItemPair> _items)
		{
			_items.Clear ();
			int n = idxseq.DB.Count;
			var stats = new DynamicSequential.Stats ();
			idxseq.ComputeDistances (idxseq.DB [piv], _items, out stats);
			int pivID = pivs.Count;
			pivs.Add (new EPivot (piv, stats.stddev, stats.mean, stats.min, stats.max, 0, 0));

			if (this.Items == null) {
				this.Items = new ItemPair[n];
				for (int objID = 0; objID < n; ++objID) {
					this.Items [objID] = new ItemPair (0, _items [objID].Dist); 
				}
			} else {
				for (int objID = 0; objID < n; ++objID) {
					var new_piv = pivs [pivID];
					var new_dist = _items [objID].Dist;
					var old_piv = pivs [this.Items [objID].ObjID];
					var old_dist = this.Items [objID].Dist;
					if (Math.Abs (old_dist - old_piv.mean) < Math.Abs (new_dist - new_piv.mean)) {
						this.Items [objID] = new ItemPair (pivID, _items [objID].Dist);
					}
				}
			}
			return stats;
		}

		public EPListOptimizedB (MetricDB DB, int num_indexes, Random rand)
		{
			this.Items = null;
			var pivs = new List<EPivot> (32);
			var n = DB.Count;
			var idxseq = new DynamicSequentialOrdered ();
			idxseq.Build (DB, RandomSets.GetIdentity (DB.Count));
			var tmp_items = new List<ItemPair> (DB.Count);
			double qrad;
			double varY;
			double mean;
			PivotSelector.EstimateQueryStatistics (DB, rand, 64, 128, out mean, out varY, out qrad);
			//double prev_cost = -1;
			//double curr_cost = n;

			var pivsel = new PivotSelectorRandom (n, rand);

			double weight_prev = 0.99;
			double weight_curr = 1.0 - weight_prev;

			double max_error = 0.01;
			double error = 1;
			double prev_cost = 1.0;

			double min_cost = 1;

			// anything larger than 1.x can be considered a valid starting error_factor

			var iterID = 0;
			var window = 1;

			while (true) {
				//++iterID;

				double curr_cost = 0;
				for (int i = 0; i < window; ++i, ++iterID) {
					this.ComputeDistRow (pivsel.NextPivot(), idxseq, rand, pivs, tmp_items);
					double varX = 0;
					for (int objID = 0; objID < this.Items.Length; ++objID) {
						var u = this.Items[objID];
						var diff = Math.Abs( u.Dist - pivs[u.ObjID].mean );
						varX += diff * diff / n;
					}
					curr_cost += this.expected_cost(qrad, varX, varY, n, iterID, num_indexes);
				}
				curr_cost = (curr_cost / window) / n;

				curr_cost = weight_prev * prev_cost + weight_curr * curr_cost;

				if (curr_cost < min_cost) {
					min_cost = curr_cost;
				} else {
					break;
				}

				if (iterID % 10 == 0) { 
					Console.WriteLine("XXXXXXXXXXXXXXXXXXXX {0}, db: {1}", this, Path.GetFileName(DB.Name));
					Console.WriteLine("XXX prev-cost: {0:0.000}, curr-cost: {1:0.000}, min-cost: {6}, error: {2:0.00000}, max-error: {3:0.00000}, pivs: {4}, groups: {5}",
					                  prev_cost, curr_cost, error, max_error, iterID, num_indexes, min_cost);
				}
				error = prev_cost - curr_cost;
				prev_cost = curr_cost;
			}
			this.Pivs = pivs.ToArray ();
			Console.WriteLine("Number of pivots per group: {0}", this.Pivs.Length);
		}

		protected double expected_cost(double qrad, double varX, double varY, int n, int num_pivots, int num_indexes) //, double error_factor)
		{
			// var s = Math.Min (1, error_factor * (varX + varY) / (qrad * qrad));
			var s = Math.Min (1, (varX + varY) / (qrad * qrad));
			return num_pivots * num_indexes + n * Math.Pow (1 - s, num_indexes);
		}
	}
}