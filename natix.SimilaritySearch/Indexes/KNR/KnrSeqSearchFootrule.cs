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
	public class KnrSeqSearchFootrule: KnrSeqSearchJaccard
	{
		public KnrSeqSearchFootrule () : base()
		{
		}

		public KnrSeqSearchFootrule (KnrSeqSearch knr) : base(knr)
		{
		}
		protected override IResult GetCandidates (IList<int> qseq, int maxcand)
		{
			var len_qseq = qseq.Count;
			var C = new Dictionary<int,int> ();
			// var omega = this.IndexRefs.MainSpace.Count >> 1;
			var omega = qseq.Count << 5;
			// var omega = qseq.Count;
			// var omega = 0;
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				for (int s = 1; s <= count1; ++s) {
					var pos = rs.Select1 (s);
					var docid = pos / this.K;
					int dist;
					if (!C.TryGetValue (docid, out dist)) {
						dist = len_qseq * omega;
					}
					// var d = (int) Math.Abs (Math.Log (i + 1) - Math.Log (1 + (pos % knrbound)));
					var d = Math.Abs (i - (pos % this.K));
					C [docid] = dist + d - omega;
				}
			}
			var res = new ResultTies (Math.Abs (maxcand), false);
			foreach (var pair in C) {
				res.Push (pair.Key, pair.Value);
			}

			return res;
			/*var first = this.GetKnrSeq (0);
			var smaller = this.GetKnrSeq (C_docs [0]);
			Console.WriteLine ("query: {0}, omega: {1}, smaller-docid: {2}", SeqToString (qseq), omega, C_docs [0]);
			Console.WriteLine ("first ==> {0}", DebugSeq (qseq, first, omega, -1));
			Console.WriteLine ("smaller: {0}, computed-dist: {1}", DebugSeq (qseq, smaller, omega, C_sim [0]), C_sim [0]);
			*/
		}

		string SeqToString (IList<ushort> seq)
		{
			StringWriter s = new StringWriter ();
			for (int i = 0; i < seq.Count; ++i) {
				if (i + 1 < seq.Count) {
					s.Write ("{0} ", seq [i]);
				} else {
					s.Write ("{0}", seq [i]);
				}
			}
			return s.ToString();
		}
	}
}