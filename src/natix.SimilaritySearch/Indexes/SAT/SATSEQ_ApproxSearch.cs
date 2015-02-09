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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
    public class SATSEQ_ApproxSearch : SATSEQ
    {
		public Random rand = new Random();

        public SATSEQ_ApproxSearch ()
        {
        }

		public SATSEQ_ApproxSearch (SATSEQ sat) : base(sat)
		{
		}

        public override IResult SearchKNN (object q, int K, IResult res)
        {
			//var C = new Result (this.RepeatLocalMinima);
			this.GreedySearchGlobalMinima (this.root, q, res);
//			HashSet<int> history = new HashSet<int> ();
//			for (int i = 0; i < this.RepeatLocalMinima; ++i) {
//				var random_node = this.rand.Next (0, this.DB.Count);
//				this.GreedySearchLocalMinima(random_node, q, res, history);
//			}
//			foreach (var c in C) {
//				//this.GreedySearchGlobalMinima(c.docid, q, res, null);
//				base.SearchKNNNode(c.docid, q, res);
//			}
            return res;
        }
		
        public override IResult SearchRange (object q, double radius)
        {
			var res = new ResultRange (radius, this.DB.Count);
			return this.SearchKNN (q, this.DB.Count, res);
        }

		protected void GreedySearchLocalMinima (int node, object q, IResult res, HashSet<int> history)
		{
			if (history.Contains(node)) {
				return;
			}
			res.Push (node, this.DB.Dist (q, this.DB[node]));
			var rs = this.SEQ.Unravel (node);
			var children_count = rs.Count1;
			for (int rank = 1; rank <= children_count; ++rank) {
				var objID = rs.Select1(rank);
				if (!history.Contains(objID)) {
					var dist = this.DB.Dist(q, this.DB[objID]);
					res.Push (objID, dist);
					history.Add(objID);
				}
			}
			if (res.Count == 0) {
				// only possible in range queries
				return;
			}
			var first = res.First;
			if (first.ObjID == node) {
				var parent_objID = this.SEQ.Access(first.ObjID);
				this.GreedySearchLocalMinima (parent_objID, q, res, history);
			} else {
				this.GreedySearchLocalMinima (first.ObjID, q, res, history);
			}
			//			for (int childID = 0; childID < children_count; ++childID) {
			//				var child_objID = C[childID];
			//				var child_dist = D[childID];
			//				var radius = res.CoveringRadius;
			//				//Console.WriteLine ("---- cov: {0}", this.COV[child_objID]);
			//				if (child_dist <= radius + this.GetCOV(child_objID) && child_dist <= closer_dist + radius + radius) {
			//					this.SearchKNNNode(child_objID, q, res);
			//                }
			//            }
		}

		protected void GreedySearchGlobalMinima (int parent, object q, IResult res) //, Result C)
        {
			var rs = this.SEQ.Unravel (parent);
			var children_count = rs.Count1;
			var closer_dist = double.MaxValue;
			var closer_objID = -1;
			for (int rank = 1; rank <= children_count; ++rank) {
				var objID = rs.Select1(rank);
				var dist = this.DB.Dist(q, this.DB[objID]);
				res.Push (objID, dist);
				if (dist < closer_dist) {
					closer_dist = dist;
					closer_objID = objID;
				}
				//if (C != null) C.Push (objID, dist);
			}
			if (closer_objID >= 0) {
				this.GreedySearchGlobalMinima (closer_objID, q, res); //, C);
			}
//			for (int childID = 0; childID < children_count; ++childID) {
//				var child_objID = C[childID];
//				var child_dist = D[childID];
//				var radius = res.CoveringRadius;
//				//Console.WriteLine ("---- cov: {0}", this.COV[child_objID]);
//				if (child_dist <= radius + this.GetCOV(child_objID) && child_dist <= closer_dist + radius + radius) {
//					this.SearchKNNNode(child_objID, q, res);
//                }
//            }
        }
	}
}

