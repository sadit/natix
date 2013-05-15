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
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class MNappHash : BasicIndex
	{
        public NappHash[] A;
		public HyperplaneFP hplanes;
		public static int NumberHyperplanes = 64;

		public override string ToString ()
		{
			return string.Format ("[NappHash num_indexes: {0}, first-index: {1}]", this.A.Length,
			                      this.A.Length == 0 ? "undefined" : this.A[0].ToString());
		}

		public override void Save (BinaryWriter Output)
        {
			base.Save (Output);
            Output.Write (A.Length);
            foreach (var a in A) {
                a.Save(Output);
            }
		}

		public override void Load (BinaryReader Input)
        {
			base.Load (Input);
            var len = Input.ReadInt32 ();
            this.A = new NappHash[len];
            for (int i = 0; i < len; ++i) {
                var a = new NappHash();
                a.Load(Input);
                this.A[i] = a;
            }
		}

		public MNappHash () : base()
		{
		}

//		public void SetQueryExpansion()
//		{
//			for (int i = 0; i < this.A.Length; ++i) {
//				this.A[i] = new KnrLSHQueryExpansion(this.A[i]);
//			}
//		}
//
//		public void UnsetQueryExpansion()
//		{
//			for (int i = 0; i < this.A.Length; ++i) {
//				this.A[i] = new KnrLSH(this.A[i]);
//			}
//		}

		public void Build (MNappHash I, int num_instances)
		{
			this.A = new NappHash[num_instances];
			this.DB = I.DB;
			this.hplanes = I.hplanes;
			for (int i = 0; i < num_instances; ++i) {
				this.A[i] = I.A[i];
			}
		}

		public void Build (MetricDB db, int K, int num_refs, int num_instances, HyperplaneFP hpfp)
        {
            this.A = new NappHash[num_instances];
            this.DB = db;
			var seed = RandomSets.GetRandomInt ();
			int I = 0;
			for (int i = 0; i < num_instances; ++i) {
				this.A[i] = new NappHash();
				this.A[i].Build(db, K, num_refs, RandomSets.GetRandom(seed + i));
				++I;
				Console.WriteLine ("=== Created {0}/{1} K:{2}, {3}", I, num_instances, K, this);
			}
			this.hplanes = hpfp;
		}

		public void SetHyperplanes(int numhyperplanes)
		{
			this.hplanes = new HyperplaneFP ();
			this.hplanes.Build (this.DB, numhyperplanes);
		}

		public void SetExpansionK (int k)
		{
			foreach (var a in this.A) {
				a.K = k;
			}
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
            var L = new HashSet<int>();
			foreach (var a in this.A) {
				foreach (var b in a.GetNear(q, a.K)) {
					L.UnionWith( b );
				}
			}
	
			Console.WriteLine ("L.Count: {0}",L.Count);
			var r = this.FilterCandidates(q,L);
			
			foreach (var pair in r) {
				double d = this.DB.Dist (q, this.DB [pair.docid]);
				res.Push (pair.docid, d);
			}
			return res;
		}

		public virtual IResult FilterCandidates (object q, HashSet<int> L)
		{
			var query = this.hplanes.GetFP(q);
			IResult r = new Result(this.hplanes.MaxCandidates, false);
			var fp = this.hplanes.Fingerprints;
			foreach(var docID in L){
				var d = fp.Dist (query, fp[docID]);
				r.Push(docID,d);
			}
			return r;
		}

		public override IResult SearchRange (object q, double radius)
		{
            return this.SearchKNN(q, this.DB.Count, new ResultRange(radius));
		}
	}
}