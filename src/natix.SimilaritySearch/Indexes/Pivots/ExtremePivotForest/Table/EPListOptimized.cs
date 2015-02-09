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
	public class EPListOptimized : EPList
	{			
		public EPListOptimized () : base()
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

		public EPListOptimized (MetricDB DB, int num_indexes, Random rand, int max_iters, double error_factor)
		{
			Console.WriteLine ("XXX {0}, num_indexes: {1}, max_iters: {2}, error_factor: {3}", this, num_indexes, max_iters, error_factor);
			this.Items = null;
			var pivs = new List<EPivot> (32);
			var n = DB.Count;
			var idxseq = new DynamicSequentialOrdered ();
			idxseq.Build (DB, RandomSets.GetIdentity (DB.Count));
			var tmp_items = new List<ItemPair> (DB.Count);
			double qrad;
			double varY;
			double mean;
			PivotSelector.EstimateQueryStatistics (DB, rand, 128, 128, out mean, out varY, out qrad);
			double prev_cost = -1;
			double curr_cost = n;
			double derivative;
			var pivsel = new PivotSelectorRandom (n, rand);
			int nextpiv = pivsel.NextPivot();
			int i = 0;
			do {
				// Console.WriteLine("A {0} => {1}, {2}", this, i, seed);
				//double min_diff = double.MaxValue;
				this.ComputeDistRow (nextpiv, idxseq, rand, pivs, tmp_items);
				// Console.WriteLine("B {0} => {1}, {2}", this, i, seed);
				double varX = 0;
				for (int objID = 0; objID < this.Items.Length; ++objID) {
					var u = this.Items[objID];
					var diff = Math.Abs( u.Dist - pivs[u.ObjID].mean );
					varX += diff * diff / n;
//					if (diff < min_diff) {
//						min_diff = diff;
//						next_piv = objID;
//					}
				}
				nextpiv = pivsel.NextPivot();
				// Console.WriteLine("C {0} => {1}, {2}", this, i, seed);
				++i;
				prev_cost = curr_cost;
				curr_cost = this.expected_cost(qrad, varX, varY, n, i, num_indexes, error_factor);
				derivative = curr_cost - prev_cost;
				// Console.WriteLine ("DEBUG: stddev: {0}", stats.stddev);
				if (i % 10 == 1) {
					Console.Write("XXXXXX {0}, iteration: {1}, DB: {2}, ", this, i, DB.Name);
					Console.WriteLine("qcurr_cost: {0}, prev_cost: {1}, varX: {2}, varY: {3}, qrad: {4}",
					                   curr_cost, prev_cost, varX, varY, qrad);
				}
			} while (derivative < 0 && i < max_iters);
			this.Pivs = pivs.ToArray ();
			Console.WriteLine("Number of pivots per group: {0}", this.Pivs.Length);
		}

		protected double expected_cost(double qrad, double varX, double varY, int n, int num_pivots, int num_indexes, double error_factor)
		{
			var s = Math.Min (1, error_factor * (varX + varY) / (qrad * qrad));
			return num_pivots * num_indexes + n * Math.Pow (1 - s, num_indexes);
		}
	}
}