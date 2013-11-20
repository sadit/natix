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
	public class ApproxGraphLocalBeamKNRExpandNear: ApproxGraphLocalBeamKNR
	{
		protected void ExpandNeighborhood(object q, int startID, List<int> neighborhood, SearchState state, int level)
		{
			var nodelist = this.Vertices [startID];
			foreach (var nodeID in nodelist) {
				if (state.evaluated.Add(nodeID)) {
					neighborhood.Add (nodeID);
					if (level > 0) {
						this.ExpandNeighborhood (q, nodeID, neighborhood, state, level - 1);
					}
				}
			} 
		}

		public override IResult SearchKNN (object q, int K, IResult final_result)
		{
			var state = new SearchState ();
			var beam = this.FirstBeam (q, final_result, state);
			var beamsize = beam.Count;
			double prevcov = 0;
			int count_ties = 0;
			var neighborhood = new List<int> (32);
			for (int i = 0; i < this.RepeatSearch; ++i) {
				prevcov = final_result.CoveringRadius;
				var _beam = new Result (beamsize);
				foreach (var pair in beam) {
					neighborhood.Clear ();
					// neighborhood.Add (pair.docid);
					foreach (var neighbor_docID in this.Vertices [pair.docid]) {
						this.ExpandNeighborhood (q, neighbor_docID, neighborhood, state, 8);
					}
					foreach (var neighbor_docID in neighborhood) {
						var d = this.DB.Dist (q, this.DB [neighbor_docID]);
						final_result.Push (neighbor_docID, d);
						_beam.Push (neighbor_docID, d);
					}
				}
				if (final_result.CoveringRadius == prevcov) {
					if (count_ties == 1) {
						// we stop after two ties
						break;
					}
					++count_ties;
				} else {
					count_ties = 0;
				}
				beam = _beam;
			}
			return final_result;
		}
	}
}