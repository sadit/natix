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
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class KnrSeqSearch : BasicIndex
	{
		public int K;
		public int MAXCAND;
		public Index R;
		public Sequence SEQ;

		public KnrSeqSearch GetSortedByPrefix (SequenceBuilder seq_builder = null, ListIBuilder list_builder = null)
		{
			int n = this.DB.Count;
			var seqs = new int[n][];
			var perm = new int[n];
			for (int i = 0; i < n; ++i) {
				seqs [i] = this.GetStoredKnr (i);
				perm [i] = i;
			}
			// please speed up this method using another sorting method
			// Sorting.Sort<int> (perm, (x,y) => StringSpace<int>.LexicographicCompare (seqs [x], seqs [y]));
			Sorting.Sort<int[],int> (seqs, perm, (x,y) => SeqSpace<int>.LexicographicCompare (x, y));
			var S = new ListGen<int> ((int i) => seqs [i / this.K] [i % this.K], n * this.K);
			if (list_builder == null) {
				list_builder = ListIBuilders.GetListIFS();
			}
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqXLB_DiffSet64(24, 63);
			}
			var knr = new KnrSeqSearch();
			knr.DB = new SampleSpace("", this.DB, list_builder(perm, n-1));
			knr.K = this.K;
			knr.MAXCAND = this.MAXCAND;
			knr.R = this.R;
			knr.SEQ = seq_builder(S, this.R.DB.Count);
			return knr;
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write(this.K);
			Output.Write(this.MAXCAND);
			IndexGenericIO.Save(Output, this.R);
			GenericIO<Sequence>.Save(Output, this.SEQ);
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.K = Input.ReadInt32 ();
			this.MAXCAND = Input.ReadInt32 ();
			this.R = IndexGenericIO.Load(Input);
			this.SEQ = GenericIO<Sequence>.Load(Input);
			Console.WriteLine ("=== Loading KnrSeqSearch {0} DB.Count: {1}, R: {2}, SEQ length: {3}, K: {4}", this, this.DB.Count, this.R, this.SEQ.Count, this.K);
		}

		public KnrSeqSearch () : base()
		{
		}

		public KnrSeqSearch (KnrSeqSearch other) : base()
		{
			this.Build (other);
		}

		public void Build (KnrSeqSearch other)
		{
			this.DB = other.DB;
			this.K = other.K;
			this.MAXCAND = other.MAXCAND;
			this.R = other.R;
			this.SEQ = other.SEQ;
		}

		public void BuildApprox (MetricDB db, Random rand, int num_refs, int K=7, int maxcand=1024, SequenceBuilder seq_builder = null)
		{
			var sample = new SampleSpace ("", db, num_refs);
			var inner = new KnrSeqSearch ();
			inner.Build (sample, rand, 1024, K, int.MaxValue);
			this.Build (db, new KnrSeqSearchFootrule(inner), K, maxcand, seq_builder);
		}

		public void Build (MetricDB db, Random rand, int num_refs, int K=7, int maxcand=1024, SequenceBuilder seq_builder=null)
		{
			var sample = new SampleSpace ("", db, num_refs, rand);
			var sat = new SAT_Distal ();
			sat.Build (sample, RandomSets.GetRandom());
			this.Build (db, sat, K, maxcand, seq_builder);
		}

		public void Build (MetricDB db, Index refs, int K=7, int maxcand=1024, SequenceBuilder seq_builder=null)
		{
			var knrfp = new KnrFP ();
			knrfp.Build (db, refs, K);
			this.Build (db, knrfp, maxcand, seq_builder);
		}

		public void Build (MetricDB db, KnrFP knrfp, int maxcand=1024, SequenceBuilder seq_builder=null)
		{
			this.DB = db;
			this.R = knrfp.IdxRefs;
			this.K = knrfp.K;
			this.MAXCAND = maxcand;
			//var M = knrfp.Fingerprints.seqs;
			var M = knrfp.Fingerprints;
//			var L = new int[this.K * this.DB.Count];
//			int pos = 0;
//			for (int objID = 0; objID< this.DB.Count; ++objID) {
//				var u = M [objID];
//				for (int i = 0; i < this.K; ++i, ++pos) {
//					L [pos] = u [i];
//				}
//			}
			var L = new ListGen<int> ((int i) => M [i / K] [i % K], this.DB.Count * this.K);
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqXLB_SArray64 (16);
			}
			Console.WriteLine ("xxxxx Build L: {0}, R: {1}, db-count: {2}, db: {3}, K: {4}", L.Count, this.R.DB.Count, db.Count, db, K);
			this.SEQ = seq_builder (L, this.R.DB.Count);
		}

		public int[] GetKnr (object q)
		{
			var idist = this.R.DB.NumberDistances;
			// Console.WriteLine ("=== GetKnr KnrSeqSearch DB: {0} DB.Count: {1}, R: {2}, SEQ length: {3}, K: {4}, q: {5}", this.DB, this.DB.Count, this.R.DB, this.SEQ.Count, this.K, q);
			var qseq = KnrFP.GetFP (q, this.R, this.K);
            this.internal_numdists += this.R.DB.NumberDistances - idist;
			return qseq;
		}

		public int[] GetStoredKnr (int docid)
		{
			var L = new int[this.K];
			for (int i = 0, start_pos = this.K * docid; i < this.K; ++i) {
				L [i] = this.SEQ.Access (start_pos + i);
			}
			return L;
		}

		protected virtual IResult GetCandidates (int[] qseq, int maxcand)
		{
			var len_qseq = qseq.Length;
			var n = this.DB.Count;
			var A = new Dictionary<int,int>(1024);
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				for (int j = 1; j <= count1; ++j) {
					var pos = rs.Select1 (j);
					var docid = pos / this.K;
					var internal_pos = pos - docid * this.K;
					if (internal_pos == i) {
						int value;
						if (i == 0) {
							A [docid] = 1;
						} else if (A.TryGetValue(docid, out value) && value == i) {
							A [docid] = value + 1;
						}
					}
				}
			}
			var res = new ResultTies (Math.Abs (maxcand));
			foreach (var p in A) {
				res.Push (p.Key, -p.Value);
			}
			return res;
		}

		protected virtual IResult GetCandidatesPlain (int[] qseq, int maxcand)
		{
//			var n = this.DB.Count;
//			if (n < 500000) {
//				//return this.GetCandidatesSmallDB (qseq, maxcand);
//			}
			var len_qseq = qseq.Length;
			var ialg = new BaezaYatesIntersection<int> (new DoublingSearch<int> ());
			IList<int> current = new SortedListRSCache (this.SEQ.Unravel (qseq [0]));
			int i = 1;
			while (i < len_qseq && current.Count > maxcand) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var I = new SortedListRSCache (rs, -i);
				var L = new List<IList<int>> () {current, I};
				var intersection = ialg.Intersection (L);
				++i;
				if (intersection.Count < maxcand) {
					break;
				}
				current = intersection;
			}
			var res = new ResultTies (int.MaxValue);
			foreach (var c in current) {
				if (c % this.K == 0) {
					res.Push (c / this.K, 0);
				}
			}
			return res;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var qseq = this.GetKnr (q);
			return this.SearchKNN(qseq, q, res, MAXCAND); 
		}

		public IResult SearchKNN (int[] qseq, object q, IResult res, int maxcand)
		{
			var C = this.GetCandidates (qseq, Math.Abs (maxcand));
			if (maxcand < 0) {
				return C;
			} else {
				foreach (var p in C) {
					var docid = p.ObjID;
					double d = this.DB.Dist (q, this.DB [docid]);
					res.Push (docid, d);
					--maxcand;
					if (maxcand <= 0) {
						break;
					}
				}
			}
			return res;
		}
	}
}