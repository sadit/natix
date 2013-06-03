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
    public class SATSEQ : BasicIndex
    {
		public Sequence SEQ;
		public List<double> COV;
		public int root;
		public GGMN COV_ZERO;

        public SATSEQ ()
        {
        }

		public SATSEQ (SATSEQ sat)
		{
			this.Build (sat);
		}

		public void Build(SATSEQ sat)
		{
			this.DB = sat.DB;
			this.SEQ = sat.SEQ;
			this.COV = sat.COV;
			this.root = sat.root;
			this.COV_ZERO = sat.COV_ZERO;
		}

		public double GetCOV(int index)
		{
			if (this.COV_ZERO.Access (index)) {
				return 0;
			} else {
				var rank1 = this.COV_ZERO.Rank0(index);
				return this.COV[rank1-1];
			}
		}

		public void Build(SAT sat)
		{
			var n = sat.DB.Count;
			this.DB = sat.DB;
			var cov = new double[n];
			var seq = new int[n];
			this.root = sat.root.objID;
			int nonzeros = 0;
			var visit = new Action<SAT.Node,SAT.Node>((parent, child) => {
				seq[child.objID] = (parent == null) ? n : parent.objID;
				cov[child.objID] = child.cov;
				if (child.cov > 0) ++nonzeros;
			});
			visit (null, sat.root);
			this.Build_IterateSAT (sat.root, visit);
			var listibuilder = ListIBuilders.GetArray ();
			var permbuilder = PermutationBuilders.GetCyclicPerms (1, listibuilder, listibuilder);
			var seqbuilder = SequenceBuilders.GetSeqSinglePerm (permbuilder);
			this.SEQ = seqbuilder (seq, n + 1);
			this.COV = new List<double> (nonzeros);
			var cov_zero = new BitStream32 ();
			cov_zero.Write (true, n);
			for (int objID = 0; objID < n; ++objID) {
				if (cov[objID] > 0) {
					this.COV.Add(cov[objID]);
					cov_zero[objID] = false;
				}
			}
			this.COV_ZERO = new GGMN ();
			this.COV_ZERO.Build (cov_zero, 8);
		}

		void Build_IterateSAT(SAT.Node node, Action<SAT.Node, SAT.Node> visit)
		{
			foreach (var child in node.Children) {
				visit(node, child);
				this.Build_IterateSAT(child, visit);
			}
		}

        public override void Load (BinaryReader Input)
        {
            base.Load (Input);
			this.root = Input.ReadInt32 ();
			this.SEQ = GenericIO<Sequence>.Load (Input);
			var len = Input.ReadInt32 ();
			this.COV = new List<double> (len);
			PrimitiveIO<double>.ReadFromFile (Input, len, this.COV);
			this.COV_ZERO = new GGMN ();
			this.COV_ZERO.Load (Input);
        }

        public override void Save (BinaryWriter Output)
        {
            base.Save(Output);
			Output.Write (this.root);
			GenericIO<Sequence>.Save (Output, this.SEQ);
			Output.Write (this.COV.Count);
			PrimitiveIO<double>.WriteVector (Output, this.COV);
			this.COV_ZERO.Save (Output);
        }

        public override IResult SearchKNN (object q, int K, IResult res)
        {
			var dist = this.DB.Dist(q, this.DB[this.root]);
			res.Push (this.root, dist);
			if (dist <= res.CoveringRadius + this.GetCOV(this.root)) {
				this.SearchKNNNode (this.root, q, res);
			}
            return res;
        }
		
        public override IResult SearchRange (object q, double radius)
        {
			var res = new ResultRange (radius, this.DB.Count);
			return this.SearchKNN (q, this.DB.Count, res);
        }

        protected virtual void SearchKNNNode (int parent, object q, IResult res)
        {
			var rs = this.SEQ.Unravel (parent);
			var children_count = rs.Count1;
			var D = new double[children_count];
			var C = new int[children_count];
			var closer_dist = double.MaxValue;
			for (int rank = 1; rank <= children_count; ++rank) {
				var objID = rs.Select1(rank);
				var dist = this.DB.Dist(q, this.DB[objID]);
				res.Push (objID, dist);
				D[rank-1] = dist;
				C[rank-1] = objID;
				if (dist < closer_dist) {
					closer_dist = dist;
				}
			}
			for (int childID = 0; childID < children_count; ++childID) {
				var child_objID = C[childID];
				var child_dist = D[childID];
				var radius = res.CoveringRadius;
				//Console.WriteLine ("---- cov: {0}", this.COV[child_objID]);
				if (child_dist <= radius + this.GetCOV(child_objID) && child_dist <= closer_dist + radius + radius) {
					this.SearchKNNNode(child_objID, q, res);
                }
            }
        }
	}
}

