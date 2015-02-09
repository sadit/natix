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
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class ApproxGraphSeq : ApproxGraph
	{
		public ApproxGraphSeq (): base()
		{
		}

		public ApproxGraphSeq (ApproxGraph a) : base(a)
		{
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var visited = new HashSet<int> ();
			var evaluated = new HashSet<int> ();
			for (int i = 0; i < this.RepeatSearch; ++i) {
				var objID = this.rand.Next (this.Vertices.Count);
				while (visited.Add (objID)) {
					if (evaluated.Add (objID)) {
						var d = this.DB.Dist (this.DB [objID], q);
						res.Push (objID, d);
					}
					this.GreedySearch(q, res, visited, evaluated, objID);
				}
			}
			return res;
		}
		
		void GreedySearch(object q, IResult res, HashSet<int> visited, HashSet<int> evaluated, int startID)
		{
			var minDist = double.MaxValue;
			var minItem = 0;
			do {
				// Console.WriteLine ("XXXXXX======= SEARCH  startID: {0}, count: {1}, res-count: {2}", startID, this.Vertices.Count, res.Count);
				foreach (var objID in this.Vertices[startID]) {
					if (evaluated.Add (objID)) {
						var d = this.DB.Dist (this.DB [objID], q);
						res.Push (objID, d);
						if (minDist > d) {
							minDist = d;
							minItem = objID;
						}
					}
				}
				startID = minItem;
			} while (visited.Add (startID));
		}
	}
}