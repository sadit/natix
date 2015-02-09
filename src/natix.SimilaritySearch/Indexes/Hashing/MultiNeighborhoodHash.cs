// 
//  Copyright 2012-2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
using System;
using System.IO;
using System.Collections.Generic;
using natix.CompactDS;
using natix.Sets;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class MultiNeighborhoodHash : BasicIndex
	{
		public class Parameters {
			public NeighborhoodHash Index;
			public int NumberOfInstances = 0;
		}

		public NeighborhoodHash[] A;

		public override void Save (BinaryWriter Output)
        {
			base.Save (Output);
            Output.Write (A.Length);
            foreach (var a in A) {
                a.Save(Output);
            }
		}

		public override void Load (BinaryReader Input)
        {
			base.Load (Input);
            var len = Input.ReadInt32 ();
			this.A = new NeighborhoodHash[len];
            for (int i = 0; i < len; ++i) {
				var a = new NeighborhoodHash();
                a.Load(Input);
                this.A[i] = a;
            }
		}

		public MultiNeighborhoodHash () : base()
		{
		}

		static int _EstimateParameters(int k, double expected_recall, NeighborhoodHash I, int[] Q, HashSet<int>[] res_array)
		{
			var recall = 0.0;
			var recall_sq = 0.0;
			for (int i = 0; i < Q.Length; ++i) {
				var res = res_array [i];
				double matches = 0;
				var approx_res = I.SearchKNN (I.DB [Q[i]], k);
				foreach (var p in approx_res) {
					if (res.Contains (p.ObjID)) {
						++matches;
					}
				}
				// we remove one, the first is always found
				if (i % 10 == 0) {
					Console.WriteLine ("estimation step, query matches: {0}", matches);
				}
				var current_recall = (matches - 1) / (k - 1);
				recall += current_recall;
				recall_sq += current_recall * current_recall;
			}
			recall /= Q.Length;
			recall_sq /= Q.Length;			
			var recall_stddev = Math.Sqrt (recall_sq - recall * recall);
			Console.WriteLine ("=== expected recall mean: {0}, recall stddev: {1}",
			                   recall, recall_stddev);
			if (recall == 0) {
				throw new ArgumentException ("A recall zero will produce an infinite number of instances, " +
				                             "please check the basic setup in order to create a valid index");
			}
			// recall *= (1.0 - recall_stddev);
			// Console.WriteLine ("=== CORRECTED recall: {0}", recall);
			var num_instances = 1 + (int)(Math.Log (1.0 - expected_recall) / Math.Log(1.0 - recall));
			Console.WriteLine ("=== # instances {0}", num_instances);
			return num_instances;
		}

		public static Parameters EstimateParameters(MetricDB db, int max_instances, int k, double expected_recall, int num_estimation_queries)
		{
			var seq = new Sequential ();
			seq.Build (db);
			var I = new NeighborhoodHash ();
			int symbolsPerHash = 3;
			I.Build (db, symbolsPerHash);
			var Q = RandomSets.GetRandomSubSet (num_estimation_queries, db.Count);
			// k > 1 since Q is a subset of the database
			if (k == 1) {
				++k;
			}
			++k;
			var res_array = new HashSet<int> [Q.Length];
			for (int i = 0; i < Q.Length; ++i) {
				var s = KnrFP.GetFP (db [Q [i]], seq, k);
				res_array [i] = new HashSet<int> (s);
			}
			int num_instances = 0;
			--I.NeighborhoodExpansion;
			double cost = 0.0;
			double time = 0.0;
		
			do {
				++I.NeighborhoodExpansion;
				var c = db.NumberDistances;
				var t = DateTime.Now.Ticks;
				num_instances = _EstimateParameters(k, expected_recall, I, Q, res_array);
				cost = (db.NumberDistances - c) / Q.Length * num_instances;
				time = TimeSpan.FromTicks((DateTime.Now.Ticks - t) / Q.Length).TotalSeconds * num_instances;
				Console.WriteLine("==== expansion: {0}, num_instances: {1}, search-cost: {2}, search-time: {3}", I.NeighborhoodExpansion, num_instances, cost, time);
			} while (num_instances > max_instances);

			return new Parameters() {
				Index = I,
				NumberOfInstances = num_instances
			};
		}
		/// <summary>
		/// Creates an index for db using the specified number of instances.
		/// </summary>

		public void Build (MetricDB db, Parameters uparams)
        {
			var seed = RandomSets.GetRandomInt ();
			this.A = new NeighborhoodHash[uparams.NumberOfInstances];
            this.DB = db;
			this.A [0] = uparams.Index;
			for(int i = 1; i < uparams.NumberOfInstances; ++i) {
				Console.WriteLine ("==== creating {0}/{1} instances", i + 1, uparams.NumberOfInstances);
				var I = new NeighborhoodHash ();
				I.Build(db, uparams.Index.SymbolsPerHash, uparams.Index.NeighborhoodExpansion, new Random(seed + i));
				this.A [i] = I;
			}
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var evaluated = new HashSet<int> ();
			var exp = A [0].NeighborhoodExpansion - A[0].SymbolsPerHash + 1;
			foreach (var idx in this.A) {
				var rankedList = idx.RankRefs (q);
				for (int start = 0; start < exp; ++start) {
					var hashList = idx.FetchPostingLists (rankedList, start);
					idx.InternalSearchKNN (q, res, hashList, evaluated);
				}
			}
			return res;
			// Parallel version seems to be obvious but the overhead is too high
		}
	}
}