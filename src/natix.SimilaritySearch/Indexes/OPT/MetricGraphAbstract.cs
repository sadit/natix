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
	public abstract class MetricGraphAbstract : BasicIndex
	{
		public class Neighbors : List<ItemPair>
		{
			public Neighbors() : base()
			{
			}
		}
		
		public List<Neighbors> Vertices;

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			var count = Input.ReadInt32 ();
			this.Vertices = new List<Neighbors> (count);
			for (int i = 0; i < count; ++i) {
				var c = Input.ReadInt32 ();
				var N = new Neighbors () {
					Capacity = c
				};
				CompositeIO<ItemPair>.LoadVector (Input, c, N);
				this.Vertices.Add (N);
			}
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.Vertices.Count);
			foreach (var N in this.Vertices) {
				Output.Write (N.Count);
				CompositeIO<ItemPair>.SaveVector (Output, N);
			}
		}
		
		public MetricGraphAbstract ()
		{
		}
		
		public void Build(MetricDB db, int arity)
		{
			this.DB = db;
			int n = db.Count;
			this.Vertices = new List<Neighbors> (n);

			for (int objID = 0; objID < n; ++objID) {
				if (objID % 10000 == 0) {
					Console.WriteLine ("==== {0} DB: {1}, objID: {2}/{3}, timestamp: {4}", 
						this, Path.GetFileName(db.Name), objID, db.Count, DateTime.Now);
				}
				this.AddObjID(arity);
			}
		}
		
		protected void AddObjID(int arity)
		{
			var objID = this.Vertices.Count;

			if (this.Vertices.Count <= arity) {
				// the first items are just connected among them
				var N = new Neighbors ();
				this.Vertices.Add (N);
				var obj = this.DB [objID];
				for (int i = 0; i < objID; ++i) {
					var d = this.DB.Dist (this.DB[i], obj);
					this.Vertices [i].Add (new ItemPair(objID, d));
					this.Vertices [objID].Add (new ItemPair(i, d));
				}
			} else {
				var res = new Result (arity);
				this.SearchKNN (this.DB[objID], arity, res);
				var N = new Neighbors();
				this.Vertices.Add(N);
				foreach (var p in res) {
					N.Add(new ItemPair(p.ObjID, p.Dist));
					this.Vertices[p.ObjID].Add(new ItemPair(objID, p.Dist));
				}
			}
		}
	}
}