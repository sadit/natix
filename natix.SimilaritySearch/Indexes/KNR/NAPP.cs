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
	public class NAPP : BasicIndex
	{
		public int K;
		public int MINOCC;
		// public int MAXCAND;
		public Index R;
		public List<int[]> INVINDEX;
		//public ITThresholdAlgorithm TThreshold = new LargeStepTThreshold(new DoublingSearch<int>());
		public NTTArray8A TThreshold = new NTTArray8A ();
		public int MAXCAND = int.MaxValue;
		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write(this.K);
			Output.Write(this.MINOCC);
			//Output.Write(this.MAXCAND);
			IndexGenericIO.Save(Output, this.R);
			for (int i = 0, sigma = this.R.DB.Count; i < sigma; ++i) {
				var list = this.INVINDEX[i];
				Output.Write(list.Length);
				PrimitiveIO<int>.WriteVector(Output, list);
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
			this.INVINDEX = new List<int[]> (sigma);
			for (int i = 0; i < sigma; ++i) {
				var len = Input.ReadInt32 ();
				var list = PrimitiveIO<int>.ReadFromFile(Input, len, null) as int[];
				this.INVINDEX.Add(list);
			}
		}

		public NAPP () : base()
		{
		}

		public NAPP (KnrSeqSearch knr) : base()
		{
			this.Build (knr, 1);
		}

		public void Build (KnrSeqSearch knr, int min_occ)
		{
			this.DB = knr.DB;
			this.K = knr.K;
			this.MINOCC = min_occ;
			// this.MAXCAND = knr.MAXCAND;
			this.R = knr.R;
			int sigma = this.R.DB.Count;
			this.INVINDEX = new List<int[]> (sigma);
			var list = new List<int>();
			for (int i = 0; i < sigma; ++i) {
				list.Clear();
				var unravel = knr.SEQ.Unravel(i);
				var count = unravel.Count1;
				for (int s = 1; s <= count; ++s) {
					list.Add (unravel.Select1(s) / this.K);
				}
				this.INVINDEX.Add( list.ToArray() );
			}
		}

		public int[] GetKnr (object q, int number_near_references)
		{
            this.internal_numdists-=this.R.Cost.Internal;
			var res = this.R.SearchKNN(q, number_near_references);
            this.internal_numdists+=this.R.Cost.Internal;
			var qseq = new int[res.Count];
			int i = 0;
			foreach (var s in res) {
				qseq[i] = s.docid;
				++i;
			}
			return qseq;
		}

		protected virtual int[][] GetPosting (int[] qseq)
		{
			var len_qseq = qseq.Length;
			// var C = new Dictionary<int,short> ();
			var posting = new int[len_qseq][];
			int avg_len = 0;
			for (int i = 0; i < len_qseq; ++i) {
				posting[i] = this.INVINDEX[qseq[i]];
				avg_len += posting[i].Length;
			}
			Console.WriteLine ("=== avg-posting-list-length: {0}", avg_len / ((float)len_qseq));
			return posting;
		}

		public IResult SearchKNN (object q, int knn, IResult res, int knrsearch)
		{
			var qseq = this.GetKnr (q, this.K);
			var posting = this.GetPosting (qseq);
			//int maxcand = this.MAXCAND;
			//this.TThreshold.SearchTThreshold (posting, this.MINOCC, out docs, out card);	
			var cand = new Result (this.MAXCAND);
			this.TThreshold.SearchTT (posting, this.MINOCC, cand);	
//			Console.WriteLine ("XXXX : requested K: {0}, output K: {1}, posting-count: {2}, maxcand: {3}, minocc: {4}, cand-count: {5}",
//			                   this.K, qseq.Length, posting.Length, this.MAXCAND, this.MINOCC, cand.Count);
			foreach (var p in cand) {
				double d = this.DB.Dist (q, this.DB [p.docid]);
				res.Push (p.docid, d);
			}
			return res;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			return this.SearchKNN (q, knn, res, this.K);
		}

		public override IResult SearchRange (object q, double radius)
		{
			var res = new ResultRange (radius, this.DB.Count);
			this.SearchKNN(q, this.DB.Count, res);
			return res;
		}

	}
}