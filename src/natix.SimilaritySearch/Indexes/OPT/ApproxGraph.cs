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
using natix.SortingSearching;
using System.Threading;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class ApproxGraph : BasicIndex
	{
		protected class SearchState
		{
			public HashSet<int> visited = new HashSet<int> ();
			public HashSet<int> evaluated = new HashSet<int> ();
		}

		public class Vertex : List<int>
		{
			public Vertex(Vertex v) : base(v) {}
			public Vertex(int capacity) : base(capacity) {}
			public Vertex() : base() {}
		}
		
		public List<Vertex> Vertices;
		public short Arity;
		public short RepeatSearch;
		protected Random rand = new Random ();
		
		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.Arity = Input.ReadInt16 ();
			this.RepeatSearch = Input.ReadInt16 ();
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
			Output.Write (this.Arity);
			Output.Write (this.RepeatSearch);
			Output.Write ((int) this.Vertices.Count);
			foreach (var vertex in this.Vertices) {
				Output.Write ((int) vertex.Count);
				PrimitiveIO<int>.SaveVector (Output, vertex);
			}
		}
		
		public ApproxGraph ()
		{
			this.rand = new Random ();
		}

		public ApproxGraph(ApproxGraph a) : base()
		{
			this.DB = a.DB;
			this.Arity = a.Arity;
			this.RepeatSearch = a.RepeatSearch;
			this.Vertices = a.Vertices;
		}

		public void CloneVertices()
		{
			for (int docID = 0; docID < this.Vertices.Count; ++docID) {
				this.Vertices [docID] = new Vertex (this.Vertices [docID]);
			}
		}

		public void DropLargeLinks()
		{
			for (int docID = 0; docID < this.Vertices.Count; ++docID) {
				var obj = this.DB [docID];
				var queue = new Result (this.Arity);
				foreach (var linkToID in this.Vertices[docID]) {
					var dist = this.DB.Dist (obj, this.DB [linkToID]);
					queue.Push (linkToID, dist); 
				}
				var vertex = new Vertex (this.Arity);
				foreach (var p in queue) {
					vertex.Add (p.ObjID);
				}
				this.Vertices [docID] = vertex;
			}
		}


		public void TrimAt(int n)
		{
			for (int i = 0; i < n; ++i) {
				var vertex = this.Vertices [i];
				var list = new List<int> ();
				foreach (var v in vertex) {
					if (v < n) {
						list.Add (v);
					}
				}
				vertex.Clear ();
				vertex.AddRange (list);
			}

			while (this.Vertices.Count > n) {
				this.Vertices.RemoveAt (this.Vertices.Count - 1);
			}
		}
		
		public void Build(MetricDB db, short arity, short repeat_search)
		{
			this.DB = db;
			this.Arity = arity;
			this.RepeatSearch = repeat_search;
			int n = db.Count;
			this.Vertices = new List<Vertex> (n);
			for (int objID = 0; objID < n; ++objID) {
				if (objID % 10000 == 0) {
					Console.WriteLine ("XXX==== {0} DB: {1}, Arity: {2}, RepeatSearch: {3}, objID: {4}/{5}, timestamp: {6}", 
						this, Path.GetFileName(db.Name), arity, repeat_search, objID, db.Count, DateTime.Now);
				}
				this.AddObjID(objID);
			}
		}
		
		protected void AddObjID(int objID)
		{
			if (this.Vertices.Count <= this.Arity) {
				// the first items are just connected among them
				int c = this.Vertices.Count;
				var v = new Vertex ();
				this.Vertices.Add (v);
				for (int i = 0; i < c; ++i) {
					this.Vertices [i].Add (c);
					this.Vertices [c].Add (i);
				}
			} else {
				var res = new Result (this.Arity);
//				if (this.RepeatSearch > 1 && this.Vertices.Count > 1000000) {
//					this.ParallelSearchKNN (this.DB[objID], this.Arity, res);
//				} else {
					this.SearchKNN (this.DB[objID], this.Arity, res);
//				}
				var v = new Vertex(this.Arity*2);
				this.Vertices.Add(v);
				foreach (var p in res) {
					v.Add( p.ObjID );
					this.Vertices[p.ObjID].Add(objID);
				}
			}
		}

		public override IResult SearchKNN (object q, int K, IResult final_result)
		{
			var res_array = new Result[this.RepeatSearch];
			for (int i = 0; i < res_array.Length; ++i) {
				var res = new Result (K);
				this.GreedySearch(q, res, this.rand.Next (this.Vertices.Count), null);
				res_array [i] = res;
			}
			var inserted = new HashSet<int> ();
			foreach (var res in res_array) {
				foreach (var p in res) {
					if (inserted.Add(p.ObjID)) {
						final_result.Push(p.ObjID, p.Dist);
					}
				}
			}
			return final_result;
		}
		
		protected void GreedySearch(object q, IResult res, int startID, SearchState state)
		{
			if (state == null) {
				state = new SearchState ();
			}
			{
				state.visited.Add (startID);
				state.evaluated.Add (startID);
				var d = this.DB.Dist (this.DB [startID], q);
				res.Push (startID, d);
			}
			var minDist = double.MaxValue;
			var minItem = 0;
			do {
				foreach (var objID in this.Vertices[startID]) {
					var d = this.DB.Dist (this.DB [objID], q);
					if (state.evaluated.Add(objID)) { 
						res.Push (objID, d);
					}
					if (minDist > d) {
						minDist = d;
						minItem = objID;
					}
				}
				startID = minItem;
			} while (state.visited.Add (startID));
		}
	}
}