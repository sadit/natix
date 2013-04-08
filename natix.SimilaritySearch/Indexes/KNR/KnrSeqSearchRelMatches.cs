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
	public class KnrSeqSearchRelMatches : KnrSeqSearch
	{
		
		
		public KnrSeqSearchRelMatches (): base()
		{
		}
		 
		public KnrSeqSearchRelMatches (KnrSeqSearch knr) : base(knr)
		{
		}
		/// <summary>
		/// Gets the candidates. 
		/// </summary>
		protected override IResult GetCandidates (int[] qseq, int maxcand)
		{
			// TODO store tsearch as an object property
			ITThresholdAlgorithm tsearch = new NTTArray8 (-1, false);
			// ITThresholdAlgorithm tsearch = new MergeTThreshold ();
			// int maxcand = Math.Abs (this.Maxcand);
			var len_qseq = qseq.Length;
			var lists = new IList<int>[ len_qseq];
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				lists [i] = new SortedListRSCache (rs, -i + len_qseq);
			}
			// lists [len_qseq] = new ListGen<int> ((int i) => i * knrbound, (int)Math.Ceiling(this.seqindex.Count * 1.0 / knrbound));
			IList<int> __C_docs;
			IList<short> __C_sim;
			tsearch.SearchTThreshold (lists, 1, out __C_docs, out __C_sim);
			var res = new ResultTies (Math.Abs (maxcand), false);
			for (int i = 0; i < __C_docs.Count; ++i) {
				var docid = __C_docs [i] - len_qseq;
				docid = docid / this.K;
				var sim = -__C_sim [i];
				res.Push (docid, sim);
			}
			return res;
		}
		
	}
}