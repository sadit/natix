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
//   Original filename: natix/SimilaritySearch/Indexes/LC_FixedM.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// LC with fixed percentiles (M)
	/// </summary>
	/// <exception cref='ArgumentNullException'>
	/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
	/// </exception>
	public class LC : LC_RNN
	{
		/// <summary>
		/// Initializes a new instance of the index
		/// </summary>
		public LC () : base()
		{
		}

		/// <summary>
		/// SearchKNN method to be performed at build time
		/// </summary>
		/*public static void BuildSearchKNN (Space<T> sp, SkipList2<int> queue, int docid, IResult res, IList<int> PERM)
		{
			foreach (var u in queue.Traverse()) {
				var dist = sp.Dist (sp [docid], sp [PERM [u]]);
				res.Push (u, dist);
			}
		}*/
		protected virtual void BuildSearchKNN (ref IList<int> rest_list, object center, IResult res)
		{
			var sp = this.DB;
			var n = rest_list.Count;
			int nullcount = 0;
			var R = new Result (res.K, res.Ceiling);
			for (int i = 0; i < n; ++i) {
				var oid = rest_list [i];
				if (oid < 0) {
					++nullcount;
					continue;
				}
				var dist = sp.Dist (center, sp [oid]);
				R.Push (i, dist);
			}
			foreach (var p in R) {
				var i = p.docid;
				res.Push (rest_list [i], p.dist);
				rest_list [i] = -1;
				++nullcount;
			}
			// an amortized algorithm to handle deletions
			// the idea is to keep the order of review to improve cache
			// 0.33n because it works well for my tests, but it can be any constant proportion
			// of the database
			if (nullcount >= (int)(0.33 * n)) {
				var L = new List<int> (n - nullcount);
				foreach (var u in rest_list) {
					if (u >= 0) {
						L.Add (u);
					}
				}
				rest_list = L;
			}
			// Console.WriteLine ("XXX NULLCOUNT END: rest_list.Count: {0}, nullcount: {1}", rest_list.Count, nullcount);
		}
		
		/// <summary>
		/// Builds the LC with fixed bucket size (static version).
		/// </summary>
		protected virtual IList<int> InternalBuild (ref IList<int> rest_list, int bsize)
		{
			int iteration = 0;
			int numiterations = rest_list.Count / bsize + 1;
			var seq = new int[this.DB.Count];
			var rand = new Random ();
			Console.WriteLine ("XXX BEGIN Build rest_list.Count: {0}", rest_list.Count);
			while (rest_list.Count > 0) {
				int center;
				int i;
				do {
					i = rand.Next (rest_list.Count);
					center = rest_list [i];
				} while (center < 0);
				rest_list [i] = -1;
				var symcenter = this.CENTERS.Count;
				this.CENTERS.Add (center);
				IResult res = this.DB.CreateResult (bsize, false);
				BuildSearchKNN (ref rest_list, this.DB [center], res);
				double covrad = double.MaxValue;
				foreach (var p in res) {
					seq[p.docid] = symcenter;
					covrad = p.dist;
				}
				COV.Add ((float)covrad);
				if (iteration % 50 == 0) {
					Console.WriteLine ("docid {0}, iteration {1}/{2}, date: {3}", center, iteration, numiterations, DateTime.Now);
				}
				iteration++;
			}
			Console.WriteLine ("XXX END Build rest_list.Count: {0}, iterations: {1}", rest_list.Count, iteration);
			return seq;
		}
	
		/// <summary>
		/// Build the LC_FixedM
		/// </summary>
		public override void Build (MetricDB db, int num_centers, SequenceBuilder seq_builder = null)
		{
			int bsize = (db.Count - num_centers) / num_centers;
			this.DB = db;
			int n = db.Count;
			this.CENTERS = new List<int> (num_centers + 1);
			IList<int> rest_list = new List<int> (n);
			this.COV = new List<float> (num_centers + 1);
			for (int i = 0; i < n; ++i) {
				rest_list.Add (i);
			}
			var seq = this.InternalBuild (ref rest_list, bsize);
			foreach (var c in this.CENTERS) {
				seq[c] = this.CENTERS.Count;
			}
			this.FixOrder(seq);
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqXLB_SArray64 (16);
			}
			this.SEQ = seq_builder(seq, this.CENTERS.Count + 1);
		}

		protected void FixOrder (IList<int> seq)
		{
			var num_centers = this.CENTERS.Count;
			var centers_perm = new int[num_centers];
			for (int i = 0; i < num_centers; ++i) {
				centers_perm[i] = i;
			}
			Sorting.Sort<int,int> (this.CENTERS, centers_perm);
			var inv = RandomSets.GetInverse(centers_perm);
			var cov = new float[num_centers];
			for (int i = 0; i < num_centers; ++i) {
				cov [inv [i]] = this.COV [i];
			}
			this.COV = cov;
			var n = seq.Count;
			for (int i = 0; i < n; ++i) {
				var u = seq [i];
				if (u < num_centers) {
					seq [i] = inv [u];
				}
			}
		}
	}
}
