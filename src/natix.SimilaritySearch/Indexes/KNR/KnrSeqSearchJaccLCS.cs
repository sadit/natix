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
	public class KnrSeqSearchJaccLCS : KnrSeqSearch
	{
		public KnrSeqSearchJaccLCS () : base()
		{
		}

		public KnrSeqSearchJaccLCS (KnrSeqSearch knr) : base(knr)
		{
		}
		 
		protected override IResult GetCandidates (int[] qseq, int maxcand)
		{
			int knrbound = this.K;
			var len_qseq = qseq.Length;
			var partial_strings = new Dictionary<int,byte[]> (Math.Abs(maxcand) * 2);
			for (int i = 0; i < len_qseq; ++i) {
				byte mask = (byte)(1 << i);
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				for (int s = 1; s <= count1; ++s) {
					var pos = rs.Select1 (s);
					var docid = pos / knrbound;
					var internalpos = pos - docid * knrbound;
					byte[] useq;
					if (partial_strings.TryGetValue (docid, out useq)) {
						// useq [internalpos] = qseq[i] + symshift;
						useq [internalpos] = mask;
					} else {
						useq = new byte[knrbound];
						partial_strings [docid] = useq;
						// useq [internalpos] = qseq[i] + symshift;
						useq [internalpos] = mask;
					}
				}
			}

			var res = new ResultTies (Math.Abs (maxcand));
			foreach (var p in partial_strings) {
				var useq = p.Value;
				var docID = p.Key;
				var sim = BitParallelKnr8LLCS.llcs_diggested_pattern_with_intersection(useq);
				res.Push (docID, -sim);
			}
			return res;
		}

	}
}