//
//  Copyright 2013  Eric Sadit Tellez Avila <eric.tellez@uabc.edu.mx>
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
	public class ApproxGraphEffectiveNear : ApproxGraphKNR
	{
		protected Result GetStartingPoints (object q, SearchState state)
		{
			int ss = Math.Min (this.SampleSize, this.Vertices.Count);
			var n = this.Vertices.Count;
			Result sorted = new Result (ss);
			for (int i = 0; i < ss; ++i) {
				var objID = this.rand.Next (0, n);
				var d = this.DB.Dist (q, this.DB [objID]);
				sorted.Push (objID, d);
			}
			return sorted;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var state = new SearchState ();
			var sorted = this.GetStartingPoints(q, state);
			int count_startingpoints = 0;
			int count_skipped = 0;
			foreach (var p in sorted) {
				++count_startingpoints;
				if (state.visited.Contains (p.docid)) {
					++count_skipped;
				} else {
					this.GreedySearch (q, res, p.docid, state);
				}
				if (count_startingpoints - count_skipped > this.RepeatSearch) {
					break;
				}
			}
//			Console.WriteLine ("#### count_startingpoints: {0}, count_skipped: {1}, repeat_search: {2}",
//			                   count_startingpoints, count_skipped, this.RepeatSearch);
			return res;
		}
	}
}

