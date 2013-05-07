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
	public class MKnrLSH : BasicIndex
	{
        public KnrLSH[] A;

		public override string ToString ()
		{
			return string.Format ("[MKnrLSH num_indexes: {0}, first-index: {1}]", this.A.Length,
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
            this.A = new KnrLSH[len];
            for (int i = 0; i < len; ++i) {
                var a = new KnrLSH();
                a.Load(Input);
                this.A[i] = a;
            }
		}

		public MKnrLSH () : base()
		{
		}

		public void SetQueryExpansion()
		{
			for (int i = 0; i < this.A.Length; ++i) {
				this.A[i] = new KnrLSHQueryExpansion(this.A[i]);
			}
		}

		public void UnsetQueryExpansion()
		{
			for (int i = 0; i < this.A.Length; ++i) {
				this.A[i] = new KnrLSH(this.A[i]);
			}
		}

		public void Build (MKnrLSH knrlsh, int num_instances)
		{
			this.A = new KnrLSH[num_instances];
			this.DB = knrlsh.DB;
			for (int i = 0; i < num_instances; ++i) {
				this.A[i] = knrlsh.A[i];
			}
		}

		public void Build (MetricDB db, int K, int num_refs, int num_instances)
        {
            this.A = new KnrLSH[num_instances];
            this.DB = db;
			var seed = RandomSets.GetRandomInt ();
			int I = 0;
			Action<int> compute = delegate (int i) {
				var a = new KnrLSH();
				a.Build(db, K, num_refs, RandomSets.GetRandom(seed + i));
				this.A[i] = a;
				++I;
				Console.WriteLine ("=== Created {0}/{1} KnrLSH index", I, num_instances);
			};
//			for(int i = 0; i < num_instances; ++i) {
//				compute.Invoke(i);
//			}
			ParallelOptions ops = new ParallelOptions ();
			ops.MaxDegreeOfParallelism = -1; 
			Parallel.For(0, num_instances, ops, compute);
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
            var L = new HashSet<int>();
			foreach (var a in this.A) {
				var h = a.GetHashKnr(q);
				List<int> M;
				if (a.TABLE.TryGetValue(h, out M)) {
					foreach (var docID in M) {
						L.Add(docID);
					}
				}
            	foreach (var docID in L) {
               		double d = this.DB.Dist (q, this.DB [docID]);
               		res.Push (docID, d);
				}
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
            return this.SearchKNN(q, this.DB.Count, new ResultRange(radius));
		}
	}
}