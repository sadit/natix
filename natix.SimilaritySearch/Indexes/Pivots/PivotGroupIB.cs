////
////  Copyright 2012  Francisco Santoyo
////
////    Licensed under the Apache License, Version 2.0 (the "License");
////    you may not use this file except in compliance with the License.
////    You may obtain a copy of the License at
////
////        http://www.apache.org/licenses/LICENSE-2.0
////
////    Unless required by applicable law or agreed to in writing, software
////    distributed under the License is distributed on an "AS IS" BASIS,
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
////    See the License for the specific language governing permissions and
////    limitations under the License.
//// 
//// Eric S. Tellez
//// - Load and Save methods
//// - Everything was modified to compute slices using radius instead of the percentiles
//// - Argument minimum bucket size
//
//using System;
//using System.IO;
//using natix.CompactDS;
//using System.Collections;
//using System.Collections.Generic;
//using natix.SortingSearching;
//
//namespace natix.SimilaritySearch
//{
//	public class PivotGroupIB : PivotGroup
//	{
//
//		public PivotGroupIB ()
//		{
//		}
//		
//		protected override void SearchExtremes (DynamicSequential idx, List<ItemPair> items, object piv, double quantile, int min_bs, out IResult near, out IResult far, out DynamicSequential.Stats stats)
//		{
//			items.Clear();
//			idx.ComputeDistances (piv, items, out stats);
//			DynamicSequential.SortByDistance(items);
//			var n = idx.Count;
//			min_bs = Math.Max ((int)(quantile * n), min_bs);
//			near = new Result (min_bs);
//			far = new Result (min_bs);
//			idx.AppendKExtremes(near, far, items);
//		}
//
//		public override void Build (MetricDB DB, double alpha, int min_bs, int seed)
//		{
//			this.Items = new ItemPair[DB.Count];
//			for (int objID = 0; objID < Items.Length; ++objID) {
//				this.Items[objID] = new ItemPair(-1,-1);
//			}
//			var pivs = new List<Pivot> (32);
//			this.InternalBuild1 (DB, pivs, alpha, min_bs, seed);
//			// ordering Items and Pivs as expected
//			var items = new ItemPair[DB.Count];
//			for (int objID = 0; objID < items.Length; ++objID) {
//				items[objID] = new ItemPair(objID, this.Items[objID].dist);
//			}
//			Array.Sort<ItemPair,ItemPair>(this.Items, items, new ComparerFromComparison<ItemPair>((a,b) => {
//				var cmp = a.objID.CompareTo(b.objID);
//				if (cmp == 0) {
//					return a.dist.CompareTo(b.dist);
//				} else {
//					return cmp;
//				}
//			}));
//			// Array.Sort<Pivot> (this.Pivs, new ComparerFromComparison<Pivot> ((a,b) => a.objID.CompareTo (b.objID)));
//			this.Items = items;
//			this.Pivs = pivs.ToArray ();
//			Console.WriteLine("Number of pivots per group: {0}", this.Pivs.Length);
//		}
//
//		public virtual void InternalBuild1 (MetricDB DB, List<Pivot> pivs, double alpha, int min_bs, int seed)
//		{
//			var idxDynamic = new DynamicSequentialOrdered ();
//			idxDynamic.Build (DB, RandomSets.GetRandomPermutation(DB.Count, new Random(seed)));
//			var old_pivots = new HashSet<int> ();
//			this.InternalBuild2 (idxDynamic, old_pivots, pivs, alpha, min_bs);
//			var perm = RandomSets.GetIdentity (this.Items.Length);
//			var num_loops = 8;
//			for (int i = 0; i < num_loops; ++i) {
//				Array.Sort(perm, (int x, int y) => {
//					var itemx = this.Items[x];
//					var itemy = this.Items[y];
//					var diffx = Math.Abs (itemx.dist - pivs[itemx.objID].mean);
//					var diffy = Math.Abs (itemy.dist - pivs[itemy.objID].mean);
//					return diffx.CompareTo(diffy);
//				});
//				idxDynamic.Build (DB, perm);
//				this.InternalBuild2 (idxDynamic, old_pivots, pivs, alpha, min_bs);
//			}
//		}
//
//		protected double EstimateMean(int objID, int sample_size)
//		{
//			if (this.Items [objID].objID < 0) {
//				this.Items [objID] = new ItemPair (new_pivot, proposed_item.dist);
//			} else {
//				var old_item = this.Items [proposed_item.objID];
//				if (old_item.dist <= pivs [old_item.objID].last_near) {
//		k			pivs [old_item.objID].num_near -= 1;
//				} else {
//					pivs [old_item.objID].num_far -= 1;
//				}
//				this.Items [objID] = new ItemPair (new_pivot, proposed_item.dist);
//			}
//			if (is_near) {
//				pivs [new_pivot].num_near += 1;
//			} else {
//				pivs [new_pivot].num_far += 1;
//			}
//
//		}
//
//		// the internal build stores in this.Items tuples (pivot,dist(u, pivot)), then Build
//		// must convert them to the standard format for PivotGroupIndex* 
//		public virtual void InternalBuild2 (DynamicSequential idxDynamic, HashSet<int> old_pivots, List<Pivot> pivs, double alpha, int min_bs)
//		{
//			var DB = idxDynamic.DB;
//			// var items = new List<ItemPair> (DB.Count);
//			int I = 0;
//			var extreme_items = new List<ItemPair>(idxDynamic.Count);
//			while (idxDynamic.Count > 0) {
//				int pidx;
//				do {
//					pidx = idxDynamic.GetAnyItem();
//					idxDynamic.Remove(pidx);
//				} while (old_pivots.Contains(pidx));
//				old_pivots.Add(pidx);
//				object piv = DB[pidx];
//				IResult near, far;
//				DynamicSequential.Stats stats;
//				this.SearchExtremes(idxDynamic, extreme_items, piv, alpha, min_bs, out near, out far, out stats);
//				var pivID = pivs.Count;
//				int covered_near = 1;
//				int covered_far = 0;
//				if (this.Items[pidx].objID >= 0) {
//					var item = this.Items[pidx];
//					if (item.dist <= pivs[item.objID].last_near) {
//						pivs[item.objID].num_near -= 1;
//					} else {
//						pivs[item.objID].num_far -= 1;
//					}
//				}
//				this.Items[pidx] = new ItemPair(pivID, 0.0);
//
//				double covering_near = 0;
//				double covering_far = double.MaxValue;
//				foreach (var pair in near) {
//					var item = this.Items[pair.docid];
//					if (item.objID == -1) {
//						this.Items[pair.docid] = new ItemPair(pivID, pair.dist);
//						covering_near = Math.Max (covering_near, pair.dist);
//						++covered_near;
//					} else if (Math.Abs(pivs[item.objID].mean - item.dist) < Math.Abs(stats.mean - pair.dist) ) {
//						if (item.dist <= pivs[item.objID].last_near) {
//							pivs[item.objID].num_near -= 1;
//						} else {
//							pivs[item.objID].num_far -= 1;
//						}
//						this.Items[pair.docid] = new ItemPair(pivID, pair.dist);
//						covering_near = Math.Max (covering_near, pair.dist);
//						++covered_near;
//					}
//				}
//				foreach (var pair in far) {
//					var item = this.Items[pair.docid];
//					if (item.objID == -1) {
//						this.Items[pair.docid] = new ItemPair(pivID, pair.dist);
//						covering_far = Math.Max (covering_far, pair.dist);
//						++covered_far;
//					} else if (Math.Abs(pivs[item.objID].mean - item.dist) < Math.Abs(stats.mean - pair.dist) ) {
//						if (item.dist <= pivs[item.objID].last_near) {
//							pivs[item.objID].num_near -= 1;
//						} else {
//							pivs[item.objID].num_far -= 1;
//						}
//						this.Items[pair.docid] = new ItemPair(pivID, pair.dist);
//						covering_far = Math.Max (covering_far, pair.dist);
//						++covered_far;
//					}
//				}
//				var piv_data = new Pivot(pidx, stats.mean, stats.stddev, covering_near, covering_far, covered_near, covered_far);
//				pivs.Add(piv_data);
//				if (I % 10 == 0) {
//					Console.WriteLine ("");
//					Console.WriteLine (this.ToString());
//					Console.WriteLine("-- I {0}> remains: {1}, alpha_stddev: {2}, mean: {3}, stddev: {4}, pivot: {5}",
//					                  I, idxDynamic.Count, alpha, stats.mean, stats.stddev, pidx);
//					double near_first, near_last, far_first, far_last;
//					if (covered_near > 0) {
//						near_first = near.First.dist;
//						near_last = near.Last.dist;
//						//                        Console.WriteLine("-- (ABSVAL)  first-near: {0}, last-near: {1}, near-count: {2}",
//						//                                          near_first, near_last, near.Count);
//						Console.WriteLine("-- (NORMVAL) first-near: {0}, last-near: {1}, near-count: {2}",
//						                  near_first / stats.max, near_last / stats.max, covered_near);
//						//                        Console.WriteLine("-- (SIGMAS)  first-near: {0}, last-near: {1}",
//						//                                          near_first / stats.stddev, near_last / stats.stddev);
//						
//					}
//					if (covered_far > 0) {
//						far_first = far.First.dist;
//						far_last = far.Last.dist;
//						//                        Console.WriteLine("++ (ABSVAL)  first-far: {0}, last-far: {1}, far-count: {2}",
//						//                                          far_first, far_last, far.Count);
//						Console.WriteLine("++ (NORMVAL) first-far: {0}, last-far: {1}, far-count: {2}",
//						                  far_first / stats.max, far_last / stats.max, covered_far);
//						//                        Console.WriteLine("++ (SIGMAS)  first-far: {0}, last-far: {1}",
//						//                                          far_first / stats.stddev, far_last / stats.stddev);
//					}
//				}
//				++I;
//				idxDynamic.Remove(near);
//				idxDynamic.Remove(far);
//				//Console.WriteLine("Number of objects after: {0}",idxDynamic.DOCS.Count);
//			}
//		}
//
//		public override int SearchKNN (MetricDB db, object q, int K, IResult res, short[] A)
//		{
//			int abs_pos = 0;
//			int count_dist = 0;
//			foreach (var piv in this.Pivs) {
//				var pivOBJ = db [piv.objID];
//				var dqp = db.Dist (q, pivOBJ);
//				// the object itself is not removed from Items, then we cannot push the pivot into the result
//				// res.Push (piv.objID, dqp);
//				++count_dist;
//				// checking near ball radius
//				if (dqp <= piv.last_near + res.CoveringRadius) {
//					for (int j = 0; j < piv.num_near; ++j, ++abs_pos) {
//						// Console.WriteLine ("abs_pos: {0}, count: {1}", abs_pos, db.Count);
//						var item = this.Items [abs_pos];
//						// checking covering pivot
//						if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
//							++A [item.objID];
//						}
//					}
//				} else {
//					abs_pos += piv.num_near;
//				}
//				// checking external radius
//				if (dqp + res.CoveringRadius >= piv.first_far) {
//					for (int j = 0; j < piv.num_far; ++j, ++abs_pos) {
//						var item = this.Items [abs_pos];
//						// checking covering pivot
//						if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
//							++A [item.objID];
//						}
//					}
//				} else {
//					abs_pos += piv.num_far;
//				}
//				// This invariant is not followed because buckets are independent
////				if (dqp + res.CoveringRadius <= piv.last_near || piv.first_far <= dqp - res.CoveringRadius) {
////					break;
////				}
//			}
//			return count_dist;
//		}
//	}
//}
//
