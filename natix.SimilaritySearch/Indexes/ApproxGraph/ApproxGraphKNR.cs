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
	public class ApproxGraphKNR : ApproxGraph
	{
		public int SampleSize = 1024;

		public ApproxGraphKNR ()
		{
		}

		public ApproxGraphKNR (ApproxGraph ag, int sample_size) : base(ag)
		{
			this.SampleSize = sample_size;
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.SampleSize = Input.ReadInt32 ();
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.SampleSize);
		}

		public void Build (MetricDB db, short arity, short repeat_search, int sample_size)
		{
			this.SampleSize = sample_size;
			this.Build (db, arity, repeat_search);
		}

		public override IResult SearchKNN (object q, int K, IResult final_result)
		{
			var knr = Math.Min (this.RepeatSearch, this.Vertices.Count);
			var knrseq = new Result (knr);
			var n = this.Vertices.Count;
			int ss = Math.Min (this.SampleSize, this.Vertices.Count);

			for (int i = 0; i < ss; ++i) {
				var objID = this.rand.Next (0, n);
				var d = this.DB.Dist (q, this.DB [objID]);
				knrseq.Push (objID, d);
			}
			var res_array = new Result[knr];
			int I = 0;
			// on parallel implementation these two hashsets must be set to null
			var visited = new HashSet<int> ();
			var evaluated = new HashSet<int> ();
			// visited = evaluated = null;
			foreach (var p in knrseq) {
				var res = new Result (K);
				this.GreedySearch(q, res, p.docid, visited, evaluated);
				res_array [I] = res;
				++I;
			}
			var inserted = new HashSet<int> ();
			//Console.WriteLine ("==== res_array {0}, I: {1}, knr: {2}", res_array, I, knr);
			foreach (var res in res_array) {
				//Console.WriteLine ("==== res {0}", res);
				if (res == null) {
					break;
				}
				foreach (var p in res) {
					if (inserted.Add(p.docid)) {
						final_result.Push(p.docid, p.dist);
					}
				}
			}
			return final_result;
		}
	}
}