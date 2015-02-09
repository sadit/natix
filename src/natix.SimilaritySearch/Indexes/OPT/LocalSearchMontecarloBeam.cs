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
	public class LocalSearchMontecarloBeam: LocalSearchBeam
	{
		protected override Result FirstBeam (object q, HashSet<int> evaluated, IResult res)
		{
			int samplesize = 0;
			// the real value can bee really complex, Monte Carlo methods
			// just use the larger sustainable value, however we are interested
			// on keep a fixed upper bound
			if (this.Vertices.Count > 4) {
				if (this.Vertices.Count < 10000) {
					samplesize = (int) Math.Sqrt (this.Vertices.Count) * 2;
				} else {
					samplesize = 1000;
				}
			}
			// int samplesize = Math.Min (2, this.Vertices.Count);
			int beamsize = Math.Min (this.BeamSize, this.Vertices.Count);
			var beam = new Result (beamsize);
			// initializing the first beam
			for (int i = 0; i < samplesize; ++i) {
				var docID = this.Vertices.GetRandom().Key;
				if (evaluated.Add (docID)) {
					var d = this.DB.Dist (q, this.DB [docID]);
					beam.Push (docID, d);
					res.Push(docID, d);
				}
			}
			return beam;
		}
	}
}