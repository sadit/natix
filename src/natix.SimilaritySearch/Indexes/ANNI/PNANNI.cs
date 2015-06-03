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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Parallel List/Node based ILC
	/// </summary>
	public class PNANNI: NANNI
	{
		public int PROCESSORS = -1;

		public PNANNI () : base()
		{
		}

		public void OneProcessorSearchKNN (int processID, Result[] results, double[] dcq_cache, double rad, object q, int K)
		{
			Result res = new Result (K);
			int span = dcq_cache.Length / results.Length; // clusters / processors
			int centerID = processID * span;

			// rad should replace covering radius
			for (span = Math.Min(span + centerID, dcq_cache.Length); centerID < span; ++centerID) {
				if (rad > res.CoveringRadius) {
					rad = res.CoveringRadius;
				}
				var node = this.clusters [centerID];
				var center = this.DB [node.objID];
				var dcq = dcq_cache [centerID];

				if (dcq > node.cov + rad) {
					continue;
				}

				int l = node.dists.Count;
				for (int i = 0; i < l; ++i) {
					if (rad > res.CoveringRadius) {
						rad = res.CoveringRadius;
					}
					if (Math.Abs (dcq - node.dists [i]) <= rad) {
						var docID = node.objects [i];

						var d = this.DB.Dist (this.DB [docID], q);
						res.Push (docID, d);
					}
				}
			}
			results[processID] = res;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var dcq_cache = new double[this.clusters.Count];
			var m = this.clusters.Count;
			for (int centerID = 0; centerID < m; ++centerID) {
				var node = this.clusters [centerID];
				var center = this.DB [node.objID];
				var dcq = this.DB.Dist (center, q);
				dcq_cache [centerID] = dcq;
				res.Push (node.objID, dcq);
			}

			this.internal_numdists += m;
			Result[] results = new Result[64];

			Parallel.For(0, results.Length, (i) => this.OneProcessorSearchKNN(i, results, dcq_cache, res.CoveringRadius, q, K));

			foreach (var partial_res in results) {
				foreach (var p in partial_res) {
					res.Push (p.ObjID, p.Dist);
				}
			}

			return res;
		}
	}
}
