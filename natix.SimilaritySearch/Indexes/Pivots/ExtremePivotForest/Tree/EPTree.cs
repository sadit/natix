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
		public int[] Items;

		public virtual void Load(BinaryReader Input)
		{
			int len;
			len = Input.ReadInt32 ();
			this.Pivs = CompositeIO<EPivot>.LoadVector (Input, len, null) as EPivot[];
			len = Input.ReadInt32 ();
			this.Items = PrimitiveIO<int>.ReadFromFile (Input, len, null) as int[];
		}
		
		public virtual void Save (BinaryWriter Output)
		{
			Output.Write (this.Pivs.Length);
			CompositeIO<EPivot>.SaveVector (Output, this.Pivs);
			Output.Write (this.Items.Length);
			PrimitiveIO<int>.WriteVector (Output, this.Items);
		}
		

		public EPTree ()
		{

		}

		public EPTree (EPList list)
		{
			var H = new HashSet<int> ();
			var D = new List<List<int>> (list.Pivs.Length);
			for (int pivID = 0; pivID < list.Pivs.Length; ++pivID) {
				D.Add(new List<int>());
				H.Add(list.Pivs[pivID].objID);
			}
			var n = list.Items.Length;
			for (int objID = 0; objID < n; ++objID) {
				var item = list.Items[objID];
				// EPTree stores Items as tuples (objID, dist)
				// EPList stores Items as tuples (pivID, dist)
				if (!H.Contains(objID)) {
					D[item.objID].Add(objID);
				}
			}
			this.Pivs = new EPivot[ list.Pivs.Length ];
			this.Items = new int[ list.Items.Length ];
			int abs_pos = 0;
			for (int pivID = 0; pivID < list.Pivs.Length; ++pivID) {
				D[pivID].Sort( (x,y) => list.Items[x].dist.CompareTo(list.Items[y].dist) );
				var lpiv = list.Pivs[pivID];
				var piv = new EPivot(lpiv.objID, lpiv.stddev, lpiv.mean, 0, double.MaxValue, 0, 0);
				this.Pivs[pivID] = piv;
				foreach (var objID in D[pivID]) {
					this.Items[abs_pos] = objID;
					++abs_pos;
					var item = list.Items[objID];
					if (item.dist <= piv.mean) {
						piv.num_near++;
						piv.last_near = Math.Max (piv.last_near, item.dist);
					} else {
						piv.num_far++;
						piv.first_far = Math.Max (piv.first_far, item.dist);
					}
				}
				D[pivID] = null;
			}
		}

		
		public virtual void SearchKNN (MetricDB db, object q, int K, IResult res,
		                               short[] A,
		                               short current_rank_A, double[] D)
		{
			int abs_pos = 0;
			for (int pivID = 0; pivID < this.Pivs.Length; ++pivID) {
				var piv = this.Pivs[pivID];
//				var pivOBJ = db [piv.objID];
//				var dqp = db.Dist (q, pivOBJ);
//				res.Push (piv.objID, dqp);
				var dqp = D[ pivID ];
				// checking near ball radius
				if (dqp <= piv.last_near + res.CoveringRadius) {
					for (int j = 0; j < piv.num_near; ++j, ++abs_pos) {
						var objID = this.Items [abs_pos];
						++A [objID];
					}
				} else {
					abs_pos += piv.num_near;
				}
				// checking external radius
				if (dqp + res.CoveringRadius >= piv.first_far) {
					for (int j = 0; j < piv.num_far; ++j, ++abs_pos) {
						var objID = this.Items [abs_pos];
						++A [objID];
					}
				} else {
					abs_pos += piv.num_far;
				}
				// This cannot be ensured in the stealing pivot construction
				//	if (dqp + res.CoveringRadius <= piv.last_near || piv.first_far <= dqp - res.CoveringRadius) {
				//		break;
				//	}
			}
		}
	}
}

