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
	public class NAPPHash : BasicIndex
	{
		public int K;
		public Index R;
		public List<List<int>> INVINDEX;
		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write(this.K);
			//Output.Write(this.MAXCAND);
			IndexGenericIO.Save(Output, this.R);
			for (int i = 0, sigma = this.R.DB.Count; i < sigma; ++i) {
				var list = this.INVINDEX[i];
				Output.Write(list.Count);
				PrimitiveIO<int>.WriteVector(Output, list);
			}
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.K = Input.ReadInt32 ();
			this.R = IndexGenericIO.Load(Input);
			int sigma = this.R.DB.Count;
			this.INVINDEX = new List<int[]> (sigma);
			for (int i = 0; i < sigma; ++i) {
				var len = Input.ReadInt32 ();
				var list = new List<int> (len);
				 PrimitiveIO<int>.ReadFromFile(Input, len, list);
				this.INVINDEX.Add(list);
			}
		}

		public NAPPHash () : base()
		{
		}

		public void Build (MetricDB db, int k, int num_refs)
		{
			var sample = new SampleSpace("", db, num_refs);
			var I = new EPTable();
			I.Build(sample, 4, (_db, _seed) => new EPListRandomPivots(_db, _seed, 300));
			this.Build(db, k, I);
		}

		public void Build (MetricDB db, int k, Index ref_index)
		{
			this.DB = db;
			this.K = k;
			this.R = ref_index;
			int sigma = this.R.DB.Count;
			this.INVINDEX = new List<List<int>> (sigma);

			var list = new List<int>();
			for (int objID = 0; objID < this.DB.Count; ++objID) {
				var u = this.GetKnr(this.DB[objID], this.K);
				for (int i = 0; i < this.K; ++i) {
					this.INVINDEX[u[i]].Add (objID);
				}
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

		public virtual List<int> GetNearList(object q)
		{
			return this.GetKnr (q, 1)[0];			
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var list = this.GetNearList(q);
			foreach (var objID in list) {
				double d = this.DB.Dist (q, this.DB [objID]);
				res.Push (objID, d);
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
			var res = new ResultRange (radius, this.DB.Count);
			this.SearchKNN(q, this.DB.Count, res);
			return res;
		}
	}
}