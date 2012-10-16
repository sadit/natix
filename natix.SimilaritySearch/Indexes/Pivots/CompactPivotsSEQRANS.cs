//
//  Copyright 2012  Eric Sadit Tellez Avila
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
	public class CompactPivotsSEQRANS : CompactPivots
	{

		public CompactPivotsSEQRANS () : base()
		{
		}



		public override void Build (MetricDB db, int num_pivs, int search_pivs = 0, SequenceBuilder seq_builder = null)
		{
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqPlain(32);
			}
			base.Build (db, num_pivs, search_pivs, seq_builder);
		}


		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var m = this.PIVS.Count;
			var n = this.DB.Count;
			var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
			var P = new float[ m ];
			var A = new HashSet<int>();
			for (int piv_id = 0; piv_id < m; ++piv_id) {
				var dqp = this.DB.Dist (q, this.PIVS [piv_id]);
				var i = _PIVS[piv_id];
				A.Add(i);
				P[piv_id] = (float)dqp;
				res.Push (i, dqp);
			}
			for (int i = 0; i < n; ++i) {
				if (A.Contains(i)) {
					continue;
				}
				bool check_object = true;
				for (int piv_id = 0; piv_id < m; ++piv_id) {
					var dqp = P[piv_id];
					var seq = this.SEQ[piv_id];
					var sym = seq.Access(i);
					var stddev = this.STDDEV[piv_id];
					var lower = this.Discretize(Math.Abs (dqp - res.CoveringRadius), stddev);
					var upper = this.Discretize(dqp + res.CoveringRadius, stddev);
					if (sym < lower || upper < sym ) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					// Console.WriteLine ("CHECKING i: {0}, m: {1}, n: {2}", i, m, n);
					res.Push(i, this.DB.Dist(q, this.DB[i]));
				}
			}
			return res;
		}

		
		public override IResult SearchRange (object q, double radius)
		{
			var m = this.PIVS.Count;
			var n = this.DB.Count;
			HashSet<int> A = null;
			HashSet<int> B = null;
			for (int piv_id = 0; piv_id < m; ++piv_id) {
				var dqp = this.DB.Dist (q, this.PIVS [piv_id]);
				var seq = this.SEQ[piv_id];
				if (A == null) {
					A = new HashSet<int>();
					for (int i = 0; i < n; ++i) {
						var sym = seq.Access(i);
						var stddev = this.STDDEV[piv_id];
						var lower = this.Discretize(Math.Abs (dqp - radius), stddev);
						var upper = this.Discretize(dqp + radius, stddev);
						if (sym < lower || upper < sym ) {
							A.Add (i);
						}
					}
				} else {
					B = new HashSet<int>();
					foreach (var i in A) {
						var sym = seq.Access(i);
						var stddev = this.STDDEV[piv_id];
						var lower = this.Discretize(Math.Abs (dqp - radius), stddev);
						var upper = this.Discretize(dqp + radius, stddev);
						if (sym < lower || upper < sym ) {
							B.Add(i);
						}
					}
					A = B;
				}
			}
			var res = new Result(this.DB.Count, false);
			foreach (var docid in A) {
				var d = this.DB.Dist(this.DB[docid], q);
				if (d <= radius) {
					res.Push(docid, d);
				}
			}
			return res;
		}

	}
}

