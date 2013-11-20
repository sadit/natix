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
	public class KnrSeqSearchLCS : KnrSeqSearch
	{
		public KnrSeqSearchLCS () : base()
		{
		}

		public KnrSeqSearchLCS (KnrSeqSearch knr) : base(knr)
		{
		}
		 
		protected override IResult GetCandidates (int[] qseq, int maxcand)
		{
			int knrbound = this.K;
			var len_qseq = qseq.Length;
			// var C = new Dictionary<int,short> ();
			var C = new byte[this.DB.Count];
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				for (int s = 1; s <= count1; ++s) {
					var pos = rs.Select1 (s);
					var docid = pos / knrbound;
					C [docid] += 1;
				}
			}
			var res = new ResultTies (Math.Abs (maxcand), false);
			for (int i = 0; i < C.Length; ++i) {
				if (C [i] > 0) {
					var useq = this.GetStoredKnr (i);
					var lenlcs = GetLengthLCS (useq, qseq);
//					var oldlenlcs = this.K * 2 - StringSpace<int>.LCS (useq, qseq);
//					if (lenlcs != oldlenlcs/2) {
//						var err =  String.Format("ERROR LOS VALORES DE LEN LCS SON DIFERENTES  seqs: {2} ~ {3}, {0} != {1}",
//						                         lenlcs, oldlenlcs,
//						                         String.Join<int> (", ", useq),
//						                         String.Join<int> (", ", qseq));
//						throw new ArgumentOutOfRangeException (err);
//					}
					res.Push (i, -lenlcs);
				}
			}
			return res;
		}

		public static int GetLengthLCS(int[] useq, int[] qseq)
		{
			var len = qseq.Length;
			var C_ant = new byte[len+1];
			var C_cur = new byte[len+1]; // keeping track of the LLCS
			for (int i = 0; i < len; ++i) {
				var tmp = C_ant;
				C_ant = C_cur;
				C_cur = tmp;

				for (int j = 0; j < len; ++j) {
					var jpp = j + 1;
					if (useq [i] == qseq [j]) {
						C_cur [jpp] = (byte)( C_ant [j] + 1 );
					} else {
						C_cur [jpp] = Math.Max(C_ant[jpp], C_cur[j]);
					}
				}
			}
			return C_cur[C_cur.Length - 1];
		}
	}
}