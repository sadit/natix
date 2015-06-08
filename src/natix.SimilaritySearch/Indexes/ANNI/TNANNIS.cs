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
	public class TNANNIS: TNANNI
	{
		public TNANNIS ()
		{
			this.clusters = new List<Node> ();
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
					var docID = node.objects [i];
					var d = this.DB.Dist (this.DB [docID], q);
					res.Push (docID, d);
				}
			}
			return res;
		}
	} 
}
