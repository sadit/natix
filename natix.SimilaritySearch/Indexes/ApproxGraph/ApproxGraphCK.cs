//
//  Copyright 2013     Eric Sadit Tellez Avila <eric.tellez@uabc.edu.mx>
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
	public class ApproxGraphCK : ApproxGraphKNR 
	{

		public ApproxGraphCK () : base()
		{
		}

		public ApproxGraphCK (ApproxGraph ag, int sample_size) : base(ag, sample_size)
		{
		}

		public override Result GetStartingPoints (object q, int num_starting_points)
		{
			var C = 8;
			var knr = Math.Min (this.RepeatSearch * C, this.Vertices.Count);
			var near = new Result (knr);
			int ss = Math.Min (this.SampleSize, this.Vertices.Count);
			var n = this.Vertices.Count;
			for (int i = 0; i < ss; ++i) {
				var objID = this.rand.Next (0, n);
				var d = this.DB.Dist (q, this.DB [objID]);
				near.Push (objID, d);
			}
			if (this.Vertices.Count < this.RepeatSearch) return near;
			var prob = this.RepeatSearch / knr; // 1.0 / C;
			var sp = new Result (Math.Min (this.RepeatSearch, this.Vertices.Count));
			int I = 0;
			foreach (var pair in near) {
				if (I == 0 || this.rand.NextDouble() < prob) {
					sp.Push (pair.docid, pair.dist);
				}
				++I;
			}
			return sp;
		}
	}
}