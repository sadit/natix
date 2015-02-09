// 
//  Copyright 2012  Eric Sadit Tellez Avila 
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
	public class KnrSeqSearchCosine: KnrSeqSearchJaccard
	{
		public KnrSeqSearchCosine () : base()
		{
		}

		public KnrSeqSearchCosine (KnrSeqSearch knr) : base(knr)
		{
		}

		/// <summary>
		/// Computes the candidates using a dissimilarity function preserving the order of the
		/// cosine similarity.
		/// </summary>
		protected override IResult GetCandidates (int[] qseq, int maxcand)
		{
			var len_qseq = qseq.Length;
			var C = new Dictionary<int,int> ();
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				for (int s = 1; s <= count1; ++s) {
					var pos = rs.Select1 (s);
					var docID = pos / this.K;
					var internal_pos = pos - docID * this.K;
					int sim;
					if (!C.TryGetValue (docID, out sim)) {
						sim = 0;
					}
					sim += (1 + len_qseq - i) * (1 + this.K - internal_pos);
					C [docID] = sim;
				}
			}
			var res = new ResultTies (Math.Abs (maxcand));
			foreach (var pair in C) {
				res.Push (pair.Key, -pair.Value);
			}
			return res;
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