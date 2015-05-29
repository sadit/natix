//
//  Copyright 2013-2014  Eric Sadit Tellez Avila <eric.tellez@infotec.com.mx>
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
	public class ApproxGraphOptRestartsIS: ApproxGraphAbstractIS
	{
		public ApproxGraphOptRestartsIS ()
		{
		}

		public virtual void Build(MetricDB db, int neighbors)
		{
			this.DB = db;
			this.Neighbors = neighbors;
			int n = db.Count;
			this.Vertices = new List<Vertex> (n);

			for (int objID = 0; objID < n; ++objID) {
				if (objID % 10000 == 0) {
					Console.WriteLine ("XXX==== {0} DB: {1}, Neighbors: {2}, objID: {3}/{4}, timestamp: {5}", 
						this, Path.GetFileName(db.Name), neighbors, objID, db.Count, DateTime.Now);
				}
				this.AddObjID(objID);
			}
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var window = 2;
			if (this.Vertices.Count > 16) {
				window = 8;
			}
//			if (this.Vertices.Count > 10000) {
//				Console.WriteLine ("STARTING SEARCH");
//			}
			var prev = double.MaxValue;
			var curr = 0.0;
			var inserted = new HashSet<int> ();
			var candidates = new Result (this.Vertices.Count);

			while (prev > curr) {
				prev = res.CoveringRadius;
				for (int i = 0; i < window; ++i) {
					var next = this.rand.Next (this.Vertices.Count);
					if (inserted.Add (next)) {
						var d = this.DB.Dist (q, this.DB [next]);
						candidates.Push (next, d);
						res.Push (next, d);
						this.InternalSearch (q, candidates, inserted, res);
					}
				}
				curr = res.CoveringRadius;
			}
			return res;
		}
	}
}