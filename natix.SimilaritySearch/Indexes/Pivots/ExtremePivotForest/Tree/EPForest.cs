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
	public class EPForest : BasicIndex
	{
		public EPTree[] forest;

		public EPForest ()
		{
		}

		protected virtual void InitForest(int len)
		{
			this.forest = new EPTree[len];
			for (int i = 0; i < len; ++i) {
				this.forest[i] = new EPTree();
			}
		}
       
		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var num = Input.ReadInt32 ();
			this.InitForest (num);
			//CompositeIO<PivotGroup>.LoadVector (Input, num_groups, this.GROUPS);
			foreach (var g in this.forest) {
				g.Load(Input);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write ((int)this.forest.Length);
			// CompositeIO<PivotGroup>.SaveVector (Output, this.GROUPS);
			foreach (var g in this.forest) {
				g.Save(Output);
			}
		}

		public void Build (EPForest f, int num_trees)
		{
			this.DB = f.DB;
			if (num_trees <= 0) {
				num_trees = f.forest.Length;
			}
			this.InitForest (num_trees);
			for (int i = 0; i < num_trees; ++i) {
				this.forest[i] = f.forest[i];
			}
		}


		public void Build (EPTable f, int num_trees)
		{
			this.DB = f.DB;
			if (num_trees <= 0) {
				num_trees = f.rows.Length;
			}
			this.InitForest (num_trees);
			for (int i = 0; i < num_trees; ++i) {
				this.forest[i] = new EPTree(f.rows[i]);
			}
		}

        public override IResult SearchKNN (object q, int K, IResult res)
        {
            var l = this.forest.Length;
            var n = this.DB.Count;
			short[] A = new short[n];
			ItemPair[] B = new ItemPair[n];

			var D_trees = new double[this.forest.Length][];
			for (int rowID = 0; rowID < this.forest.Length; ++rowID) {
				var pivs = this.forest[rowID].Pivs;
				var D = D_trees[rowID] = new double[pivs.Length];
				for (int pivID = 0; pivID < pivs.Length; ++pivID) {
					var objID = pivs[pivID].objID;
					D[pivID] = this.DB.Dist(q, this.DB[objID]);
					res.Push (objID, D[pivID]);
				}
				this.internal_numdists += pivs.Length;
			}
			for (short treeID = 0; treeID < l; ++treeID) {
				var t = this.forest[treeID];
				t.SearchKNN(this.DB, q, K, res, A, B, treeID, D_trees[treeID]);
			}
			for (int docID = 0; docID < A.Length; ++docID) {
                if (A[docID] == l && 
				    var d = this.DB.Dist(q, this.DB[docID]);
                    res.Push(docID, d);
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

