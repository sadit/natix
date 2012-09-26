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
	public class CompactPivotsRANS : CompactPivots
	{

		public CompactPivotsRANS () : base()
		{
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
					var lower = this.Discretize(Math.Max (0.0, dqp - res.CoveringRadius), stddev);
					var upper = (int)Math.Min(seq.Sigma - 1, this.Discretize(dqp + res.CoveringRadius, stddev));
					if (sym < lower || upper < sym ) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					res.Push(i, this.DB.Dist(q, this.DB[i]));
				}

			}
			return res;
		}
	}
}

