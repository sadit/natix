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
    public class LCrefs : BasicIndex
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

		public LCrefs () : base()
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
			// randomized has very good performance, even compared with more "intelligent" strategies
			var dseq = new DynamicSequentialOrdered ();
			dseq.Build (db, rand);
			this.node_list = new List<Node> (num_centers);
			while (dseq.Count > 0) {
				var refID = dseq.GetAnyItem ();
				dseq.Remove (refID);
				//dseq.xxxxxxxx aqui xxxxxxxx
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
        public override IResult SearchKNN (object q, int K, IResult res)
        {
			int num_centers = this.node_list.Count;
			var D = new double[num_centers];
			var min_dist = double.MaxValue;
			for (int centerID = 0; centerID < num_centers; ++centerID) {
				var center_objID = this.node_list[centerID].refID;
				var d = this.DB.Dist(q, this.DB[center_objID]);
				D[centerID] = d;
				min_dist = Math.Min (d, min_dist);
				res.Push (center_objID, d);
			}
			for (int centerID = 0; centerID < num_centers; ++centerID) {
				var node = this.node_list[centerID];
				var rad = res.CoveringRadius;
				if (min_dist + rad + rad >= D[centerID] && D[centerID] <= rad + node.cov) {
					foreach (var objID in node.bucket) {
						var d = this.DB.Dist(q, this.DB[objID]);
						res.Push(objID, d);
					}
				}
			}
			return res;
        }
    }
}

