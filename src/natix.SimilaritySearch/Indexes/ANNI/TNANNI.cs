//
//  Copyright 2015  Luis Guillermo Ruiz / Eric S. Tellez <eric.tellez@infotec.com.mx>
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
	public class TNANNI: BasicIndex
	{
		public class Node : ILoadSave {
			public int objID;
			public List<int> objects;
			public List<double> dists;
			public double cov;

			public Node () {
				this.objects = new List<int>();
				this.dists = new List<double>();
				this.cov = 0;
			}
	
			public Node (int _objID)
			{
				this.objID=_objID;
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

			// finds the covering radius amoung the object
			public void Get_cov()
			{
				this.cov = 0;
				foreach (double d in this.dists) {
					if (this.cov < d) {
						this.cov = d;
					}
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

		public List<Node> clusters;

		public TNANNI ()
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

		public void PromoteObjectToPivot(int pivID)
		{
			// Add new center
			this.clusters.Add(new Node(pivID));
			var piv = this.DB [pivID];

			// the new center takes its closest elemens
			foreach (Node center in this.clusters)
			{
				if (center.objID == pivID) // is the new center
					continue;

				int c = center.objID;
				double dcc=this.DB.Dist (this.DB [c], piv);

				if (center.cov * 2 < dcc) // all the points in the cell are too far.
					continue;

				bool get_cov = false;
				// the new center tries to steal the elements of the actual center
				// Console.WriteLine("START> ===== {0}", c);
				for (int i = 0; i < center.objects.Count; i++) {
					int docID = center.objects [i];
					if (dcc <= 2 *  center.dists[i]) { // <= 2 * (...) or <= 4*(...)
						//var dmin = this.DT [docID];
						//if (Math.Abs(dmin - dcc) <= dmin) {
						var d = this.DB.Dist (this.DB [docID], piv);
						if (d < center.dists[i]) { 	// add to the new center
							this.clusters [this.clusters.Count - 1].Add (docID, d);
							// remove from center.objects
							if (center.dists [i] == center.cov) { // the covering radius must be found again
								get_cov = true;
							}
							int lastID = center.objects.Count - 1;
							center.dists[i] = center.dists[lastID];
							center.objects[i] = center.objects[lastID];
							center.dists.RemoveAt (lastID);
							center.objects.RemoveAt (lastID);
							--i;
						}
					}
				}
				if (get_cov)
					center.Get_cov ();
			}
		}

		public void PartialBuild (MetricDB db, PivotSelector pivsel)
		{
			this.DB = db;
			int n = this.DB.Count;
			this.clusters.Clear();

			Node node = new Node ();
			var pivID = pivsel.NextPivot ();
			node.objID = pivID;
			this.clusters.Add (node);

			// take distances from all points to the center
			var piv = this.DB [pivID];
			for (int docID = 0; docID < n; ++docID) {
				if (pivID == docID) {
					continue;
				}
				var d = this.DB.Dist (this.DB [docID], piv);
				node.Add (docID, d);
			}
		}

		public void Build (MetricDB db, int k, PivotSelector pivsel)
		{
			this.PartialBuild (db, pivsel);
			++k; // since we use items from the database as training queries

			// select the queries to test the construction
			var qlist = new List<int>();
			for (int i = 0; i < 64; ++i) {
				qlist.Add(pivsel.NextPivot());
			}

			int step_width = 128;
			long curr = long.MaxValue;
			long prev = 0L;
			int iter = 0;

			do {
				for (int s = 0; s < step_width; ++s) {
					this.PromoteObjectToPivot(pivsel.NextPivot());
				}
				prev = curr;
				curr = DateTime.Now.Ticks;
				foreach (var qID in qlist) {
					var q = this.DB[qID];
					var res = new Result(k);
					this.SearchKNN(q, k, res);
				}
				curr = DateTime.Now.Ticks - curr;
				++iter;
				Console.WriteLine("xxxxxxxx> iter: {0}, current-search-time: {1}, timestamp: {2}", iter, curr / 1e8, DateTime.Now);
			} while (prev > curr * 1.01);
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
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

			for (int centerID = 0; centerID < m; ++centerID) {
				var node = this.clusters [centerID];
				var center = this.DB [node.objID]; 
				var dcq = dcq_cache [centerID];
				var rad = res.CoveringRadius;
				if (dcq > node.cov + rad) {
					continue;
				}

				if (dcq > min_dist + rad + rad) {
					continue;
				}
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
