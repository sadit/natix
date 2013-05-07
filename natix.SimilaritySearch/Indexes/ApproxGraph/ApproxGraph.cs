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
		}
		
		public List<Vertex> Vertices;
		public short Arity;
		public short RepeatSearch;
		public Random rand;

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.Arity = Input.ReadInt16 ();
			this.RepeatSearch = Input.ReadInt16 ();
			this.Vertices = new List<Vertex> ();
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.Arity);
			Output.Write (this.RepeatSearch);
			// 
		}
		
		public ApproxGraph ()
		{
			this.rand = new Random ();
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
					Console.WriteLine ("==== {0} DB: {1}, Arity: {2}, RepeatSearch: {3}", this, db.Name, arity, repeat_search);
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

		public override IResult SearchRange (object q, double radius)
		{
			var res = new ResultRange (radius, this.DB.Count);
			this.SearchKNN (q, this.DB.Count, res);
			return res;
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

		protected void GreedySearch(object q, IResult res, HashSet<int> visited, HashSet<int> evaluated, int startID)
		{
			var minDist = double.MaxValue;
			var minItem = 0;
			do {
				// Console.WriteLine ("XXXXXX SEARCH  startID: {0}, count: {1}, res-count: {2}", startID, this.vertices.Count, res.Count);
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