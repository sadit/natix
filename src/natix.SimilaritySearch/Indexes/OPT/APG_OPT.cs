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
	public class APG_OPT : BasicIndex
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
		
		public APG_OPT ()
		{
		}

		public APG_OPT(APG_OPT a) : base()
		{
			this.DB = a.DB;
			this.Neighbors = a.Neighbors;
			this.Vertices = a.Vertices;
		}

		public void CloneVertices()
		{
			for (int docID = 0; docID < this.Vertices.Count; ++docID) {
				this.Vertices [docID] = new Vertex (this.Vertices [docID]);
			}
		}

		public void Build(MetricDB db, int neighbors)
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
				var v = new Vertex(this.Neighbors << 1);
				this.Vertices.Add(v);
				foreach (var p in res) {
					v.Add( p.ObjID );
					this.Vertices[p.ObjID].Add(objID);
				}
			}
		}


		public override IResult SearchKNN (object q, int K, IResult final_result)
		{
			var window = 2;
			if (this.Vertices.Count > 16) {
				window = 8;
			}
			var prev = double.MaxValue;
			var curr = 0.0;
			var inserted = new HashSet<int> ();
			var visited = new HashSet<int> ();

			while (prev > curr) {
				prev = final_result.CoveringRadius;
				for (int i = 0; i < window; ++i) {
					var res = new Result (K);
					var next = this.rand.Next (this.Vertices.Count);
					if (visited.Add(next)) {
						this.TabuSearch (q, next, visited, res);
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

		protected void TabuSearch(object q, int startID, HashSet<int> expanded, IResult res)
		{
			expanded.Add (startID);
			res.Push (startID, this.DB.Dist (this.DB [startID], q));

			int minItem;
			var evaluated = new HashSet<int> ();

			// visited is a global set containing nodes already expanded and explored
			// evaluated is a local variable containing items already evaluated
			// evaluated must be local to preserve diversity

			do {
				var adjList = this.Vertices[startID];

				foreach (var objID in adjList) {
					if (evaluated.Add(objID)) { // true iff it wasn't evaluated 
						var d = this.DB.Dist (this.DB [objID], q);
						res.Push (objID, d);
					}
				}
				minItem = -1;
				foreach (var p in res) {
					if (expanded.Contains(p.ObjID)) {
						continue;
					}
					minItem = p.ObjID;
					break;
				}
				startID = minItem;
			} while (minItem >= 0 && expanded.Add(startID));
		}
	}
}