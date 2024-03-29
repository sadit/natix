//
//   Copyright 2015,2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class XMANNIAbstract : BasicIndex
	{
		public TANNI[] rows;
		public XNANNI leader;

		public XMANNIAbstract ()
		{
		}


		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var num = Input.ReadInt32 ();

			this.leader = new XNANNI();
			this.leader.Load (Input);

			this.rows = new TANNI[num];
			for (int i = 0; i < num; ++i) {
				this.rows[i] = new TANNI();
				this.rows[i].Load (Input);
			}

		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write ((int)this.rows.Length);
			this.leader.Save (Output);
			foreach (var row in this.rows) {
				row.Save(Output);
			}
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			this.InternalSearchKNN (q, K, res);
			return res;
		}

		public int InternalSearchKNN (object q, int K, IResult res)
		{
			var m = this.leader.clusters.Count;
			var dcq_cache = new double[m];
			var order = new int[m];
			var cost = m;
			double min_dist = double.MaxValue;

			for (int centerID = 0; centerID < m; ++centerID) {
				var node = this.leader.clusters [centerID];
				var center = this.DB [node.objID];
				var dcq = this.DB.Dist (center, q);
				dcq_cache [centerID] = dcq;
				res.Push (node.objID, dcq);
				if (dcq < min_dist) {
					min_dist = dcq;
				}
				order [centerID] = centerID;
			}
			this.internal_numdists += m;
			//var min_dist = res.First.Dist;
			Sorting.Sort(order, (a, b) => dcq_cache[a].CompareTo(dcq_cache[b]));

			var rows_dcq_cache = new double[this.rows.Length][];
			for (int rowID = 0; rowID < this.rows.Length; ++rowID) {
				var row = this.rows [rowID];
				var _m = row.ACT.Count;
				var _row_dcq_cache = new double[_m];
				this.internal_numdists += _m;
				rows_dcq_cache [rowID] = _row_dcq_cache;
				for (int i = 0; i < _m; ++i) {
					var _c = row.ACT [i];
					var dcq = this.DB.Dist (q, this.DB[_c]);
					res.Push(_c, dcq);
					_row_dcq_cache [i] = dcq;
				}
				cost += _m;
			}

			// for (int centerID = 0; centerID < m; ++centerID) {
			foreach (var centerID in order) {
				var node = this.leader.clusters [centerID];
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
					if (Math.Abs (dcq - node.dists [i]) <= rad) {
						var docID = node.objects [i];
						cost += this.VerifyInRows(docID, q, res, rad, rows_dcq_cache);
					}
				}
			}
			return cost;
		}

		int VerifyInRows(int objID, object q, IResult res, double rad, double[][] cache)
		{

			for (int rowID = 0; rowID < this.rows.Length; ++rowID) {
				var row = this.rows [rowID];
				var c = row.CT [objID];
				if (c == -1) {
					return 0;
				}
				double dcq = cache [rowID] [c];

				if (Math.Abs (dcq - row.DT [objID]) > rad) {
					return 0;
				}
			}

			var d = this.DB.Dist(q, this.DB[objID]);
			res.Push (objID, d);
			return 1;
		}
	}
}

