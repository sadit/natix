//
//  Copyright 2013     Eric Sadit Tellez Avila
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
// 

using System;
using System.IO;
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class EPTree : ILoadSave
	{			
		public EPivot[] Pivs;
		public ItemPair[] Items;

		public virtual void Load(BinaryReader Input)
		{
			int len;
			len = Input.ReadInt32 ();
			this.Pivs = CompositeIO<EPivot>.LoadVector (Input, len, null) as EPivot[];
			len = Input.ReadInt32 ();
			this.Items = CompositeIO<ItemPair>.LoadVector(Input, len, null) as ItemPair[];
		}
		
		public virtual void Save (BinaryWriter Output)
		{
			Output.Write (this.Pivs.Length);
			CompositeIO<EPivot>.SaveVector (Output, this.Pivs);
			Output.Write (this.Items.Length);
			CompositeIO<ItemPair>.SaveVector (Output, this.Items);
		}
		

		public EPTree ()
		{

		}

		public EPTree (EPList list)
		{
			var H = new HashSet<int> ();
			var D = new List<List<ItemPair>> (list.Pivs.Length);
			for (int pivID = 0; pivID < list.Pivs.Length; ++pivID) {
				D.Add(new List<ItemPair>());
				H.Add(list.Pivs[pivID].objID);
			}
			var n = list.Items.Length;
			for (int objID = 0; objID < n; ++objID) {
				var item = list.Items[objID];
				// EPTree stores Items as tuples (objID, dist)
				// EPList stores Items as tuples (pivID, dist)
				if (!H.Contains(objID)) {
					D[item.ObjID].Add(new ItemPair(objID, item.Dist));
				}
			}
			this.Pivs = new EPivot[ list.Pivs.Length ];
			this.Items = new ItemPair[ list.Items.Length ];
			int abs_pos = 0;
			for (int pivID = 0; pivID < list.Pivs.Length; ++pivID) {
				D[pivID].Sort( (x,y) => x.Dist.CompareTo(y.Dist) );
				var lpiv = list.Pivs[pivID];
				var piv = new EPivot(lpiv.objID, lpiv.stddev, lpiv.mean, 0, double.MaxValue, 0, 0);
				this.Pivs[pivID] = piv;
				foreach (var item in D[pivID]) {
					this.Items[abs_pos] = item;
					++abs_pos;
					if (item.Dist <= piv.mean) {
						piv.num_near++;
						piv.last_near = Math.Max (piv.last_near, item.Dist);
					} else {
						piv.num_far++;
						piv.first_far = Math.Max (piv.first_far, item.Dist);
					}
				}
				D[pivID] = null;
			}
		}

		
		public virtual void SearchKNN (MetricDB db, object q, int K, IResult res, byte[] A, int rank, float[] Linf, double[] D)
		{
			int abs_pos = 0;

			for (int pivID = 0; pivID < this.Pivs.Length; ++pivID) {
				var piv = this.Pivs[pivID];
				var dqp = D[ pivID ];
				// checking near ball radius
				var rad = res.CoveringRadius;
				if (dqp <= piv.last_near + rad) {
					for (int j = 0; j < piv.num_near; ++j, ++abs_pos) {
						var item = this.Items [abs_pos];
						var abs_diff = Math.Abs(item.Dist - dqp);
						if (A[item.ObjID] == rank && abs_diff <= rad) {
							++A[item.ObjID];
							Linf[item.ObjID] = (float)Math.Max(Linf[item.ObjID], abs_diff);
						}
					}
				} else {
					abs_pos += piv.num_near;
				}
				// checking external radius
				if (dqp + rad >= piv.first_far) {
					for (int j = 0; j < piv.num_far; ++j, ++abs_pos) {
						var item = this.Items [abs_pos];
						var abs_diff = Math.Abs(item.Dist - dqp);
						if (A[item.ObjID] == rank && abs_diff <= rad) {
							++A[item.ObjID];
							Linf[item.ObjID] = (float)Math.Max(Linf[item.ObjID], abs_diff);
						}
					}
				} else {
					abs_pos += piv.num_far;
				}
			}
		}
	}
}

