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
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// List/Node based ILC
	/// </summary>
	public class XNANNI: BasicIndex
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

		public XNANNI ()
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

		public void Build (MetricDB db, ANNISetup setup, Boolean optimizeDistances)
		{
			this.PartialBuild (db, setup.Selector);
			var k = 1 + setup.ExpectedK; // since we use items from the database as training queries

			// select the queries to test the construction
			var qlist = RandomSets.GetRandomSubSet(setup.NumberQueries, this.DB.Count);

			double currT = long.MaxValue;
			double prevT = 0;
			double currD = this.DB.Count;
			double prevD = 0;
			int iter = 0;
			Console.WriteLine("xxxxxxxx BEGIN> db: {0}, setup: {setup}",
				Path.GetFileName(this.DB.Name), setup);

			do {
				for (int s = 0; s < setup.StepWidth; ++s) {
					this.PromoteObjectToPivot(setup.Selector.NextPivot());
				}
				prevT = currT;
				prevD = currD;
				currT = DateTime.Now.Ticks;
				foreach (var qID in qlist) {
					var q = this.DB[qID];
					var res = new Result(k);
					currD += this.InternalSearchKNN(q, k, res);
				}
				currT = DateTime.Now.Ticks - currT;
				currT /= qlist.Length;
				currD /= qlist.Length;
				++iter;

				Console.WriteLine ("======> iter: {0}, timestamp: {1}, setup: {2}", iter, DateTime.Now, setup);
				Console.WriteLine ("------> prevT: {0}, currT: {1}, prevT / currT: {2}", prevT, currT, prevT / currT);
				Console.WriteLine ("------> prevD: {0}, currD: {1}, prevD / currD: {2}", prevD, currD, prevD / currD);
				if (optimizeDistances) {
					if (prevD > currD * (1 + setup.AlphaStop)) {
						break;
					}
				} else {
					if (prevT > currT * (1 + setup.AlphaStop)) {
						break;
					}
				}
			} while (true);
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			this.InternalSearchKNN (q, K, res);
			return res;
		}

		public int InternalSearchKNN (object q, int K, IResult res)
		{
			var m = this.clusters.Count;
			var cost = m;
			this.internal_numdists += m;
			var dcq_cache = new double[m];
			var order = new int[m];

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
				order [centerID] = centerID;
			}

			Sorting.Sort(order, (a, b) => dcq_cache[a].CompareTo(dcq_cache[b]));
			// for (int centerID = 0; centerID < m; ++centerID) {
			foreach (var centerID in order) {
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
						++cost;
						res.Push (docID, d);
					}
				}
			}
			return cost;
		}
	} 
}
