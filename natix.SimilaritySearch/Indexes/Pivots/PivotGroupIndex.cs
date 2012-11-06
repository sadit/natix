//
//  Copyright 2012  Eric Sadit Tellez Avila
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
// Francisco Santoyo 
// Adaptation of the KNN searching algorithm of LAESA

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class PivotGroupIndex : BasicIndex
	{
		public PivotGroup[] GROUPS;

		public PivotGroupIndex ()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var numgroups = Input.ReadInt32 ();
			this.GROUPS = new PivotGroup[numgroups];
			for (int i = 0; i < numgroups; ++i) {
				this.GROUPS[i] = new PivotGroup();
				this.GROUPS[i].Load(Input);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write ((int)this.GROUPS.Length);
			for (int i = 0; i < this.GROUPS.Length; ++i) {
				this.GROUPS[i].Save(Output);
			}
		}

		public void Build (PivotGroupIndex pgi, int num_groups)
		{
			this.DB = pgi.DB;
			this.GROUPS = new PivotGroup[num_groups];
			for (int i = 0; i < num_groups; ++i) {
				this.GROUPS[i] = pgi.GROUPS[i];
			}
		}

		public void Build (MetricDB db, int num_groups, double alpha_stddev, int min_bs, int num_build_processors = -1)
		{
			this.DB = db;
			this.GROUPS = new PivotGroup[num_groups];
			ParallelOptions ops = new ParallelOptions ();
			ops.MaxDegreeOfParallelism = num_build_processors;
			// Parallel.For (0, num_groups, ops, (i) => this.GROUPS[i] = this.GetGroup(percentil));
			int I = 0;
			var build_one_group = new Action<int> (delegate(int i) {
				this.GROUPS[i] = new PivotGroup();
				this.GROUPS[i].Build(this.DB, alpha_stddev, min_bs, RandomSets.GetRandomInt());
				// this.GROUPS [i] = this.GetGroup (alpha_stddev, min_bs);
				Console.WriteLine ("Advance {0}/{1} (alpha_stddev={2}, db={3}, timestamp={4})",
				                   I, num_groups, alpha_stddev, db.Name, DateTime.Now);
				I++;
			});
			// parallel_build = false;
			if (num_build_processors == 1 || num_build_processors == 0) {
				//Parallel.ForEach (new List<int>(RandomSets.GetExpandedRange (num_groups)), ops, build_one);
				for (int i = 0; i < num_groups; ++i) {
					//this.GROUPS[i] = this.GetGroup(percentil);
					build_one_group (i);
					if (i % 5 == 0) {
						Console.WriteLine ("*** Procesing groups ({0}/{1}) ***", i, num_groups);
					}
				}
			} else {
				Parallel.For (0, num_groups, ops, build_one_group);
			}
		}

		// static object LOCK = new object();

		public override IResult SearchKNN (object q, int K, IResult res)
		{		
			var l = this.GROUPS.Length;
			var n = this.DB.Count;
			var DIST = new Dictionary<int, double>();
			for (int group_id = 0; group_id < l; ++group_id) {
				foreach (var pivID in this.GROUPS[group_id].pivots_list) {
					var d = this.DB.Dist(this.DB[pivID], q);
					res.Push(pivID, d);
					DIST[pivID] = d;
				}
			}

			for(int docid=0; docid<n; ++docid){
				bool check_object = true;
				if(DIST.ContainsKey(docid)){
					continue;
				}

				for (int group_id = 0; group_id < l; ++group_id) {
					var g = this.GROUPS[group_id];
					var pivID = g.pivots_idx[ docid ];
					var dpu = g.pivots_dist[ docid ];
					var dqp = DIST[pivID];
					if (Math.Abs (dqp - dpu) > res.CoveringRadius) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					res.Push(docid, this.DB.Dist(q, this.DB[docid]));
				}
			}

			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
			var l = this.GROUPS.Length;
			var n = this.DB.Count;
			var res = new Result(this.DB.Count, false);
			var DIST = new Dictionary<int, double>();
			for (int group_id = 0; group_id < l; ++group_id) {
				foreach (var pivID in this.GROUPS[group_id].pivots_list) {
					//if (!DIST.ContainsKey(pivID)) {
						var d = this.DB.Dist(this.DB[pivID], q);
						if (d <= radius) {
							res.Push(pivID, d);
						}
						DIST[pivID] = d;
					//}
				}
			}

			for (int docid = 0; docid < n; ++docid) {
				bool check = true;
				if (DIST.ContainsKey(docid)) {
					continue;
				}
				for (int group_id = 0; group_id < l; ++group_id) {
					var g = this.GROUPS[group_id];
					var pivID = g.pivots_idx[ docid ];
					var dpu = g.pivots_dist[ docid ];
					var dqp = DIST[pivID];
					if (Math.Abs (dqp - dpu) > radius) {
						check = false;
						break;
					}
				}
				if (check) {
					var d = this.DB.Dist(this.DB[docid], q);
					if (d <= radius) {
						res.Push(docid, d);
					}
				}
			}
			return res;
		}
	}
}

