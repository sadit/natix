// 
//  Copyright 2013 Eric Sadit Tellez Avila <donsadit@gmail.com>
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
	public class KnrSeqSearchLCSv2 : KnrSeqSearch
	{
		public KnrSeqSearchLCSv2 () : base()
		{
		}

		public KnrSeqSearchLCSv2 (KnrSeqSearch knr) : base(knr)
		{
		}
		 
		protected override IResult GetCandidates (int[] qseq, int maxcand)
		{
			int knrbound = this.K;
			var len_qseq = qseq.Length;
			// the symshift is a simple hack to reserve the zero symbol as wildcard
			// for all those symbols not in the query
			int symshift = 1;
			var partial_strings = new Dictionary<int,int[]> (Math.Abs(maxcand) * 2);
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				for (int s = 1; s <= count1; ++s) {
					var pos = rs.Select1 (s);
					var docid = pos / knrbound;
					var internalpos = pos - docid * knrbound;
					int[] useq;
					if (partial_strings.TryGetValue (docid, out useq)) {
						useq [internalpos] = qseq[i] + symshift;
					} else {
						useq = new int[knrbound];
						partial_strings [docid] = useq;
						useq [internalpos] = qseq[i] + symshift;
					}
				}
			}
			var res = new ResultTies (Math.Abs (maxcand));
			BitParallelKnr8LLCS bpllcs = new BitParallelKnr8LLCS (qseq, symshift, this.R.DB.Count + symshift);
			foreach (var p in partial_strings) {
				var useq = p.Value;
				var docID = p.Key;
				var newllcs = bpllcs.llcs(useq);
				//Console.WriteLine ("lenlcs: {0}, newllcs: {1}", lenlcs, newllcs);
//					if (lenlcs != newllcs) {
//						var err =  String.Format("ERROR LOS VALORES DE LEN LCS SON DIFERENTES  seqs: {2} ~ {3}, {0} != {1}",
//							lenlcs, newllcs,
//							String.Join<int> (", ", useq),
//							String.Join<int> (", ", qseq));
//						throw new ArgumentOutOfRangeException (err);
//					}
//					res.Push (i, -lenlcs);
				res.Push (docID, -newllcs);
			}
			return res;
		}
	}
}