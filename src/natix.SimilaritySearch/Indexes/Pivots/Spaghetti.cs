//
//   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.IO;
using System.Collections.Generic;

using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class Spaghetti : BasicIndex
	{
		public class Node : ILoadSave {
			public int[] perm;
			public double[] distances;
			public int objID;

			public Node()
			{
				this.objID = -1;
			}

			public Node(MetricDB db, int pivID)
			{
				this.objID = pivID;
				var n = db.Count;
				this.perm = new int[n];
				this.distances = new double[n];
				var piv = db[pivID];
				for (int i = 0; i < n; ++i) {
					this.perm[i] = i;
					this.distances[i] = db.Dist(piv, db[i]);
				}
				Array.Sort(this.perm, (a, b) => this.distances[a].CompareTo(this.distances[b]));
			}

			public void Load(BinaryReader Input)
			{
				this.objID = Input.ReadInt32 ();
				var n = Input.ReadInt32 ();
				this.distances = new double[n];
				this.perm = new int[n];
				PrimitiveIO<double>.LoadVector (Input, n, this.distances);
				PrimitiveIO<int>.LoadVector (Input, n, this.perm);
			}

			public void Save(BinaryWriter Output)
			{
				Output.Write (this.objID);
				Output.Write (this.distances.Length);
				PrimitiveIO<double>.SaveVector (Output, this.distances);
				PrimitiveIO<int>.SaveVector (Output, this.perm);
			}

			public IEnumerable<int> IterateRange(IResult res, double dpq)
			{
				var min = 0; var max = this.distances.Length;
				while (min + 1 < max) {
					var mid = (min + max) >> 1;;
					var itemID = this.perm [mid];
					var d = this.distances [itemID];
					if (dpq < d) {
						max = mid;
					} else {
						min = mid;
					}
				}
			
				var n = this.distances.Length;
				var left = min;
				int right = left + 1;
				while (left >= 0 || right < n) {
					var rad = res.CoveringRadius;
					if (left >= 0) {
						var itemID = this.perm [left];
						if (dpq - rad <= this.distances[itemID]) {
							yield return itemID;
							--left;
						} else {
							left = -1;
						}
					}
					if (right < n) {
						var itemID = this.perm [right];
						if (this.distances[itemID] <= dpq + rad) {
							yield return itemID;
							++right;
						} else {
							right = n;
						}
					}
				}
			}

			public bool Discarded(int objID, double rad, double dqp)
			{
				if (objID == this.objID) {
					return true; // already added into res
				}
				return Math.Abs (dqp - this.distances[objID]) > rad;
			}
		}

		Node[] nodes;

		public Spaghetti () 
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			int m = Input.ReadInt32 ();
			this.nodes = new Node[m];

			CompositeIO<Node>.LoadVector (Input, m, this.nodes);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.nodes.Length);
			CompositeIO<Node>.SaveVector (Output, this.nodes);
		}

		/// <summary>
		/// Build the index
		/// </summary>
		public virtual void Build (MetricDB db, int m)
		{
			this.DB = db;
			var n = this.DB.Count;
			var pivsel = new PivotSelectorRandom (n, RandomSets.GetRandom ());
			this.nodes = new Node[m];
			for (int i = 0; i < m; ++i) {
				this.nodes [i] = new Node (db, pivsel.NextPivot ());
			}
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
		    int m = this.nodes.Length; // ~10% of the pivots will be seen
			var distances = new double[m];
			var sortedNodes = new Node[m];
			for (int i = 0; i < m; ++i) {
				sortedNodes [i] = this.nodes [i];
				var objID = this.nodes [i].objID;
				distances [i] = this.DB.Dist (q, this.DB [objID]);
				res.Push (objID, distances [i]);
			}
			this.internal_numdists += m;
			Array.Sort (distances, sortedNodes);
			foreach (var objID in sortedNodes[0].IterateRange(res, distances[0])) {
				bool discarded = false;
				var rad = res.CoveringRadius;
				for (int i = 1; i < m; ++i) {
					var node = sortedNodes [i];
					if (node.Discarded (objID, rad, distances [i])) {
						discarded = true;
						break;
					}
				}
				if (discarded) {
					continue;
				}
				var d = this.DB.Dist(q, this.DB[objID]);
				res.Push (objID, d);
			}
			return res;
		}
	}
}
