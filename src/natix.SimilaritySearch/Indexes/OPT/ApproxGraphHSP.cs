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
// Modified by Guillermo Ruiz to use the HSP proximal items
//
using System;
using System.IO;
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using natix.SortingSearching;
using HSP_app;

namespace natix.SimilaritySearch
{
	public class ApproxGraphHSP : ApproxGraph
	{
		public int SampleSize = 1024;

		public ApproxGraphHSP ()
		{
		}

		public ApproxGraphHSP (ApproxGraph ag, int sample_size) : base(ag)
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

			// Get the HSP!!!
			List<int> subdb=new List<int>(this.DB.Count);
			for (int i = 0; i < ss; ++i) {
				var objID = this.rand.Next (0, n);
				subdb.Add(objID);
				//knrseq.Push (objID, d);
			}
			// Get the hsp-neigbors of q
			IList<int> hsp_neighbors= HSP.ComputeEdges(q,this.DB,subdb);
			foreach (int id in hsp_neighbors)
			{
				var d = this.DB.Dist (q, this.DB [id]);
				Console.WriteLine("id:{0} d:{1}",id,d);
				knrseq.Push(id,d);
			}
			Console.WriteLine( knrseq.Count);
			// End getting the HSP

			var res_array = new Result[knrseq.Count];
			int I = 0;
			// on a parallel implementation these two hashsets must be set to null
			var state = new SearchState ();
			// visited = evaluated = null;
			foreach (var p in knrseq) {
				var res = new Result (K);
				//Console.WriteLine ("** starting GreedySearch");
				this.GreedySearch(q, res, p.ObjID, state);
				res_array [I] = res;
				++I;
			}
			var inserted = new HashSet<int> ();
			foreach (var res in res_array) {
				if (res == null) {
					break;
				}
				foreach (var p in res) {
					if (inserted.Add(p.ObjID)) {
						final_result.Push(p.ObjID, p.Dist);
					}
				}
			}
			return final_result;
		}
	}
}