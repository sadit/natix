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
	public class EPListOptimizedA : EPList
	{			
		public EPListOptimizedA () : base()
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
					this.Items [objID] = new ItemPair (0, _items [objID].dist); 
				}
			} else {
				for (int objID = 0; objID < n; ++objID) {
					var new_piv = pivs [pivID];
					var new_dist = _items [objID].dist;
					var old_piv = pivs [this.Items [objID].objID];
					var old_dist = this.Items [objID].dist;
					if (Math.Abs (old_dist - old_piv.mean) < Math.Abs (new_dist - new_piv.mean)) {
						this.Items [objID] = new ItemPair (pivID, _items [objID].dist);
					}
				}
			}
			return stats;
		}

		public EPListOptimizedA (MetricDB DB, int seed, int num_indexes, double max_error_factor = 0.001)
		{
			this.Items = null;
			var pivs = new List<EPivot> (32);
			var rand = new Random (seed);
			var n = DB.Count;
			var idxseq = new DynamicSequentialOrdered ();
			idxseq.Build (DB, RandomSets.GetIdentity (DB.Count));
			var tmp_items = new List<ItemPair> (DB.Count);
			double qrad;
			double varY;
			double mean;
			PivotSelector.EstimateQueryStatistics (DB, rand, 128, 128, out mean, out varY, out qrad);
			//double prev_cost = -1;
			//double curr_cost = n;

			var pivsel = new PivotSelector (n, rand);
			double avg_prev_cost = n;
			// anything larger than 1.x can be considered a valid starting error_factor
			double error_factor = n;
			var avg_window = 16;

			var iterID = 1;
			max_error_factor += 1;
			while (max_error_factor <= error_factor) {
				double avg_curr_cost = 0;
				for (int i = 0; i < avg_window; ++i, ++iterID) {
					this.ComputeDistRow (pivsel.NextPivot(), idxseq, rand, pivs, tmp_items);
					double varX = 0;
					for (int objID = 0; objID < this.Items.Length; ++objID) {
						var u = this.Items[objID];
						var diff = Math.Abs( u.dist - pivs[u.objID].mean );
						varX += diff * diff / n;
					}
					var curr_cost = this.expected_cost(qrad, varX, varY, n, iterID, num_indexes); 
					avg_curr_cost += curr_cost;
				}
				avg_curr_cost /= avg_window;
				error_factor = avg_prev_cost / avg_curr_cost;
	
				Console.WriteLine("XXXXXXXXXXXXXXXXXXXX {0}, seed: {1}, iteration: {2} ", this, seed, iterID, DB.Name);
				Console.WriteLine("XXX DB: {0}", DB.Name);
				Console.WriteLine("XXX avg_curr_cost: {0}, avg_prev_cost: {1}, error_factor: {2}",
					avg_curr_cost, avg_prev_cost, error_factor);

				avg_prev_cost = avg_curr_cost;
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