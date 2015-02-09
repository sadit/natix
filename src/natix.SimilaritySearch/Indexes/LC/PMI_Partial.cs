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
	public abstract class PMI_Partial : BasicIndex
	{
		public IList<LC> LC_LIST;

		public PMI_Partial ()
		{
		}

        protected virtual Action BuildOneClosure (IList<LC> output, int i, MetricDB db, int numcenters, Random rand)
        {
            var action = new Action(delegate () {
                var lc = new LC ();
                lc.Build (db, numcenters, rand);
                output[i] = lc;
            });
            return action;
        }

		public virtual void Build (IList<LC> indexlist, int max_instances = 0)
        {
            if (max_instances <= 0) {
                max_instances = indexlist.Count;
            }
			this.LC_LIST = new List<LC>();
			for (int i = 0; i < max_instances; ++i) {
                var lc = indexlist[i];
	            this.LC_LIST.Add(lc);
			}
		}

		public override MetricDB DB {
			get {
				return this.LC_LIST[0].DB;
			}
			set {
			}
		}

		public override void Save (BinaryWriter Output)
		{
			Output.Write((int) this.LC_LIST.Count);
			for (int i = 0; i < this.LC_LIST.Count; ++i) {
				IndexGenericIO.Save(Output, this.LC_LIST[i]);
			}
		}

		public override void Load (BinaryReader Input)
		{
			var count = Input.ReadInt32 ();
			this.LC_LIST = new LC [count];
			for (int i = 0; i < count; ++i) {
				this.LC_LIST[i] = (LC)IndexGenericIO.Load(Input);
			}
			// this.UI_ALG = new FastUIArray8 (this.DB.Count);
		}

        protected virtual IResult PartialSearchKNN (object q, IResult res, int max, Dictionary<int,double> cache, Action<int> on_intersection)
        {
            var queue_dist = new List<double>();
            var queue_nodes = new List<LC.Node>();
            for (int i = 0; i < max; ++i) {
                var lc = this.LC_LIST[i];
                lc.PartialSearchKNN (q, res, cache, queue_dist, queue_nodes);
            }
            byte[] A = new byte[ this.DB.Count ];
            // int max = this.LC_LIST.Count;
            // Sorting.Sort<double, LC.Node>(queue_dist, queue_nodes);
            for (int x = 0; x < queue_dist.Count; ++x) {
                var node = queue_nodes[x];
                var dcq = queue_dist[x];
				if (dcq <= res.CoveringRadius + node.cov) {
					foreach (var item in node.bucket) {
						A [item]++;	
						if (A [item] == max) {
							on_intersection (item);
						}
					}
				}
            }
            return res;           
        }
	}
}
