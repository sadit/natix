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
//   Original filename: natix/SimilaritySearch/Indexes/LC_PFixedM.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NDesk.Options;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// LC with fixed percentiles and parallel preprocessing
	/// </summary>
	/// <exception cref='ArgumentNullException'>
	/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
	/// </exception>
	public class LC_ParallelBuild : LC
	{
		/// <summary>
		/// Initializes a new instance of the index
		/// </summary>
		public LC_ParallelBuild () : base()
		{
		}
		
		List<int> build_rest_list;
		
		void BuildSearchKNN (object center, IResult res, object[] null_locks)
		{
			var n = this.build_rest_list.Count;
			int max_t = null_locks.Length;
			Result[] R = new Result[max_t];
			int[] nullC = new int[max_t];
			for (int i = 0; i < max_t; ++i) {
				R [i] = new Result (res.K, res.Ceiling);
			}
			Action<int> action = delegate(int i) {
				//for (int i = 0; i < n; ++i) {
				var t_id = Thread.CurrentThread.ManagedThreadId % max_t;
				var oid = this.build_rest_list [i];
				if (oid < 0) {
					// lock (R[t_id]) {
					lock (null_locks[t_id]) {
						++nullC [t_id];
					}
					return;
				}
				var dist = this.DB.Dist (center, this.DB [oid]);
				lock (R[t_id]) {
					R [t_id].Push (i, dist);
				}
			};
			var pops = new ParallelOptions ();
			pops.MaxDegreeOfParallelism = -1;
			// var w = new TaskFactory ();
			// pops.TaskScheduler = new FixedSizeScheduler ();
			Parallel.For (0, n, pops, action);
			/*for (int i = 0; i < this.thread_counter.Length; ++i) {
				Console.Write ("{0}, ", this.thread_counter [i]);
			}
			Console.WriteLine ();*/
			int nullcount = 0;
			var _res = new Result (res.K, res.Ceiling);
			for (int x = 0; x < R.Length; ++x) {
				var _R = R [x];
				nullcount += nullC [x];
				foreach (var p in _R) {
					var i = p.docid;
					_res.Push (i, p.dist);
				}
			}
			foreach (var p in _res) {
				var i = p.docid;
				res.Push (this.build_rest_list [i], p.dist);
				this.build_rest_list [i] = -1;
				++nullcount;
			}
			// an amortized algorithm to handle deletions
			// the idea is to keep the order of review to improve cache
			// 0.33n because it works well for my tests, but it can be any constant proportion
			// of the database
			if (nullcount >= (int)(0.33 * n)) {
				var L = new List<int> (n - nullcount);
				foreach (var u in this.build_rest_list) {
					if (u >= 0) {
						L.Add (u);
					}
				}
				this.build_rest_list = L;
			}
			// Console.WriteLine ("XXX NULLCOUNT END: rest_list.Count: {0}, nullcount: {1}", rest_list.Count, nullcount);
		}
		
		/// <summary>
		/// Builds the LC with fixed bucket size (static version).
		/// </summary>
		public void _Build (int[] seq, int bsize, Random rand)
		{
			int iteration = 0;
			int numiterations = this.build_rest_list.Count / bsize + 1;
			Console.WriteLine ("XXX BEGIN Parallel-BuildFixedM rest_list.Count: {0}", this.build_rest_list.Count);
			int max_t = 16;
			object[] null_locks = new object[max_t];
			for (int i = 0; i < max_t; ++i) {
				null_locks [i] = new object ();
			}
			while (this.build_rest_list.Count > 0) {
				int center;
				int i;
				do {
					i = rand.Next (this.build_rest_list.Count);
					center = this.build_rest_list [i];
				} while (center < 0);
				this.build_rest_list [i] = -1;
				int sym_center = this.CENTERS.Count;
				this.CENTERS.Add (center);
				IResult res = this.DB.CreateResult (bsize, false);
				this.BuildSearchKNN (this.DB [center], res, null_locks);
				double covrad = double.MaxValue;
				foreach (var p in res) {
					seq[p.docid] = sym_center;
					covrad = p.dist;
				}
				this.COV.Add ((float)covrad);
				if (iteration % 50 == 0) {
					Console.WriteLine ("docid {0}, iteration {1}/{2}, date: {3}", center, iteration, numiterations, DateTime.Now);
				}
				iteration++;
			}
			Console.WriteLine ("XXX END Parallel-BuildFixedM rest_list.Count: {0}, iterations: {1}", this.build_rest_list.Count, iteration);
		}
		
		/// <summary>
		/// Build the LC_FixedM
		/// </summary>

		public override void Build (MetricDB db, int num_centers, Random rand, SequenceBuilder seq_builder)
		{
			int bsize = (db.Count - num_centers) / num_centers;
			this.DB = db;
			int n = db.Count;
			this.CENTERS = new List<int> (num_centers + 1);
			this.COV = new List<float> (this.CENTERS.Count);
			this.build_rest_list = new List<int> (n);
			for (int i = 0; i < n; ++i) {
				this.build_rest_list.Add (i);
			}
			var seq = new int[n];
			this._Build (seq, bsize, rand);
			foreach (var c in this.CENTERS) {
				seq[c] = this.CENTERS.Count;
			}
			this.FixOrder(seq);
			this.SEQ = seq_builder(seq, this.CENTERS.Count + 1);
		}
	}
}
