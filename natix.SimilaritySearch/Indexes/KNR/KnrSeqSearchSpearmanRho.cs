// 
//  Copyright 2012  sadit
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
using System.Collections.Generic;
using natix.CompactDS;
using natix.Sets;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class KnrSeqSearchSpearmanRho : KnrSeqSearchJaccard
	{
		public KnrSeqSearchSpearmanRho () : base()
		{
		}

		public KnrSeqSearchSpearmanRho (KnrSeqSearch knr) : base(knr)
		{
		}

		protected override IResult GetCandidates (int[] qseq, int maxcand)
		{
			var len_qseq = qseq.Length;
			var C = new Dictionary<int,int> ();
			var omega = this.R.DB.Count >> 1;
			// omega *= omega;
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				// Console.WriteLine ("seq: {0}/{1}, class: {2}, count1: {3}", i, len_qseq, this.seqindex, count1);
				for (int s = 1; s <= count1; ++s) {
					var pos = rs.Select1 (s);
					var docid = pos / this.K;
					int dist;
					if (!C.TryGetValue (docid, out dist)) {
						dist = (short)(len_qseq * omega);
					}
					var diff = Math.Abs (i - (pos % this.K));
					C [docid] = dist + diff * diff - omega;
					//C [docid] = dist + diff - omega;
				}
			}
			//Chronos chronos = new Chronos ();
			//chronos.Begin ();
			var res = new ResultTies (Math.Abs (maxcand), false);
			foreach (var pair in C) {
				res.Push (pair.Key, (short)Math.Sqrt (pair.Value));
			}
			//chronos.End ();
			//chronos.PrintStats ();
			return res;
		}
	}
}