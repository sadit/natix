//
//  Copyright 2015     Eric Sadit Tellez Avila <eric.tellez@infotec.com.mx>
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
//

using System;
using System.IO;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class MetricGraphGreedy : MetricGraphAbstract
	{
		public MetricGraphGreedy ()
		{
		}

		Random rand = new Random();

		public override IResult SearchKNN(object q, int K, IResult res)
		{
			var n = this.Vertices.Count;
			const int window = 2;

			var prev = double.MaxValue;
			var curr = 0.0;
			var inserted = new HashSet<int> ();
			var candidates = new Result (Math.Max(K, 32));
			// var candidates = new Result (K+K);

			while (prev > curr) {
				prev = res.CoveringRadius;
				for (int i = 0; i < window; ++i) {
					var next = this.rand.Next (this.Vertices.Count);
					if (inserted.Add (next)) {
						var _res = new Result (res.K);
						var d = this.DB.Dist (q, this.DB [next]);
						candidates.Push (next, d);
						res.Push (next, d);
						this.InternalSearch (q, candidates, inserted, _res);
						foreach (var p in _res) {
							res.Push (p.ObjID, p.Dist);
						}
					}
				}
				curr = res.CoveringRadius;
			}
			return res;
		}

		public void InternalSearch(object q, Result candidates, HashSet<int> inserted, IResult res)
		{
			do {
				var start = candidates.PopFirst ();
				if (start.Dist > res.CoveringRadius) {
					break;
				}
				var adjList = this.Vertices[start.ObjID];

				foreach (var item in adjList) {
					if (inserted.Add(item.ObjID)) { // true iff it wasn't evaluated 
						var d = this.DB.Dist (this.DB [item.ObjID], q);
						candidates.Push (item.ObjID, d);
						res.Push(item.ObjID, d);
					}
				}
			} while (candidates.Count > 0);
		}
	}
}