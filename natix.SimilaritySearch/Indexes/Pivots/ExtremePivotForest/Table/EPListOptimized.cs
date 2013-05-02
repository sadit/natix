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
			return stats;
		}

		protected void EstimateQueryStatistics(MetricDB DB, Random rand, int num_queries, int sample_size, out double mean, out double varY, out double qrad)
		{
			var n = DB.Count;
			var N = num_queries * sample_size;
			mean = 0.0;
			var square_mean = 0.0;
			qrad = 0;
			for (int qID = 0; qID < num_queries; ++qID) {
				var q = DB[ rand.Next(0, n) ];
				var min = double.MaxValue;
				for (int sampleID = 0; sampleID < sample_size; ++sampleID) {
					var u = DB[ rand.Next(0, n) ];
					var d = DB.Dist(q, u);
					mean += d / N;
					square_mean += d * d / N;
					if (d > 0) {
						min = Math.Min (min, d);
					}
				}
				// qrad = Math.Max (min, qrad);
				if (qrad == 0) {
					qrad = min;
				} else {
					qrad = (min + qrad) * 0.5;
				}
			}
			varY = square_mean - mean * mean;
		}

		public EPListOptimized (MetricDB DB, int seed, int num_indexes, int max_iters, double error_factor)
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
			this.EstimateQueryStatistics (DB, rand, 128, 128, out mean, out varY, out qrad);
			double prev_cost = -1;
			double curr_cost = n;
			double derivative;
			int next_piv = rand.Next (0, n);
			int i = 0;
			do {
				double min_diff = double.MaxValue;
				this.ComputeDistRow (next_piv, idxseq, rand, pivs, tmp_items);
				double varX = 0;
				for (int objID = 0; objID < this.Items.Length; ++objID) {
					var u = this.Items[objID];
					var diff = Math.Abs( u.dist - pivs[u.objID].mean );
					varX += diff * diff / n;
					if (diff < min_diff) {
						min_diff = diff;
						next_piv = objID;
					}
				}
				++i;
				prev_cost = curr_cost;
				curr_cost = this.expected_cost(qrad, varX, varY, n, i, num_indexes, error_factor);
				derivative = curr_cost - prev_cost;
				// Console.WriteLine ("DEBUG: stddev: {0}", stats.stddev);
				if (i % 10 == 1) {
					Console.WriteLine("XXXXXX i: {0}, qcurr_cost: {1}, prev_cost: {2}, varX: {3}, varY: {4}, qrad: {5}",
					                   i, curr_cost, prev_cost, varX, varY, qrad);
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

