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
	public class LocalSearchBestFirst : LocalSearch
	{
		public int Window = 8;

		public LocalSearchBestFirst ()
		{
		}

		public LocalSearchBestFirst (LocalSearch other, int window) : base()
		{
			this.DB = other.DB;
			this.Neighbors = other.Neighbors;
			this.Vertices = other.Vertices;
			this.Window = window;
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.Window = Input.ReadInt32 ();
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.Window);
		}

		public void Build (MetricDB db, int neighbors, int window = 8)
		{
			this.Window = window;
			this.InternalBuild (db, neighbors);
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			HashSet<int> inserted = new HashSet<int> ();
			var maxsize = Math.Min (1000, (int)Math.Sqrt(this.Vertices.Count));
			// Rules about queue:
			// - the distance to q is the priority (smaller values come first) 
			// - has no duplicated items
			// - all items in queue were evaluated by distance
			var queue = new SkipList2<ItemPair> (0.5, (a, b) => a.Dist.CompareTo (b.Dist));
			// initial queue, it follows the rules
			for (int i = 0; i < maxsize; ++i) {
				var docID = this.Vertices.GetRandom ().Key;
				if (inserted.Add (docID)) {
					var d = this.DB.Dist (q, this.DB [docID]);
					if (res.Push (docID, d)) {
						queue.Add (new ItemPair (docID, d), null);
					}
				}
			}
			var coveringMontecarlo = res.CoveringRadius;

			double prev;
			double curr;
			do {
				prev = res.CoveringRadius;
				for (int i = 0; i < this.Window && queue.Count > 0; ++i) {
					var first = queue.RemoveFirst(); // it cannot be duplicated
					foreach (var neighbor in this.Vertices [first.ObjID]) {
						if (inserted.Add(neighbor)) { // ensure rules
							var d = this.DB.Dist (q, this.DB [neighbor]);
							if (d < coveringMontecarlo) {
								res.Push (neighbor, d);
								queue.Add(new ItemPair(neighbor, d), null);
							}
						}
					}
				}
				curr = res.CoveringRadius;
			} while (prev > curr && queue.Count > 0);
			return res;
		}
	}
}