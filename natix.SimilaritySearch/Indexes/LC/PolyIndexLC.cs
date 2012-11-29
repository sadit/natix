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
using NDesk.Options;
using natix.Sets;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class PolyIndexLC : PolyIndexLC_Partial
	{

		public PolyIndexLC ()
		{
		}

        public virtual void Build (PolyIndexLC_Composite pmi, int lambda, SequenceBuilder seq_builder = null)
        {
            var list = new List<LC_RNN> ();
            foreach (var lc in pmi.LC_LIST) {
                list.Add (lc);
            }
            var as_pmi = pmi.IDX as PolyIndexLC;
            if (as_pmi != null) { 
                foreach (var lc in as_pmi.LC_LIST) {
                    var _lc = new LC();
                    _lc.Build(lc, SequenceBuilders.GetIISeq(BitmapBuilders.GetPlainSortedList()));
                    list.Add(_lc);
                }
            }
            this.Build(list, lambda, seq_builder);
        }

        public virtual void Build (MetricDB db, int numcenters, int lambda, SequenceBuilder seq_builder = null)
        {
            var A = new List<Action>();
            this.LC_LIST = new LC_RNN[lambda];
            for (int i = 0; i < lambda; ++i) {
                A.Add(this.BuildOneClosure(this.LC_LIST, i, db, numcenters, seq_builder));
            }
            var ops = new ParallelOptions();
            ops.MaxDegreeOfParallelism = -1;
            Parallel.ForEach(A, ops, (action) => action());
        }
       
        public override SearchCost Cost {
            get {
                this.internal_numdists = 0;
                foreach (var lc in this.LC_LIST) {
                    var _internal = lc.Cost.Internal;
                    this.internal_numdists += _internal;
                }
                return base.Cost;
            }
        }

		public override IResult SearchRange (object q, double radius)
        {
            IResult R = this.DB.CreateResult (this.DB.Count, false);
            Action<int> on_intersection = delegate(int item) {
                var dist = this.DB.Dist (q, this.DB [item]);
                if (dist <= radius) {
                    R.Push (item, dist);
                }
            };
            return this.PartialSearchRange (q, radius, R, on_intersection);
        }

        public override IResult SearchKNN (object q, int K, IResult R)
        {
            Action<int> on_intersection = delegate(int item) {
                var dist = this.DB.Dist (q, this.DB [item]);
                R.Push (item, dist);
            };
            return this.PartialSearchKNN (q, K, R, on_intersection);
        }
	}
}
