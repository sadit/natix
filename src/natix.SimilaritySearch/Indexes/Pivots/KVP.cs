// //
// //   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
// //
// //   Licensed under the Apache License, Version 2.0 (the "License");
// //   you may not use this file except in compliance with the License.
// //   You may obtain a copy of the License at
// //
// //       http://www.apache.org/licenses/LICENSE-2.0
// //
// //   Unless required by applicable law or agreed to in writing, software
// //   distributed under the License is distributed on an "AS IS" BASIS,
// //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //   See the License for the specific language governing permissions and
// //   limitations under the License.

using System;
using System.IO;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class KVP : BasicIndex
	{
		public int K;
		public List<int> pivs = new List<int>();
		public List<ItemPair[]> assocpivots = new List<ItemPair[]> ();
		public List<byte> ispivot = new List<byte>();

		public KVP ()
		{
		}
		
		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var n = this.DB.Count;
			this.K = Input.ReadInt32();
			int m = Input.ReadInt32 ();
			PrimitiveIO<int>.LoadVector (Input, m, this.pivs);
			PrimitiveIO<byte>.LoadVector (Input, n, this.ispivot);

			var kk = this.K + this.K;
			for (int i = 0; i < n; ++i) {
				var parray = new ItemPair[kk];
				CompositeIO<ItemPair>.LoadVector (Input, kk, parray);
				this.assocpivots.Add (parray);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			var n = this.DB.Count;
			Output.Write (this.K);
			Output.Write (this.pivs.Count);
			PrimitiveIO<int>.SaveVector (Output, this.pivs);
			PrimitiveIO<byte>.SaveVector (Output, this.ispivot);

			for (int i = 0; i < n; ++i) {
				CompositeIO<ItemPair>.SaveVector (Output, this.assocpivots [i]);
			}
		}

		/// <summary>
		/// Build the index
		/// </summary>
		public virtual void Build (MetricDB db, int k, int m)
		{
			this.DB = db;
			var n = this.DB.Count;
			this.K = k;
			this.pivs.AddRange (RandomSets.GetRandomSubSet (m, n));

			this.ispivot.Capacity = this.assocpivots.Capacity = n;
			this.ispivot.Clear ();
			this.assocpivots.Clear ();

			for (int objID = 0; objID < n; ++objID) {
				this.assocpivots.Add (new ItemPair[k + k]);
				this.ispivot.Add (0);
			}

			for (int pivID = 0; pivID < m; ++pivID) {
				var objID = this.pivs [pivID];
				this.ispivot [objID] = 1;
			}

			for (int objID = 0; objID < n; ++objID) {
				var near = new Result (k);
				var far = new Result (k);
				var obj = this.DB [objID];
				for (int pivID = 0; pivID < m; ++pivID) {
					var piv = this.DB [this.pivs [pivID]];
					var d = this.DB.Dist (obj, piv);
					near.Push (pivID, d); // this can be buggy when k * 2 < n or dist() is too flat
					far.Push (pivID, -d);
				}
				var _assoc = this.assocpivots [objID];

				{
					int i = 0;
					foreach (var p in near) {
						_assoc [i] = new ItemPair (p.ObjID, p.Dist);
						++i;
					}
					foreach (var p in far) {
						_assoc [i] = new ItemPair (p.ObjID, -p.Dist);
						++i;
					}
				}
				if (objID % 5000 == 0) {
					Console.WriteLine ("=== db: {0}, k: {1}, available: {2}, advance: {3}/{4}, timestamp: {5}",
					                   Path.GetFileName(db.Name), k, m, objID+1, n, DateTime.Now);
				}
			}

		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			int m = this.pivs.Count;
			var D = new double[m];
			for (int pivID = 0; pivID < m; ++pivID) {
				var objID = this.pivs [pivID];
				var piv = this.DB [objID];
				var d = this.DB.Dist(q, piv);
				D[pivID] = d;
				res.Push (objID, d);
			}
			this.internal_numdists += m;
			int n = this.DB.Count;
			for (int objID = 0; objID < n; ++objID) {
				if (this.ispivot [objID] == 1) {
					continue; 
				}
				var rad = res.CoveringRadius;
				var review = true;
				foreach (var p in this.assocpivots[objID]) {
					var dqp = D [p.ObjID];
					var dup = p.Dist;
					if (Math.Abs (dup - dqp) > rad) {
						review = false;
						break;
					}
				}
				if (review) {
					var d = this.DB.Dist (q, this.DB [objID]);
					res.Push (objID, d);
				}
			}
			return res;
		}
	}
}

