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

namespace natix.SimilaritySearch
{
	public class PolyIndexLC_LAESA : PolyIndexLC_Partial
	{
        public LAESA laesa;

		public PolyIndexLC_LAESA ()
		{
		}

        public override void Build (IList<LC_RNN> indexlist, int max_instances = 0, SequenceBuilder seq_builder = null)
        {
            this.Build(indexlist, max_instances, max_instances, seq_builder);
        }

        public void Build (IList<LC_RNN> indexlist, int max_instances = 0, int num_pivots = 0, SequenceBuilder seq_builder = null)
        {
            base.Build (indexlist, max_instances, seq_builder);
            if (num_pivots == 0) {
                num_pivots = this.LC_LIST.Count;
            }
            this.laesa = new LAESA();
            this.laesa.Build(this.DB, num_pivots);
		}
   
        public override SearchCost Cost {
            get {
                this.internal_numdists = 0;
                foreach (var lc in this.LC_LIST) {
                    var _internal = lc.Cost.Internal;
                    this.internal_numdists += _internal;
                }
                this.internal_numdists += this.laesa.Cost.Internal;
                return base.Cost;
            }
        }

		public override void Save (BinaryWriter Output)
		{
            base.Save (Output);
            IndexGenericIO.Save (Output, this.laesa);
		}

		public override void Load (BinaryReader Input)
		{
            base.Load(Input);
            this.laesa = (LAESA)IndexGenericIO.Load (Input);
		}


		public override IResult SearchRange (object q, double radius)
        {
            IResult R = this.DB.CreateResult (this.DB.Count, false);
            var L = this.laesa.CreateQueryContext(q);
            Action<int> on_intersection = delegate(int item) {
                if (this.laesa.MustReviewItem(item, radius, L)) {
                    var dist = this.DB.Dist (q, this.DB [item]);
                    if (dist <= radius) {
                        R.Push (item, dist);
                    }
                }
            };
            return this.PartialSearchRange (q, radius, R, on_intersection);
        }

        public override IResult SearchKNN (object q, int K, IResult R)
        {
            var L = this.laesa.CreateQueryContext(q);
            Action<int> on_intersection = delegate(int item) {
                if (this.laesa.MustReviewItem(item, R.CoveringRadius, L)) {
                    var dist = this.DB.Dist (q, this.DB [item]);
                    R.Push (item, dist);
                }
            };
            return this.PartialSearchKNN (q, K, R, on_intersection);
        }
	}
}
