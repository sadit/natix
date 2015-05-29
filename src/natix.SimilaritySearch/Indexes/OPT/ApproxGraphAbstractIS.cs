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
	public abstract class ApproxGraphAbstractIS: BasicIndex
	{
		public class Vertex : List<int>
		{
			public Vertex(Vertex v) : base(v) {}
			public Vertex(int capacity) : base(capacity) {}
			public Vertex() : base() {}
		}
		
		public List<Vertex> Vertices;
		public int Neighbors;
		protected Random rand = new Random ();
		
		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.Neighbors = Input.ReadInt32 ();
			var count = Input.ReadInt32 ();
			this.Vertices = new List<Vertex> (count);
			for (int i = 0; i < count; ++i) {
				var c = Input.ReadInt32 ();
				var vertex = new Vertex (c);
				PrimitiveIO<int>.LoadVector(Input, c, vertex);
				this.Vertices.Add (vertex);
			}
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.Neighbors);
			Output.Write ((int) this.Vertices.Count);
			foreach (var vertex in this.Vertices) {
				Output.Write ((int) vertex.Count);
				PrimitiveIO<int>.SaveVector (Output, vertex);
			}
		}
	
		public void CloneVertices()
		{
			for (int docID = 0; docID < this.Vertices.Count; ++docID) {
				this.Vertices [docID] = new Vertex (this.Vertices [docID]);
			}
		}

			
		protected void AddObjID(int objID)
		{
			if (this.Vertices.Count <= this.Neighbors) {
				// the first items are just connected among them
				int c = this.Vertices.Count;
				var v = new Vertex ();
				this.Vertices.Add (v);
				for (int i = 0; i < c; ++i) {
					this.Vertices [i].Add (c);
					this.Vertices [c].Add (i);
				}

			} else {
				var res = new Result (this.Neighbors);
				this.SearchKNN (this.DB[objID], this.Neighbors, res);

				var v = new Vertex (this.Neighbors << 1);
				this.Vertices.Add (v);
			
				foreach (var p in res) {
					v.Add( p.ObjID );
					this.Vertices[p.ObjID].Add(objID);
				}

			}
		}

		// From Malkov et al. 2014 Information System. (repeat loop)
		//def KNNSearch(q, m, k) {
		//    TreeSet [object] tempRes, candidates, visitedSet, result
		//
		//    for (i = 0; i < m; i++) {
		//       put random entry point in candidates tempRes <-- null;
		//       repeat {
		//       	     get element c closest from candidates to q;
		//	             remove c from candidates;
		//	             // check stop condition:
		//	             if (c is further than k-th element from result) {
		//	                  break repeat;
		//    	         }
		//	             // update list of candidates:
		//	             for (every element e from friends of c) {
		//	                  if (e is not in visitedSet) {
		//	               	     add e to visitedSet, candidates, tempRes;
		//		              }
		//               }
		//	     }
		//   	 // aggregate the results:
		//	     add objects from tempRes to result;
		//   }
		//   return best k elements from result;
		//}
		protected void InternalSearch(object q, Result candidates, HashSet<int> inserted, IResult res)
		{
			// var mindist = res.CoveringRadius;
			do {
				var c = candidates.PopFirst();
				// if (c.Dist > mindist) {
				if (c.Dist > res.CoveringRadius) {
					break;
				}
				var adjList = this.Vertices[c.ObjID];

				foreach (var objID in adjList) {
					if (inserted.Add(objID)) { // true iff it wasn't evaluated 
						var d = this.DB.Dist (this.DB [objID], q);
						candidates.Push (objID, d);
						res.Push(objID, d);
					}
				}
			} while (candidates.Count > 0);
		}

	}
}