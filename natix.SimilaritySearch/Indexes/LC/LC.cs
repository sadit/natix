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
    public class LC : BasicIndex
    {
		class Node : ILoadSave
		{
			public int refID;
			public double cov;
			public List<int> bucket;

			public Node()
			{
			}

			public Node(int refID)
			{
				this.refID = refID;
				this.cov = 0;
				this.bucket = new List<int>();
			}

			public void Add(int docID, double dist)
			{
				this.cov = Math.Max (this.cov, dist);
				this.bucket.Add (docID);
			}


			public void Save(BinaryWriter Output)
			{
				Output.Write (this.refID);
				Output.Write (this.cov);
				Output.Write (this.bucket.Count);
				PrimitiveIO<int>.WriteVector (Output, this.bucket);
			}

			public void Load(BinaryReader Input)
			{
				this.refID = Input.ReadInt32 ();
				this.cov = Input.ReadDouble ();
				var len = Input.ReadInt32 ();
				this.bucket = new List<int> (len);
				PrimitiveIO<int>.ReadFromFile (Input, len, this.bucket);
			}

		}

		List<Node> node_list;

		public LC () : base()
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
		public virtual void Build (MetricDB db, int num_centers, Random rand)
		{
			this.DB = db;
			var n = this.DB.Count;
			var bsize = n / num_centers;
			// randomized has very good performance, even compared with more "intelligent" strategies
			var dseq = new DynamicSequentialOrdered ();
			dseq.Build (db, rand);
			this.node_list = new List<Node> (num_centers);
			var L = new List<ItemPair> (n);
			while (dseq.Count > 0) {
				if (this.node_list.Count % 100 == 0) {
					Console.WriteLine ("XXX {0}, num_centers: {1}, bsize: {5}, remain {2}/{3}, db: {4}, date-time: {6}", this, num_centers, dseq.Count, db.Count, db.Name, bsize, DateTime.Now);
				}
				var refID = dseq.GetAnyItem ();
				dseq.Remove (refID);
				L.Clear ();
				dseq.ComputeDistances (this.DB[refID], L);
				var near = new Result(bsize);
				var far = new Result (1);
				dseq.AppendKExtremes (near, far, L);
				var node = new Node (refID);
				this.node_list.Add (node);
				dseq.Remove (near);
				foreach (var p in near) {
					node.Add(p.docid, p.dist);
				}
			}
		}

        /// <summary>
        /// KNN search.
        /// </summary>
        public override IResult SearchKNN (object q, int K, IResult res)
        {
			int num_centers = this.node_list.Count;
			var D = new double[num_centers];
			for (int centerID = 0; centerID < num_centers; ++centerID) {
				var center_objID = this.node_list[centerID].refID;
				var d = this.DB.Dist(q, this.DB[center_objID]);
				D[centerID] = d;
				res.Push (center_objID, d);
			}
			this.internal_numdists += num_centers;
			for (int centerID = 0; centerID < num_centers; ++centerID) {
				var node = this.node_list[centerID];
				var rad = res.CoveringRadius;
				var dcq = D [centerID];
				if (dcq <= rad + node.cov) {
					foreach (var objID in node.bucket) {
						var d = this.DB.Dist(q, this.DB[objID]);
						res.Push(objID, d);
					}
					rad = res.CoveringRadius;
					if (dcq + rad <= node.cov) {
						break;
					}
				}
			}
			return res;
        }
    }
}
