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
	public class DiskPivotGroupIndex : BasicIndex
	{
		public DiskPivotGroup[] GROUPS;

		public DiskPivotGroupIndex ()
		{
		}
       
		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var num_groups = Input.ReadInt32 ();
			this.GROUPS = new DiskPivotGroup[num_groups];
			CompositeIO<DiskPivotGroup>.LoadVector (Input, num_groups, this.GROUPS);
			foreach (var g in this.GROUPS) {
				g.CachePivObjects(this.DB);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write ((int)this.GROUPS.Length);
			CompositeIO<DiskPivotGroup>.SaveVector (Output, this.GROUPS);
		}

		public void Build (string basename, PivotGroupIndex pgi, int num_groups = 0)
		{
			this.DB = pgi.DB;
			if (num_groups <= 0) {
				num_groups = pgi.GROUPS.Length;
			}
			this.GROUPS = new DiskPivotGroup[num_groups];
			for (int i = 0; i < num_groups; ++i) {
				var g = new DiskPivotGroup();
				g.Build (pgi.GROUPS[i], basename + "-" + i);
				this.GROUPS[i] = g;
				this.GROUPS[i].CachePivObjects(this.DB);
			}
		}

        public override IResult SearchKNN (object q, int K, IResult res)
        {       
            var l = this.GROUPS.Length;
            var n = this.DB.Count;
			short[] A = new short[this.DB.Count]; 
			int review_groups = 0;
			foreach (var group in this.GROUPS) {
				++review_groups;
				int abs_pos = 0;
				for (int pivID = 0; pivID < group._Pivs.Length; ++pivID) {
					var piv = group._Pivs[pivID];
					var pivOBJ = group._PivObjects[pivID];
					//foreach (var piv in group._Pivs) {
					// var pivOBJ = this.DB[piv.objID];
					var dqp = this.DB.Dist(q, pivOBJ);
					res.Push (piv.objID, dqp);
					++this.internal_numdists;
					// checking near ball radius
					if (dqp <= piv.last_near + res.CoveringRadius) {
						for (int nearID = 0; nearID < piv.num_near; ++nearID, ++abs_pos) {
							var item = group._Items[abs_pos];
							// checking covering pivot
							if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
								++A[item.objID];
							}
						}
					} else {
						abs_pos+= piv.num_near;
					}
					// checking external radius
					if (dqp + res.CoveringRadius >= piv.first_far) {
						for (int j = 0; j < piv.num_far; ++j, ++abs_pos) {
							var item = group._Items[abs_pos];
							// checking covering pivot
							if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
								++A[item.objID];
							}
						}
					} else {
						abs_pos+= piv.num_far;
					}
					if (dqp + res.CoveringRadius <= piv.last_near || piv.first_far <= dqp - res.CoveringRadius) {
						break;
					}
				}
			}
			for (int docID = 0; docID < A.Length; ++docID) {
                if (A[docID] == review_groups) {
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

