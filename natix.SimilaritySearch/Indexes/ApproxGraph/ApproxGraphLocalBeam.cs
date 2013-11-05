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
	public class ApproxGraphLocalBeam : ApproxGraph
	{
		public int BeamSize;

		public ApproxGraphLocalBeam ()
		{
		}

		public ApproxGraphLocalBeam(ApproxGraph ag, int beamsize) : base(ag)
		{
			this.BeamSize = beamsize;
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.BeamSize = Input.ReadInt32 ();
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write (this.BeamSize);
		}

		public void Build (MetricDB db, short arity, short generations, int beam_size)
		{
			this.BeamSize = beam_size;
			this.Build (db, arity, generations);
		}

		protected virtual IResult FirstBeam (object q, IResult final_result, SearchState state)
		{
			int beamsize = Math.Min (this.BeamSize, this.Vertices.Count);
			IResult beam = new Result (beamsize);
			// initializing the first beam
			for (int i = 0; i < beamsize; ++i) {
				var docID = this.rand.Next(this.Vertices.Count);
				if (state.evaluated.Add (docID)) {
					var d = this.DB.Dist (q, this.DB [docID]);
					beam.Push (docID, d);
					final_result.Push(docID, d);
				}
			}
			return beam;
		}

		public override IResult SearchKNN (object q, int K, IResult final_result)
		{
			var state = new SearchState ();
			var beam = this.FirstBeam (q, final_result, state);
			var beamsize = beam.Count;
			//Console.WriteLine ("**** beamsize: {0}, repeatsearch: {1}", beamsize, this.RepeatSearch);
			//Console.WriteLine ("**** count: {0}, vertices-count: {1}", this.DB.Count, this.Vertices.Count);
			// expand successors and select the best BeamSize ones among them
			for (int i = 0; i < this.RepeatSearch; ++i) {
				IResult _beam = new Result (beamsize);
				if (this.Vertices.Count == this.DB.Count)
					Console.WriteLine ("=== Iteration {0}/{1}, res-count: {2}, res-cov: {3}", i, this.RepeatSearch, final_result.Count, final_result.CoveringRadius);
				foreach (var pair in beam) {
					foreach(var neighbor_docID in this.Vertices [pair.docid]) {
						// Console.WriteLine ("=== B i: {0}, docID: {1}, parent: {2}, beamsize: {3}", i, neighbor_docID, pair, beam.Count);
						if (state.evaluated.Add(neighbor_docID)) {
							var d = this.DB.Dist (q, this.DB [neighbor_docID]);
							final_result.Push (neighbor_docID, d);
							_beam.Push (neighbor_docID, d);
						}
					}
				}
				beam = _beam;
			}
			return final_result;
		}
	}
}