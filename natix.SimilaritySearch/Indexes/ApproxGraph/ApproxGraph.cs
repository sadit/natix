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
	public class ApproxGraph : BasicIndex
	{
		public class Vertex : List<int>
		{
			public Vertex(int capacity) : base(capacity) {}
			public Vertex() : base() {}
		}
		
		public List<Vertex> Vertices;
		public short Arity;
		public short RepeatSearch;
		public Random rand = new Random ();
		
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
				PrimitiveIO<int>.ReadFromFile(Input, c, vertex);
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
				PrimitiveIO<int>.WriteVector (Output, vertex);
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
		
		public void Build(MetricDB db, short arity, short repeat_search)
		{
			this.DB = db;
			this.Arity = arity;
			this.RepeatSearch = repeat_search;
			int n = db.Count;
			this.Vertices = new List<Vertex> (n);
			for (int objID = 0; objID < n; ++objID) {
				if (objID % 1000 == 0) {
					Console.WriteLine ("XXX==== {0} DB: {1}, Arity: {2}, RepeatSearch: {3}, objID: {4}", this, db.Name, arity, repeat_search, objID);
				}
				this.AddObjID(objID);
			}
		}
		
		protected void AddObjID(int objID)
		{
			if (this.Vertices.Count == 0) {
				this.Vertices.Add (new Vertex ());
			} else {
				var res = new Result (this.Arity);
				this.SearchKNN (this.DB [objID], this.Arity, res);
				var v = new Vertex();
				this.Vertices.Add(v);
				foreach (var p in res) {
					v.Add( p.docid );
					this.Vertices[p.docid].Add(objID);
				}
			}
		}
		
		public override IResult SearchKNN (object q, int K, IResult final_result)
		{
			var res_array = new Result[ this.RepeatSearch ];
			// seeds[i] = Randomly select a subset of size this.RepeatSearch in [0,n]
//			var search = new Action<int> (delegate (int i) {
//				var res = new Result (K);
//				this.GreedySearch(q, res, seeds[i]);
//				res_array [i] = res;
//			});
//			System.Threading.Tasks.Parallel.For (0, this.RepeatSearch, search);
			for (int i = 0; i < res_array.Length; ++i) {
				var res = new Result (K);
				this.GreedySearch(q, res, this.rand.Next (this.Vertices.Count));
				res_array [i] = res;
			}
			var inserted = new HashSet<int> ();
			foreach (var res in res_array) {
				foreach (var p in res) {
					if (inserted.Add(p.docid)) {
						final_result.Push(p.docid, p.dist);
					}
				}
			}
			return final_result;
		}
		
		void GreedySearch(object q, IResult res, int startID)
		{
			HashSet<int> visited = new HashSet<int> ();
			HashSet<int> evaluated = new HashSet<int> ();
			{
				visited.Add (startID);
				evaluated.Add (startID);
				var d = this.DB.Dist (this.DB[ startID ], q);
				res.Push (startID, d);
			}
			var minDist = double.MaxValue;
			var minItem = 0;
			do {
				// Console.WriteLine ("StartID: {0}, Count-Vertices: {1}", startID, this.Vertices.Count);
				foreach (var objID in this.Vertices[startID]) {
					var d = this.DB.Dist (this.DB [objID], q);
					if (evaluated.Add(objID)) { 
						res.Push (objID, d);
					}
					if (minDist > d) {
						minDist = d;
						minItem = objID;
					}
				}
				startID = minItem;
			} while (visited.Add (startID));
		}
	}
}