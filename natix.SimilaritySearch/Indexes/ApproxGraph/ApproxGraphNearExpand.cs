//
//  Copyright 2013  Eric Sadit <eric.tellez@uabc.edu.mx>
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
	public class ApproxGraphNearExpand : ApproxGraphKNR
	{
		public ApproxGraphNearExpand ()
		{
		}

		public ApproxGraphNearExpand (ApproxGraph ag, int sample_size) : base(ag, sample_size)
		{
		}
		
		protected virtual Result GetNearPoint (object q)
		{
			var near = new Result (1);
			int ss = Math.Min (this.SampleSize, this.Vertices.Count);
			var n = this.Vertices.Count;
			for (int i = 0; i < ss; ++i) {
				var objID = this.rand.Next (0, n);
				var d = this.DB.Dist (q, this.DB [objID]);
				near.Push (objID, d);
			}
			return near;
		}


		protected void ExpandNode(object q, IResult res, int startID, SearchState state, int level)
		{
			var nodeS = this.Vertices [startID];
			foreach (var nodeID in nodeS) {
				if (state.visited.Add(nodeID)) {
					if (level > 0) {
						this.ExpandNode (q, res, nodeID, state, level - 1);
					}
				}
				if (state.evaluated.Add (nodeID)) {
					var d = this.DB.Dist (q, this.DB [nodeID]);
					res.Push(nodeID, d);
				}
			} 
		}

		public int MAX_EXPANSION_LEVEL = 1;
		public override IResult SearchKNN (object q, int K, IResult res)
		{
			//Console.WriteLine ("******* STARTING SEARCH repeat={0} *******", this.MAX_EXPANSION_LEVEL);
			var near = this.GetNearPoint(q);
			var state = new SearchState ();
			//int I = 0;
			this.GreedySearch (q, res, near.First.docid, state);
			// even a small number can expand a very large set of items because
			// it depends on the branching factor
			// the first level is already expanded but they can be trunked by
			// the Result object, then we must expand it too and use state to
			// avoid the duplication of work
			this.ExpandNode (q, res, near.First.docid, state, MAX_EXPANSION_LEVEL);
			return res;
		}
	}
}