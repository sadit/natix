//
//  Copyright 2015  Eric Sadit Tellez Avila <eric.tellez@infotec.com.mx>
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
	public class APG_OptTabuSatNeighborhoodMontecarloStart : BasicIndex
	{
		public class Vertex : List<int>
		{
			public Vertex(Vertex v) : base(v) {}
			public Vertex(int capacity) : base(capacity) {}
			public Vertex() : base() {}
		}
		
		public List<Vertex> Vertices;
		protected Random rand = new Random ();
		
		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
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
			Output.Write ((int) this.Vertices.Count);
			foreach (var vertex in this.Vertices) {
				Output.Write ((int) vertex.Count);
				PrimitiveIO<int>.SaveVector (Output, vertex);
			}
		}
		
		public APG_OptTabuSatNeighborhoodMontecarloStart ()
		{
		}

		public void CloneVertices()
		{
			for (int docID = 0; docID < this.Vertices.Count; ++docID) {
				this.Vertices [docID] = new Vertex (this.Vertices [docID]);
			}
		}

		public void Build(MetricDB db)
		{
			this.DB = db;
			int n = db.Count;
			this.Vertices = new List<Vertex> (n);

			for (int objID = 0; objID < n; ++objID) {
				if (objID % 5000 == 0) {
					Console.WriteLine ("XXX==== {0} DB: {1}, Neighbors: SAT-like, objID: {2}/{3}, timestamp: {4}",
						this, Path.GetFileName(db.Name), objID, db.Count, DateTime.Now);
				}
				this.AddObjID(objID);
				AVG_NEIGHBORS = AVG_NEIGHBORS * 0.9 + this.Vertices [objID].Count * 0.1;
			}
		}

		int MAX_NEIGHBORS = 64;
		double AVG_NEIGHBORS = 32;

		protected void AddObjID(int objID)
		{
			// const int MAXN = 100;
			int MAXN = 8;

			if (this.Vertices.Count <= MAX_NEIGHBORS) {
				// the first items are just connected among them
				int c = this.Vertices.Count;
				var v = new Vertex ();
				this.Vertices.Add (v);
				for (int i = 0; i < c; ++i) {
					this.Vertices [i].Add (c);
					this.Vertices [c].Add (i);
				}
			} else {
				//Console.WriteLine ("N: {0}, PREV: {1}", AVG_NEIGHBORS, this.Vertices[objID-1].Count);
				MAXN = (int)Math.Min (32, AVG_NEIGHBORS + 8);
				var res = new Result (MAXN);
				var obj = this.DB [objID];
				this.SearchKNN (obj, MAXN, res);

				var v = new Vertex ();
				this.Vertices.Add (v);
			
				var _res = new List<ItemPair> (res);
				for (int currPos = 0; currPos < _res.Count; ++currPos) {
					var q = _res [currPos];
					var skip = false;

					for (int i = 0; i < currPos; ++i) {
						var u = _res [i];
						if (2 * Math.Abs(q.Dist - u.Dist) > q.Dist) { // not essential
							skip = true;
							break;
						}
					}

					if (skip) {
						continue;
					}

					for (int i = 0; i < currPos; ++i) {
						var u = _res [i];
						var d = this.DB.Dist (this.DB [q.ObjID], this.DB [u.ObjID]);
						if (d < q.Dist) { 
							skip = true; // not essential neighbor
							break;
						}
					}

					if (skip) {
						continue;
					}

					v.Add (q.ObjID);
					this.Vertices [q.ObjID].Add (objID);
				}
//				foreach (var p in res) {
//					// SAT-like Neighborhood
//					var d = double.MaxValue;
//					for (int i = 0; i < v.Count; ++i) {
//						d = this.DB.Dist (this.DB [p.ObjID], this.DB [v [i]]);
//						if (d < p.Dist) { // not essential
//							break;
//						}
//					}
//					if (d > p.Dist) {
//						v.Add (p.ObjID);
//						this.Vertices [p.ObjID].Add (objID);
//					}
//				}
				//if (objID % 100 == 0)	Console.WriteLine ("@@@@ --- neighborhood-size: {0}, objID: {1}", v.Count, objID);

			}
		}


		void MontecarloStart(object q, IResult res, int size, HashSet<int> inserted)
		{

			for (int i = 0; i < size; ++i) {
				var objID = this.rand.Next (this.Vertices.Count);
				if (inserted.Add (objID)) {
					var d = this.DB.Dist (q, this.DB [objID]);
					res.Push (objID, d);
				}
			}
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
			if (this.Vertices.Count > 2000) {
				this.MontecarloStart (q, final_result, (int)Math.Sqrt(this.Vertices.Count), inserted);
			}

			while (prev > curr) {
				prev = final_result.CoveringRadius;
				for (int i = 0; i < window; ++i) {
					var res = new Result (K);
					var next = this.rand.Next (this.Vertices.Count);
					if (expanded.Add(next)) {
						this.TabuSearch (q, next, expanded, res);
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