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
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class LAESA_ExternalPivots : BasicIndex, IndexSingle
	{
		public IList<float>[] DIST;
		public MetricDB PIVS;

		public LAESA_ExternalPivots ()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.PIVS = SpaceGenericIO.SmartLoad(Input, false);
			this.DIST = new IList<float>[this.PIVS.Count];
			for (int i = 0; i < this.PIVS.Count; ++i) {
				this.DIST[i] = PrimitiveIO<float>.ReadFromFile(Input, this.DB.Count, null);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.PIVS);
			for (int i = 0; i < this.PIVS.Count; ++i) {
				PrimitiveIO<float>.WriteVector(Output, this.DIST[i]);
			}
		}

        public void Build (LAESA_ExternalPivots idx)
        {
            this.DB = idx.DB;
            var num_pivs = this.PIVS.Count;
            this.PIVS = idx.PIVS;
            this.DIST = new IList<float>[num_pivs];
            int I = 0;
            Action<int> one_pivot = delegate (int i) {
                var L = new List<float>(idx.DIST[i]);
                this.DIST[i] = L;
                if (I % 10 == 0) {
                    Console.WriteLine("LAESA_ExternalPivots Build, advance {0}/{1} (approx.) ", I, num_pivs);
                }
                I++;
            };
            Parallel.For (0, num_pivs, one_pivot);
        }

		public void Build (MetricDB db, MetricDB pivs)
		{
			var laesa = new LAESA_ExternalPivots();
			laesa.BuildLazy(db, pivs);
            this.DB = db;
            this.PIVS = pivs;
            this.Build(laesa);

		}

		IList<float> GetLazyDIST (int piv)
		{
			var seq = new ListGen<float>((int index) => {
				var d = this.DB.Dist (this.PIVS[piv], this.DB [index]);
				return (float)d;
			}, this.DB.Count);
			return seq;
		}

		public void BuildLazy (MetricDB db, MetricDB pivs)
		{
			this.DB = db;
            this.PIVS = pivs;
			this.DIST = new IList<float>[pivs.Count];
			for (int i = 0; i < pivs.Count; ++i) {
				this.DIST[i] = this.GetLazyDIST(i);
			}
		}

		public override IResult SearchKNN (object q, int K, IResult res)
        {		
            var m = this.PIVS.Count;
            var n = this.DB.Count;
            var dqp_vec = new double[ m ];
            for (int pivID = 0; pivID < m; ++pivID) {
                dqp_vec[pivID] = this.DB.Dist(q, this.PIVS[pivID]);
            }
            this.internal_numdists += m;
			for (int docID = 0; docID < n; ++docID) {
				bool check_object = true;
				for (int pivID = 0; pivID < m; ++pivID) {
                    //var db_pivID = _PIVS[__pivID];
                    var dqp = dqp_vec[pivID];
					var dpu = this.DIST[pivID][docID];
					if (Math.Abs (dqp - dpu) > res.CoveringRadius) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					res.Push(docID, this.DB.Dist(q, this.DB[docID]));
				}
			}
			return res;
		}
              
        public object CreateQueryContext (object q)
        {
            var m = this.PIVS.Count;
            var L = new double[ m ];
            for (int pivID = 0; pivID < m; ++pivID) {
                ++this.internal_numdists;
                L[pivID] = this.DB.Dist(q, this.PIVS[pivID]);
            }
            return L;
        }

        public bool MustReviewItem (object q, int item, double radius, object ctx)
        {
            var pivs = ctx as Double[];
            var m = this.PIVS.Count;
            for (int pivID = 0; pivID < m; ++pivID) {
                var P = this.DIST[pivID];
                if (Math.Abs (P[item] - pivs[pivID]) > radius) {
                    return false;
                }
            }
            return true;
        }

        public override IResult SearchRange (object q, double radius)
        {
            var res = new ResultRange(radius, this.DB.Count);
            return this.SearchKNN(q, this.DB.Count, res);
        }
	}
}

