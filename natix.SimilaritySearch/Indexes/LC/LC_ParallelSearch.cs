//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Original filename: natix/SimilaritySearch/Indexes/LC_ParallelSearch.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// LC with a parallel search, can work with any LC
	/// </summary>
	/// <exception cref='ArgumentNullException'>
	/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
	/// </exception>
	public class LC_ParallelSearch : LC_RNN
	{		
		/// <summary>
		/// Initializes a new instance
		/// </summary>
		public LC_ParallelSearch () : base()
		{	
		}
		
		/// <summary>
		/// Search the specified q with radius qrad.
		/// </summary>
		public override IResult SearchRange (object q, double qrad)
		{
			// Console.WriteLine ("XXXXXXX qrad: {0}", qrad);
			var sp = this.DB;
			int len = this.CENTERS.Count;
			// int max_t = 16;
			var L = new List<ResultPair> ();
			var M = new List<ResultPair> ();
			/*Result[] partial_res = new Result[max_t];
			for (int i = 0; i < max_t; ++i) {
				partial_res [i] = new Result (int.MaxValue, false);
			}*/
			Action<int> review_centers = delegate(int center_id) {
				var dcq = sp.Dist (this.DB [this.CENTERS [center_id]], q);
				// int uniq_tid = Thread.CurrentThread.ManagedThreadId % max_t;
				if (dcq <= qrad) {
					/*lock (partial_res[uniq_tid]) {
						partial_res [uniq_tid].Push (this.CENTERS [center_id], dcq);
					}*/
					lock (L) {
						L.Add (new ResultPair (this.CENTERS [center_id], dcq));
					}
				}
				if (dcq <= qrad + this.COV [center_id]) {
					lock (M) {
						M.Add (new ResultPair (center_id, dcq));
					}
				}
			};
			Action<ResultPair> review_buckets = delegate(ResultPair center_pair) {
				var center_id = center_pair.docid;
				var dcq = center_pair.dist;
				if (dcq <= qrad + this.COV [center_id]) {
					var rs = this.SEQ.Unravel (center_id);
					var count1 = rs.Count1;
					for (int i = 1; i <= count1; i++) {
						var u = rs.Select1 (i);
						var r = sp.Dist (q, sp [u]);
						if (r <= qrad) {
							/*lock (partial_res[uniq_tid]) {
								partial_res [uniq_tid].Push (u, r);
							}*/
							lock (L) {
								L.Add (new ResultPair (u, r));
							}
						}
					}
				}
			};
			var pops = new ParallelOptions ();
			pops.MaxDegreeOfParallelism = -1;
			Parallel.For (0, len, pops, review_centers);
			/*
			Action<int> review_items = delegate (int u) {
				// var u = rs.Select1 (i);
				var r = sp.Dist (q, sp [u]);
				if (r <= qrad) {
					lock (L) {
						L.Add (new ResultPair (u, r));
					}
				}
			};
			foreach (var center_pair in M) {
				var center_id = center_pair.docid;
				var dcq = center_pair.dist;
				if (dcq <= qrad + this.COV [center_id]) {
					var list = new SortedListRS (this.SEQ.Unravel (center_id));
					Parallel.ForEach<int> (list, pops, review_items);
				}
			}*/
			Parallel.ForEach<ResultPair> (M, pops, review_buckets);
			/*var R = partial_res [0];
			for (int i = 1; i < max_t; ++i) {
				foreach (var p in partial_res[i]) {
					R.Push (p.docid, p.dist);
				}
			}
			return R;*/
			var R = new Result (int.MaxValue, false);
			foreach (var pair in L) {
				R.Push (pair.docid, pair.dist);
			}
			return R;
		}
		
		/// <summary>
		/// KNN search.
		/// </summary>
		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var sp = this.DB;
			int len = this.CENTERS.Count;
			// int t_max = 16;
			//var center_res = new IResult[t_max];
			// var partial_res = new IResult[t_max];
			var current_res = res;
			int [] centers_list = new int[ len ];
			double [] dists_list = new double[ len ];
			/*for (int i = 0; i < t_max; ++i) {
				center_res [i] = this.MainSpace.CreateResult (len, false);
				// partial_res [i] = this.MainSpace.CreateResult (K, res.Ceiling);
			}*/
			// center's selection
			Action<int> S = delegate(int center) {
				// var t_id = Thread.CurrentThread.ManagedThreadId % t_max;
				var dcq = sp.Dist (this.DB [this.CENTERS [center]], q);
				/*lock (center_res[t_id]) {					
					center_res [t_id].Push (center, dcq);
				}*/
				centers_list [center] = center;
				dists_list [center] = dcq;
				//var current_res = partial_res [t_id];
				/*if (dcq <= current_res.CoveringRadius + this.COV [center]) {
					lock (current_res) {
						current_res.Push (this.CENTERS [center], dcq);
					}
				}*/
			};
			var pops = new ParallelOptions ();
			pops.MaxDegreeOfParallelism = -1;
			Parallel.For (0, len, pops, S);
			Array.Sort<double, int> (dists_list, centers_list);
			for (int i = 0; i < K; ++i) {
				current_res.Push (centers_list[i], dists_list[i]);
			}
			// parallel review of centers
			Action<int> review_centers = delegate(int center_index) {
				// int t_id = Thread.CurrentThread.ManagedThreadId % t_max;
				var dcq = dists_list [center_index];
				int center = centers_list [center_index];
				//var current_res = partial_res [t_id];
				var cov = this.COV [center];
				if (dcq <= current_res.CoveringRadius + cov) {
					var rs = this.SEQ.Unravel (center);
					var count1 = rs.Count1;
					for (int i = 1; i <= count1; i++) {
						var u = rs.Select1 (i);
						var r = sp.Dist (q, sp [u]);
						//if (r <= qr) // already handled by R.Push
						lock (current_res) {
							current_res.Push (u, r);
						}
					}
				}
			};
			pops = new ParallelOptions ();
			pops.MaxDegreeOfParallelism = -1;
			//Parallel.For (0, len, pops, review_centers);
			Parallel.For (0, len, pops, review_centers);
			/* foreach (var C in center_res) {	
				Parallel.ForEach<ResultPair> (C, pops, review_centers);
			}*/
			/*foreach (var R in partial_res) {
				foreach (var p in R) {
					res.Push (p.docid, p.dist);
				}
			}*/
			return res;
		}
	}
}
