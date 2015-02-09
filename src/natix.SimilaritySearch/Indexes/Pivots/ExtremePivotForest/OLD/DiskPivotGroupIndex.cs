//
//  Copyright 2013  Eric Sadit Tellez Avila
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
			}
		}

        public override IResult SearchKNN (object q, int K, IResult res)
        {       
//            var l = this.GROUPS.Length;
//            var n = this.DB.Count;
			short[] A = new short[this.DB.Count];
			int num_groups = this.GROUPS.Length;
			foreach (var group in this.GROUPS) {
				this.internal_numdists += group.SearchKNN(this.DB, q, K, res, A);
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

