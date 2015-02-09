//
//  Copyright 2013  Eric Sadit Téllez Avila <donsadit@gmail.com>
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
using System.Threading;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{

	public class PMI : PMI_Partial
	{

		public PMI () : base()
		{
		}

		public PMI(PMI pmi, int lambda = 0) : base()
		{
			this.Build (pmi, lambda);
		}

    	public virtual void Build (PMI pmi, int lambda = 0)
    	{
	        this.Build(pmi.LC_LIST, lambda);
	    }

        public virtual void Build (MetricDB db, int numcenters, int lambda, int seed)
        {
            this.LC_LIST = new LC[lambda];
			LongParallel.For(0, lambda, (int i) => {
				this.BuildOneClosure(this.LC_LIST, i, db, numcenters, new Random(seed+i));
			});
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

        public override IResult SearchKNN (object q, int K, IResult res)
        {
            Action<int> on_intersection = delegate(int item) {
                var dist = this.DB.Dist (q, this.DB [item]);
                res.Push (item, dist);
            };
            var cache = new Dictionary<int, double>(this.LC_LIST[0].NODES.Count);
            return this.PartialSearchKNN (q, res, this.LC_LIST.Count, cache, on_intersection);
        }
	}
}
