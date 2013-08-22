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
    public class VorFar : BasicIndex
    {
		class Node : ILoadSave
		{
			public int refID;
			public double ext_radius;
			public List<int> bucket;
			public Node()
			{
			}

			public Node(int refID)
			{
				this.refID = refID;
				this.ext_radius = double.MaxValue;
				this.bucket = new List<int>();
			}

			public void Add(int docID, double dist)
			{
				this.ext_radius = Math.Min (this.ext_radius, dist);
				this.bucket.Add (docID);
			}

			public void Save(BinaryWriter Output)
			{
				Output.Write (this.refID);
				Output.Write (this.ext_radius);
				Output.Write (this.bucket.Count);
				PrimitiveIO<int>.SaveVector (Output, this.bucket);
			}

			public void Load(BinaryReader Input)
			{
				this.refID = Input.ReadInt32 ();
				this.ext_radius = Input.ReadDouble ();
				var len = Input.ReadInt32 ();
				this.bucket = new List<int> (len);
				PrimitiveIO<int>.LoadVector (Input, len, this.bucket);
			}

		}

		List<Node> node_list;

		public VorFar () : base()
        {
        }

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			var len = Input.ReadInt32 ();
			this.node_list = new List<Node> (len);
			CompositeIO<Node>.LoadVector (Input, len, this.node_list);
			int i = 0;
			foreach (var node in this.node_list) {
				Console.WriteLine ("===== bucket: {0}, len: {1}", i, node.bucket.Count);
				++i;
			}
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
			var subset = RandomSets.GetRandomSubSet (num_centers, this.DB.Count, rand);
			for (int centerID = 0; centerID < num_centers; ++centerID) {
				this.node_list.Add (new Node (subset [centerID]));
			}
			var H = new HashSet<int> (subset);
			for (int docID = 0; docID < n; ++docID) {
				if (docID % 1000 == 0) {
					Console.WriteLine ("== {0} {1}/{2}, num_centers: {3}, db: {4}", this, docID + 1, n, num_centers, db.Name);
				}
				if (H.Contains(docID)) {
					continue;
				}
				var far = new Result(1);
				for (var centerID = 0; centerID < num_centers; ++centerID) {
					var node = this.node_list[centerID];
					var d = this.DB.Dist(this.DB[node.refID], this.DB[docID]);
					far.Push(centerID, -d);
				}
				var _far = far.First;
				this.node_list[_far.docid].Add(docID, -_far.dist);
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
			var max_dist = double.MinValue;
			for (int centerID = 0; centerID < num_centers; ++centerID) {
				var center_objID = this.node_list[centerID].refID;
				var d = this.DB.Dist(q, this.DB[center_objID]);
				D[centerID] = d;
				max_dist = Math.Max (d, max_dist);
				res.Push (center_objID, d);
			}
			for (int centerID = 0; centerID < num_centers; ++centerID) {
				var node = this.node_list[centerID];
				var rad = res.CoveringRadius;
				if (max_dist + rad >= D[centerID] - rad && D[centerID] + rad >= node.ext_radius) {
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

