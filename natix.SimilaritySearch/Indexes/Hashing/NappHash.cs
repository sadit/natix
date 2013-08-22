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
	public class NappHash : BasicIndex
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
				PrimitiveIO<int>.SaveVector(Output, list);
			}
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.K = Input.ReadInt32 ();
			this.R = IndexGenericIO.Load(Input);
			int sigma = this.R.DB.Count;
			this.INVINDEX = new List<List<int>> (sigma);
			for (int i = 0; i < sigma; ++i) {
				var len = Input.ReadInt32 ();
				var list = new List<int> (len);
				 PrimitiveIO<int>.LoadVector(Input, len, list);
				this.INVINDEX.Add(list);
			}
		}

		public NappHash () : base()
		{
		}

		public void Build (MetricDB db, int k, int num_refs, Random rand)
		{
			var sample = new SampleSpace("", db, num_refs, rand);
			var I = new SAT_Distal();
			I.Build(sample, rand);
			this.Build(db, k, I);
		}

		public void Build (MetricDB db, int k, Index ref_index)
		{
			this.DB = db;
			this.K = k;
			this.R = ref_index;
			int sigma = this.R.DB.Count;
			this.INVINDEX = new List<List<int>> (sigma);
			for (int i = 0; i < sigma; ++i) {
				this.INVINDEX.Add(new List<int>());
			}
			var A = new int[this.DB.Count][];
			int count = 0;
			var compute_one = new Action<int>(delegate(int objID) {
				var u = this.GetKnr(this.DB[objID], this.K);
				A[objID] = u;
				++count;
				if (count % 1000 == 0) {
					Console.WriteLine ("==== {0}/{1} db: {2}, k: {3}", count, this.DB.Count, this.DB.Name, k);
				}
			});
			ParallelOptions ops = new ParallelOptions();
			ops.MaxDegreeOfParallelism = -1;
			Parallel.ForEach(new ListGen<int>((int i) => i, this.DB.Count), ops, compute_one);

			for (int objID = 0; objID < this.DB.Count; ++objID) {
				var u = A[objID];
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

		public virtual List<int>[] GetNear(object q, int ksearch)
		{
			var near = new List<int> [ksearch];
			var knrseq = this.GetKnr(q, ksearch);
			for (int i = 0; i < ksearch; ++i) {
				near[i] = this.INVINDEX[knrseq[i]];
			}
			return near;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var list = this.GetNear (q, this.K);
			if (list.Length == 1) {
				foreach (var objID in list[0]) {
					double d = this.DB.Dist (q, this.DB [objID]);
					res.Push (objID, d);
				}
			} else {
				var near = new HashSet<int> ();
				foreach (var L in list) {
					near.UnionWith(L);
				}
				foreach (var objID in near) {
					double d = this.DB.Dist (q, this.DB [objID]);
					res.Push (objID, d);
				}
			}
			return res;
		}
	}
}