//
//  Copyright 2014  Eric S. Tellez <eric.tellez@infotec.com.mx>
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
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// List/Node based ILC
	/// </summary>
	public class NILC: BasicIndex
	{
		public class Node : ILoadSave {
			public int objID;
			public List<int> objects;
			public List<double> dists;
			public double cov; // covering radius

			public Node () {
				this.objects = new List<int>();
				this.dists = new List<double>();
				this.cov = 0;
			}
	
			public void Add(int obj, double dist) {
				this.objects.Add (obj);
				this.dists.Add (dist);
				if (this.cov < dist) {
					this.cov = dist;
				}
			}

			public void Save(BinaryWriter Output) {
				int n = this.objects.Count;
				Output.Write (this.objID);
				Output.Write (n);
				Output.Write (this.cov);
				PrimitiveIO<int>.SaveVector (Output, this.objects);
				PrimitiveIO<double>.SaveVector (Output, this.dists);
			}

			public void Load(BinaryReader Input) {
				this.objID = Input.ReadInt32 ();
				int n = Input.ReadInt32 ();
				this.cov = Input.ReadDouble ();
				PrimitiveIO<int>.LoadVector (Input, n, this.objects);
				PrimitiveIO<double>.LoadVector (Input, n, this.dists);
			}
		}

		public List<Node> clusters; // the centers

		public NILC ()
		{
			this.clusters = new List<Node> ();
		}


		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			int n = Input.ReadInt32 ();
			CompositeIO<Node>.LoadVector (Input, n, this.clusters);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.clusters.Count);
			CompositeIO<Node>.SaveVector (Output, this.clusters);
		}

		public void Build (ILC ilc)
		{
			this.DB = ilc.DB;
			int n = this.DB.Count;
			this.clusters.Clear();

			foreach (var objID in ilc.ACT) {
				var node = new Node();
				node.objID = objID;
				this.clusters.Add (node);
			}

			for (int objID = 0; objID < n; ++objID) {
				var c = ilc.CT [objID];
				if (c == -1) continue;
				this.clusters[c].Add (objID, ilc.DT [objID]);
			}
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			// take the distance from q to the centers.
			var dcq_cache = new double[this.clusters.Count];
			var m = this.clusters.Count;
			double min_dist = double.MaxValue;
			for (int centerID = 0; centerID < m; ++centerID) {
				var node = this.clusters [centerID];
				var center = this.DB [node.objID];
				var dcq = this.DB.Dist (center, q);
				dcq_cache [centerID] = dcq;
				res.Push (node.objID, dcq);
				if (dcq < min_dist) {
					min_dist = dcq;
				}
			}
			this.internal_numdists += m;
			// range queries can be empty at this point
			// var min_dist = res.First.Dist;

			// check for the cells of all the centers
			for (int centerID = 0; centerID < m; ++centerID) {
				var node = this.clusters [centerID];
				var center = this.DB [node.objID]; 
				var dcq = dcq_cache [centerID];
				var rad = res.CoveringRadius;

				if (dcq > node.cov + rad) { // the elements of the center are not in the query ball
					continue;
				}

				if (dcq > min_dist + rad + rad) { // the query is in a cell too far from the center.
					continue;
				}
				// check for the elements in the cell of actual center
				int l = node.dists.Count;
				for (int i = 0; i < l; ++i) {
					var lower = dcq - node.dists[i];
					if (lower < 0) {
						lower = -lower;
					}
					if (lower <= rad) {
						var docID = node.objects [i];
						var d = this.DB.Dist (this.DB [docID], q);
						res.Push (docID, d);
					}
				}
			}
			return res;
		}
	} 
}
