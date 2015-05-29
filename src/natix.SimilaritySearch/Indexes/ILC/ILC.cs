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
	public class ILC: BasicIndex
	{
		public List<int> ACT;
		public int[] CT;
		public double[] DT;

		public ILC ()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			var n = this.DB.Count;
			var m = Input.ReadInt32();
			this.ACT = new List<int>(m);
			this.CT = new int [n];
			this.DT = new double [n];
			PrimitiveIO<int>.LoadVector (Input, m, this.ACT);
			PrimitiveIO<int>.LoadVector (Input, n, this.CT);
			PrimitiveIO<double>.LoadVector (Input, n, this.DT);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.ACT.Count);
			PrimitiveIO<int>.SaveVector(Output, this.ACT);
			PrimitiveIO<int>.SaveVector(Output, this.CT);
			PrimitiveIO<double>.SaveVector(Output, this.DT);
		}

		public void Build (MetricDB db, int k, int step_width, int num_indexes, PivotSelector pivsel = null)
		{
			if (pivsel == null) {
				pivsel = new PivotSelectorRandom (db.Count, RandomSets.GetRandom ());
			}
			this.InternalBuild(k, 0, 1, db, step_width, num_indexes, pivsel);
		}

		public struct BuildSearchCost {
			public double CompositeCost;
			public double SingleCost;
		}

		public BuildSearchCost InternalBuild(int k, int leader_num_centers, double leader_review_prob, MetricDB db, int step_width, int num_indexes, PivotSelector pivsel)
		{
			this.DB = db;
			int n = this.DB.Count;
			++k; // since we use items from the database as training queries
			this.ACT = new List<int>(256); // just a starting size
			this.CT = new int[n];
			this.DT = new double[n];
			var pivID = 0;
			this.ACT.Add (pivsel.NextPivot());
			var piv = this.DB [this.ACT[pivID]];

			for (int docID = 0; docID < n; ++docID) {
				var d = this.DB.Dist (this.DB [docID], piv);
				this.CT [docID] = pivID;
				this.DT [docID] = d;
			}

			var cache = new Dictionary<int, double> (256);
			var qlist = new List<int>();
			for (int i = 0; i < 64; ++i) {
				qlist.Add(pivsel.NextPivot());
			}

			Console.WriteLine("xxxxxxxx BEGIN> db: {0}, step: {1}, indexes: {2}, k: {3}, timestamp: {4}",
				Path.GetFileName(this.DB.Name), step_width, num_indexes, k, DateTime.Now);

			int iterID = 0;
			var cost = new BuildSearchCost ();

			long curr = n;
			long prev = n;

			do {
				for (int s = 0; s < step_width; ++s, ++iterID) {
					cache.Clear ();
					pivID = this.ACT.Count;
					var nextPivot = pivsel.NextPivot ();
					this.ACT.Add (nextPivot);
					this.CT [nextPivot] = -1;
					this.DT [nextPivot] = 0.0;

					piv = this.DB [this.ACT[pivID]];
					for (int docID = 0; docID < n; ++docID) {
						var c = this.CT [docID];
						if (c == -1) {
							continue;
						}
						double dcc;
						if (!cache.TryGetValue (c, out dcc)) {
							dcc = this.DB.Dist (this.DB [this.ACT[c]], piv); // <- incorrecto? deberia ser ACT[c]?, porque entonces funcionan algunas cosas?
							cache [c] = dcc;
						}

					    if (dcc <= 2 * this.DT [docID]) {
						//var dmin = this.DT [docID];
						//if (Math.Abs(dmin - dcc) <= dmin) {
							var d = this.DB.Dist (this.DB [docID], piv);
							if (d < this.DT [docID]) {
								this.CT [docID] = pivID;
								this.DT [docID] = d;
							}
						}
					}
				}
				cost.SingleCost = 0;
				foreach (var qID in qlist) {
					cost.SingleCost += this.InternalSearchKNN(this.DB[qID], new Result(k));
				}
				cost.SingleCost /= qlist.Count;
				double _prob = (Math.Max(cost.SingleCost - iterID, 1.0)) / n;
				_prob = Math.Pow (_prob, num_indexes) * leader_review_prob;

				var internal_cost = iterID * num_indexes + leader_num_centers;
				var external_cost = n * _prob;
				cost.CompositeCost = internal_cost + external_cost;

				prev = curr;
				curr = (long)cost.CompositeCost;
				Console.WriteLine("---- {0}/{1}> #pivots: {2}, prev-cost: {3}, curr-cost: {4}, #idx: {5}, timestamp: {6}",
					this, Path.GetFileName(this.DB.Name), this.ACT.Count, prev, curr, num_indexes, DateTime.Now);
			} while (prev > curr * 1.001);
			return cost;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			this.InternalSearchKNN (q, res);
			return res;
		}

		protected int InternalSearchKNN (object q, IResult res)
		{
			var n = this.DB.Count;
			var dcq_cache = new double[this.ACT.Count];
			for (int i = 0; i < this.ACT.Count; ++i) {
				var objID = this.ACT[i];
				var dcq = this.DB.Dist (q, this.DB[objID]);
				dcq_cache [i] = dcq;
				res.Push (objID, dcq);
			}
			int cost = this.ACT.Count;
			this.internal_numdists += cost;

			for (var docID = 0; docID < n; ++docID) {
				var c = this.CT [docID];
				if (c == -1) continue;
				double dcq = dcq_cache[c];
				if (Math.Abs (dcq - this.DT[docID]) <= res.CoveringRadius) {
					var d = this.DB.Dist (this.DB [docID], q);
					res.Push (docID, d);
					++cost;
				}
			}
			return cost;
		}
	}
}
