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
	public class EPListRandomPivotsPriorized : EPList
	{			
		public EPListRandomPivotsPriorized () : base()
		{
		}

		public void Statistics(out double mean, out double variance)
		{
			var n = this.Items.Length;
			var square_mean = 0.0;
			mean = 0;
			foreach (var item in this.Items) {
				mean += item.dist / n;
				square_mean += item.dist * item.dist / n;
			}
			variance = square_mean - mean * mean;
		}

		public EPListRandomPivotsPriorized (MetricDB DB, int seed, int num_pivs)
		{
			var n = DB.Count;
			this.Items = new ItemPair[n];
			var pivs = new List<EPivot> (32);
			var rand = new Random (seed);
			var pivsel = new PivotSelector (n, rand);
			var piv = pivsel.NextPivot ();
			var pivOBJ = DB [piv];
			for (int objID = 0; objID < n; ++objID) {
				var d = DB.Dist(pivOBJ, DB[objID]);
				this.Items[objID] = new ItemPair(0, d);
			}
			double mean, variance;
			this.Statistics (out mean, out variance);
			pivs.Add(new EPivot(piv, Math.Sqrt(variance), mean, 0, 0, 0, 0));
			var item_cmp = new Comparison<ItemPair>((x,y) => {
				var diff_x = Math.Abs (x.dist - pivs[x.objID].mean);
				var diff_y = Math.Abs (y.dist - pivs[y.objID].mean);
				return diff_x.CompareTo(diff_y);
			});
			var queue = new SkipList2<int> (0.5, (x,y) => item_cmp (this.Items [x], this.Items [y]));
			for (int objID = 0; objID < n; ++objID) {
				queue.Add(objID, null);
			}
			var max_review = 2 * n / num_pivs;
			var list = new List<int> ();
			for (int i = 0; i < num_pivs; ++i) {
				Console.WriteLine("XXXXXX BEGIN {0} i: {1}", this, i);
				piv = pivsel.NextPivot();
				double piv_mean, piv_variance, qrad;
				PivotSelector.EstimatePivotStatistics(DB, rand, DB[piv], 256, out piv_mean, out piv_variance, out qrad);
				var pivID = pivs.Count;
				pivs.Add(new EPivot(piv, Math.Sqrt(piv_variance), mean, 0, 0, 0, 0));
				list.Clear();
				for (int s = 0; s < max_review; ++s) {
					var objID = queue.RemoveFirst();
					var d = DB.Dist(DB[objID], pivOBJ);
					var new_item = new ItemPair(pivID, d);
					if (item_cmp(new_item, this.Items[objID]) > 0) {
						this.Items[objID] = new_item;
					}
					list.Add (objID);
				}
				foreach (var objID in list) {
					queue.Add(objID, null);
				}
				Console.WriteLine("XXXXXX END {0} i: {1}", this, i);
			}
			this.Pivs = pivs.ToArray ();
			Console.WriteLine("Number of pivots per group: {0}", this.Pivs.Length);
		}
	}
}

