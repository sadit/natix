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
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
    public class DualVor : BasicIndex
    {
		class Node : ILoadSave
		{
			public int refID;
			public double cov_radius;
			public double ext_radius;
			public List<int> bucket_near;
			public List<int> bucket_far;
			public Node()
			{
			}

			public Node(int refID)
			{
				this.refID = refID;
				this.cov_radius = 0;
				this.ext_radius = double.MaxValue;
				this.bucket_near = new List<int>();
				this.bucket_far = new List<int>();
			}

			public void AddNear(int docID, double dist)
			{
				this.cov_radius = Math.Max (this.cov_radius, dist);
				this.bucket_near.Add (docID);
			}

			public void AddFar(int docID, double dist)
			{
				this.ext_radius = Math.Min (this.ext_radius, dist);
				this.bucket_far.Add (docID);
			}

			public void Save(BinaryWriter Output)
			{
				Output.Write (this.refID);
				Output.Write (this.cov_radius);
				Output.Write (this.ext_radius);
				Output.Write (this.bucket_near.Count);
				PrimitiveIO<int>.WriteVector (Output, this.bucket_near);
				Output.Write (this.bucket_far.Count);
				PrimitiveIO<int>.WriteVector (Output, this.bucket_far);
			}

			public void Load(BinaryReader Input)
			{
				this.refID = Input.ReadInt32 ();
				this.cov_radius = Input.ReadDouble ();
				this.ext_radius = Input.ReadDouble ();
				var len = Input.ReadInt32 ();
				this.bucket_near = new List<int> (len);
				PrimitiveIO<int>.ReadFromFile (Input, len, this.bucket_near);
				len = Input.ReadInt32 ();
				this.bucket_far = new List<int> (len);
				PrimitiveIO<int>.ReadFromFile (Input, len, this.bucket_far);
			}

		}

		List<Node> node_list;

		public DualVor () : base()
        {
        }


		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			var len = Input.ReadInt32 ();
			this.node_list = new List<Node> (len);
			CompositeIO<Node>.LoadVector (Input, len, this.node_list);
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.node_list.Count);
			CompositeIO<Node>.SaveVector (Output, this.node_list);
		}

		/// <summary>
		/// Build the index
		/// </summary>
		public virtual void Build (MetricDB db, int num_centers, Random rand, SequenceBuilder seq_builder = null)
		{
			this.DB = db;
			var n = this.DB.Count;
			// randomized has very good performance, even compared with more "intelligent" strategies
			this.node_list = new List<Node> (num_centers);
			{
				var subset = RandomSets.GetRandomSubSet (num_centers, this.DB.Count, rand);
				for (int centerID = 0; centerID < num_centers; ++centerID) {
					this.node_list.Add (new Node (subset [centerID]));
				}
			}
			for (int docID = 0; docID < n; ++docID) {
				var near = new Result(1);
				var far = new Result(1);
				for (var centerID = 0; centerID < num_centers; ++centerID) {
					var node = this.node_list[centerID];
					var d = this.DB.Dist(this.DB[node.refID], this.DB[docID]);
					near.Push(centerID, d);
					far.Push(centerID, -d);
				}
				var _near = near.First;
				var _far = far.First;
				this.node_list[_near.docid].AddNear(docID, _near.dist);
				this.node_list[_far.docid].AddFar(docID, -_far.dist);
			}
		}


        /// <summary>
        /// Search the specified q with radius qrad.
        /// </summary>
        public override IResult SearchRange (object q, double qrad)
        {
			var res = new ResultRange (qrad, this.DB.Count);
			this.SearchKNN (q, this.DB.Count, res);
			return res;
        }
        
        /// <summary>
        /// KNN search.
        /// </summary>
        public override IResult SearchKNN (object q, int K, IResult R)
        {
			throw new NotImplementedException();
//            var sp = this.DB;
//            int num_centers = this.CENTERS.Count;
//            var C = this.DB.CreateResult (num_centers, false);
//            for (int centerID = 0; centerID < num_centers; centerID++) {
//                var dcq = sp.Dist (this.DB [this.CENTERS [centerID]], q);
//                ++this.internal_numdists;
//                R.Push (this.CENTERS [centerID], dcq);
//                if (dcq <= R.CoveringRadius + this.COV [centerID]) {
//                    C.Push (centerID, dcq);
//                }
//            }
//            var closer = C.First;
//            foreach (ResultPair pair in C) {
//                var dcq = pair.dist;
//                var center = pair.docid;
//                if (dcq <= closer.dist + 2 * R.CoveringRadius &&
//                    dcq <= R.CoveringRadius + this.COV [center]) {
//                    var rs = this.SEQ.Unravel (center);
//                    var count1 = rs.Count1;
//                    for (int i = 1; i <= count1; i++) {
//                        var u = rs.Select1 (i);
//                        var r = sp.Dist (q, sp [u]);
//                        //if (r <= qr) { // already handled by R.Push
//                        R.Push (u, r);
//                    }
//                }
//            }
//            return R;
        }
    }
}

