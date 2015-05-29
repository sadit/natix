//
//  Copyright 2013-2014     Eric Sadit Tellez Avila <eric.tellez@infotec.com.mx>
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
	public class LocalSearchGallopingBeam : LocalSearch
	{

		public LocalSearchGallopingBeam ()
		{
		}

		public void Build (MetricDB db, int neighbors)
		{
			this.InternalBuild (db, neighbors);
		}

		protected virtual Result FirstBeam (object q, HashSet<int> evaluated, IResult res)
		{
			int beamsize = Math.Min (128, this.Vertices.Count);
			var beam = new Result (beamsize);
			// initializing the first beam
			for (int i = 0; i < beamsize; ++i) {
				// selecting beamsize items for the first beam
				var docID = this.Vertices.GetRandom().Key;
				if (evaluated.Add (docID)) {
					var d = this.DB.Dist (q, this.DB [docID]);
					beam.Push (docID, d);
					res.Push(docID, d);
				}
			}
			return beam;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			// var state = new SearchState (final_result);
			HashSet<int> evaluated = new HashSet<int> ();
			var beam = this.FirstBeam (q, evaluated, res);
			var maxbeamsize = beam.Count;
			var beamsize = maxbeamsize;

			double prev;

			do {
				prev = res.CoveringRadius;
				var _beamsize = Math.Min (beamsize, beam.Count);
//				var _beam = new Result(_beamsize);
				var _beam = new Result(_beamsize);
				for (int j = 0; j < _beamsize; ++j) {
					var item = beam.PopFirst ();
					foreach (var neighbor_docID in this.Vertices [item.ObjID]) {
						if (evaluated.Add (neighbor_docID)) {
							var d = this.DB.Dist (q, this.DB [neighbor_docID]);
							res.Push (neighbor_docID, d);
							_beam.Push(neighbor_docID, d);
//							beam.Push (neighbor_docID, d);   // this can include distant items at any step
						}
					}
				}
				beam = _beam;
//				foreach (var p in _beam) {
//					beam.Push(p.ObjID, p.Dist);
//				}
				if (beamsize > 32) {
					beamsize = beamsize / 2;
				}
				if (prev == res.CoveringRadius) {
					beamsize = beamsize / 2;
				}
			} while (beamsize > 1);
			// } while (prev > curr);
			// } while (beamsize <= maxbeamsize);
			return res;
		}
	}
}