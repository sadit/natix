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
	public class CNAPP : BasicIndex
	{
		public int K;
		public int MINOCC;
		// public int MAXCAND;
		public Index R;
		public IRankSelect[] INVINDEX;
		public ITThresholdAlgorithm TThreshold = new LargeStepTThreshold(new DoublingSearch<int>());

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write(this.K);
			Output.Write(this.MINOCC);
			//Output.Write(this.MAXCAND);
			IndexGenericIO.Save(Output, this.R);
			for (int i = 0, sigma = this.R.DB.Count; i < sigma; ++i) {
				RankSelectGenericIO.Save(Output, this.INVINDEX[i]);
			}
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.K = Input.ReadInt32 ();
			this.MINOCC = Input.ReadInt32 ();
			//this.MAXCAND = Input.ReadInt32 ();
			this.R = IndexGenericIO.Load(Input);
			int sigma = this.R.DB.Count;
			this.INVINDEX = new IRankSelect[sigma];
			for (int i = 0; i < sigma; ++i) {
				this.INVINDEX[i] = RankSelectGenericIO.Load(Input);
			}
		}

		public CNAPP () : base()
		{
		}

		public void Build (KnrSeqSearch knr, int min_occ, BitmapFromList bitmap_builder = null)
		{
			this.DB = knr.DB;
			this.K = knr.K;
			this.MINOCC = min_occ;
			// this.MAXCAND = knr.MAXCAND;
			this.R = knr.R;
			int sigma = this.R.DB.Count;
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetDiffSetRL2(63, new EliasDelta());
			}
			this.INVINDEX = new IRankSelect[sigma];
			var list = new List<int>();
			for (int i = 0; i < sigma; ++i) {
				list.Clear();
				var unravel = knr.SEQ.Unravel(i);
				var count = unravel.Count1;
				for (int s = 1; s <= count; ++s) {
					list.Add (unravel.Select1(s) / this.K);
				}
				this.INVINDEX[i] = bitmap_builder(list);
			}
		}

		public int[] GetKnr (object q)
		{
            this.internal_numdists-=this.R.Cost.Internal;
			var res = this.R.SearchKNN(q, this.K);
            this.internal_numdists+=this.R.Cost.Internal;
			var qseq = new int[this.K];
			int i = 0;
			foreach (var s in res) {
				qseq[i] = s.docid;
				++i;
			}
			return qseq;
		}
		 
		protected virtual IList<IList<int>> GetPosting (IList<int> qseq)
		{
			var len_qseq = qseq.Count;
			// var C = new Dictionary<int,short> ();
			var posting = new List<IList<int>> (len_qseq);
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.INVINDEX [qseq [i]];
				posting.Add (new SortedListRSCache (rs));
			}
			return posting;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var qseq = this.GetKnr (q);
			var posting = this.GetPosting (qseq);
			//int maxcand = this.MAXCAND;
			IList<int> docs;
			IList<short> card;
			this.TThreshold.SearchTThreshold (posting, this.MINOCC, out docs, out card);	
			for (int i = 0; i < docs.Count; ++i) {
				int docid = docs [i];
				double d = this.DB.Dist (q, this.DB [docid]);
				res.Push (docid, d);
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
			var qseq = this.GetKnr (q);
			var posting = this.GetPosting (qseq);
			IList<int> docs;
			IList<short> card;
			var res = new Result(this.DB.Count);
			this.TThreshold.SearchTThreshold (posting, this.MINOCC, out docs, out card);
			for (int i = 0; i < docs.Count; ++i) {
				var docid = docs[i];
				double d = this.DB.Dist (q, this.DB [docid]);
				if (d <= radius) {
					res.Push (docid, d);
				}
			}
			return res;
		}
	}
}