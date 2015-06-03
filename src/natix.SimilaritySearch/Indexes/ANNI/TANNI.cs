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
	public class TANNI: BasicIndex
	{
		public List<int> ACT;
		public int[] CT;
		public double[] DT;

		public TANNI ()
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

		public void PartialBuild (MetricDB db, PivotSelector pivsel)
		{
			this.DB = db;
			int n = this.DB.Count;
			this.ACT = new List<int>(256); // just a starting capacity
			this.CT = new int[n];
			this.DT = new double[n];

			var pivID = 0;
			var nextPivot = pivsel.NextPivot ();
			this.ACT.Add (nextPivot);
			this.DT [nextPivot] = 0.0;
			this.CT [nextPivot] = -1;
			var piv = this.DB [nextPivot];

			for (int docID = 0; docID < n; ++docID) {
				this.CT [docID] = pivID;
				this.DT [docID] = this.DB.Dist (this.DB [docID], piv);
			}
		}

		public void PromoteObjectToPivot(int nextPivot)
		{
			int n = this.DB.Count;
			int pivID = this.ACT.Count;
			this.ACT.Add (nextPivot);
			this.DT [nextPivot] = 0.0;
			this.CT [nextPivot] = -1;

			Dictionary<int, double> distcache = new Dictionary<int, double> (256);

			var piv = this.DB [this.ACT [pivID]];
			for (int docID = 0; docID < n; ++docID) {
				var c = this.CT [docID];
				if (c == -1) {
					continue;
				}
				double dcc;
				if (!distcache.TryGetValue (c, out dcc)) {
					dcc = this.DB.Dist (this.DB [this.ACT [c]], piv);
					distcache [c] = dcc;
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
