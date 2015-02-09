//
//  Copyright 2013     Eric Sadit Tellez Avila
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
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using natix;
using natix.SortingSearching;
using System.Threading;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class LocalSearchRestarts : LocalSearch
	{

		public LocalSearchRestarts () : base()
		{
		}

		public LocalSearchRestarts (LocalSearchRestarts a) : base()
		{
			this.DB = a.DB;
			this.Neighbors = a.Neighbors;
			this.Vertices = a.Vertices;
		}

		public void Build(MetricDB db, int neighbors)
		{
			this.InternalBuild (db, neighbors);
		}

		public override IResult SearchKNN (object q, int K, IResult final_result)
		{
			var window = 2;
			if (this.Vertices.Count > 16) {
				window = 4;
			}
			var prev = double.MaxValue;
			var curr = 0.0;
			var inserted = new HashSet<int> ();
			var expanded = new HashSet<int> ();

			while (prev > curr) {
				prev = final_result.CoveringRadius;
				for (int i = 0; i < window; ++i) {
					var res = new Result (K);
					var next = this.rand.Next (this.Vertices.Count);
					if (expanded.Add(next)) {
						this.GreedySearch (q, next, expanded, res);
						foreach (var p in res) {
							if (inserted.Add(p.ObjID)) {
								final_result.Push(p.ObjID, p.Dist);
							}
						}
					}
				}
				curr = final_result.CoveringRadius;
			}
			return final_result;
		}
		
		protected void GreedySearch(object q, int startID, HashSet<int> visited, IResult res)
		{
			visited.Add (startID);
			res.Push (startID, this.DB.Dist (this.DB [startID], q));
		
			var minDist = double.MaxValue;
			int minItem;
			var evaluated = new HashSet<int> ();

			// visited is a global set containing nodes already expanded and explored
			// evaluated is a local variable containing items already evaluated
			// evaluated must be local to preserve diversity

			do {
				minItem = -1;
				var adjList = this.Vertices[startID];

				foreach (var objID in adjList) {
					if (evaluated.Add(objID)) { // true iff it wasn't evaluated 
						var d = this.DB.Dist (this.DB [objID], q);
						res.Push (objID, d);
						if (minDist > d) {
							minDist = d;
							minItem = objID;
						}
					}
				}
				startID = minItem;
			} while (minItem >= 0 && visited.Add(startID));
		}
	}
}