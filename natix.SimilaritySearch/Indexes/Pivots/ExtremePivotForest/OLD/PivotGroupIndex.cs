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
// - Adaptation of the KNN searching algorithm of LAESA
// Eric Sadit
// - Parallel building
// - SearchRange SearchKNN optimizations

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

		protected virtual void InitGROUPS(int len)
		{
			this.GROUPS = new PivotGroup[len];
			for (int i = 0; i < len; ++i) {
				this.GROUPS[i] = new PivotGroup();
			}
		}
       
		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var num_groups = Input.ReadInt32 ();
			this.InitGROUPS (num_groups);
			//CompositeIO<PivotGroup>.LoadVector (Input, num_groups, this.GROUPS);
			foreach (var g in this.GROUPS) {
				g.Load(Input);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write ((int)this.GROUPS.Length);
			// CompositeIO<PivotGroup>.SaveVector (Output, this.GROUPS);
			foreach (var g in this.GROUPS) {
				g.Save(Output);
			}
		}

		public void Build (PivotGroupIndex pgi, int num_groups)
		{
			this.DB = pgi.DB;
			if (num_groups <= 0) {
				num_groups = pgi.GROUPS.Length;
			}
			this.InitGROUPS (num_groups);
			for (int i = 0; i < num_groups; ++i) {
				this.GROUPS[i] = pgi.GROUPS[i];
			}
		}

		public virtual void Build (MetricDB db, int num_groups, double alpha, int min_bs, bool do_far = true, int num_build_processors = -1, Func<PivotGroup> new_pivot_group = null)
		{
			this.DB = db;
			this.InitGROUPS (num_groups);
            var seeds = new int[ num_groups ];
            for (int i = 0; i < num_groups; ++i) {
                seeds[i] = RandomSets.GetRandomInt();
            }
			ParallelOptions ops = new ParallelOptions ();
			ops.MaxDegreeOfParallelism = num_build_processors;
			// Parallel.For (0, num_groups, ops, (i) => this.GROUPS[i] = this.GetGroup(percentil));
			int I = 0;
			var build_one_group = new Action<int> (delegate(int i) {
                if (new_pivot_group == null) {
                    this.GROUPS[i] = new PivotGroup();
                } else {
                    this.GROUPS[i] = new_pivot_group();
                }
				this.GROUPS[i].Build(this.DB, alpha, min_bs, seeds[i], do_far);
				// this.GROUPS [i] = this.GetGroup (alpha_stddev, min_bs);
				Console.WriteLine ("Advance {0}/{1} (alpha={2}, db={3}, timestamp={4})",
				                   I, num_groups, alpha, db.Name, DateTime.Now);
				I++;
			});

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

        public override IResult SearchKNN (object q, int K, IResult res)
        {       
            var l = this.GROUPS.Length;
            var n = this.DB.Count;
			short[] A = new short[this.DB.Count]; 
			int num_groups = this.GROUPS.Length;
			for (short groupID = 0; groupID < l; ++groupID) {
				var group = this.GROUPS[groupID];
				this.internal_numdists += group.SearchKNN(this.DB, q, K, res, A, groupID);
			}
			for (int docID = 0; docID < A.Length; ++docID) {
                if (A[docID] == num_groups) {
                    res.Push(docID, this.DB.Dist(q, this.DB[docID]));
                }
            }
            return res;
        }

		public override IResult SearchRange (object q, double radius)
		{
			var res = new ResultRange (radius, this.DB.Count);
			return this.SearchKNN(q, this.DB.Count, res);
		}
	}
}

