//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Original filename: natix/SimilaritySearch/Indexes/PolyIndexLC.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.Sets;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class PolyIndexLC_Composite : PolyIndexLC_Partial
	{
        public IndexSingle IDX;
 
		public PolyIndexLC_Composite ()
		{
		}

        public virtual void Build (MetricDB db, int numcenters, int lambda_search, int lambda_filter, int random_seed, SequenceBuilder seq_builder = null)
        {
            var L = new LC_RNN[lambda_search];
            var M = new LC_RNN[lambda_filter];
            var builder = SequenceBuilders.GetSeqPlain (short.MaxValue, ListIBuilders.GetListIFS (), null, true);
            var A = new List<Action>();
            var rand = RandomSets.GetRandom(random_seed);
            for (int i = 0; i < lambda_search; ++i) {
                A.Add(this.BuildOneClosure(L, i, db, numcenters, rand.Next(), seq_builder));
            }
            for (int i = 0; i < lambda_filter; ++i) {
                A.Add(this.BuildOneClosure(M, i, db, numcenters, rand.Next(), builder));
            }
            var ops = new ParallelOptions();
            ops.MaxDegreeOfParallelism = -1;
            Parallel.ForEach(A, ops, (action) => action());
            var poly_filter = new PolyIndexLC();
            poly_filter.Build(M, 0, null);
            this.Build(poly_filter, L);
        }

        public void BuildLAESA (IList<LC_RNN> indexlist, int max_instances = 0, int num_pivs = 0, SequenceBuilder seq_builder = null)
        {
            base.Build (indexlist, max_instances, seq_builder);
            var laesa = new LAESA ();
            if (num_pivs == 0) {
                laesa.Build (this.DB, this.LC_LIST.Count);
            } else {
                laesa.Build (this.DB, num_pivs);
            }
        }

        public void Build (IndexSingle idx, IList<LC_RNN> indexlist, int max_instances = 0, SequenceBuilder seq_builder = null)
        {
            base.Build (indexlist, max_instances, seq_builder);
            this.IDX = idx;
		}
   
        public override SearchCost Cost {
            get {
                this.internal_numdists = 0;
                foreach (var lc in this.LC_LIST) {
                    var _internal = lc.Cost.Internal;
                    this.internal_numdists += _internal;
                }
                this.internal_numdists += this.IDX.Cost.Internal;
                return base.Cost;
            }
        }

		public override void Save (BinaryWriter Output)
		{
            base.Save (Output);
            IndexGenericIO.Save (Output, this.IDX);
		}

		public override void Load (BinaryReader Input)
		{
            base.Load(Input);
            this.IDX = (IndexSingle) IndexGenericIO.Load (Input);
		}

		public override IResult SearchRange (object q, double radius)
        {
            IResult R = this.DB.CreateResult (this.DB.Count, false);
            var L = this.IDX.CreateQueryContext(q);
            Action<int> on_intersection = delegate(int item) {
                if (this.IDX.MustReviewItem(q, item, radius, L)) {
                    var dist = this.DB.Dist (q, this.DB [item]);
                    if (dist <= radius) {
                        R.Push (item, dist);
                    }
                }
            };
            var cache = new Dictionary<int, double>(this.LC_LIST[0].CENTERS.Count);
            return this.PartialSearchRange (q, radius, R, this.LC_LIST.Count, cache, on_intersection);
        }

        public override IResult SearchKNN (object q, int K, IResult R)
        {
            var L = this.IDX.CreateQueryContext(q);
            Action<int> on_intersection = delegate(int item) {
                var review = this.IDX.MustReviewItem(q, item, R.CoveringRadius, L);
                //Console.WriteLine ("review: {0}", review);
                if (review) {
                    var dist = this.DB.Dist (q, this.DB [item]);
                    R.Push (item, dist);
                }
            };
            var cache = new Dictionary<int, double>(this.LC_LIST[0].CENTERS.Count);
            return this.PartialSearchKNN (q, K, R, this.LC_LIST.Count, cache, on_intersection);
        }
	}
}
