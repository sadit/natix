//
//  Copyright 2013  Eric Sadit Tellez Avila
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

namespace natix.SimilaritySearch
{
	public class DiskPivotGroup : ILoadSave
	{
		public object[] _PivObjects;
		public PivotGroup.Pivot[] Pivs;
        public DiskList64<ItemPair> DiskItems;
		public string Filename;

		public DiskPivotGroup ()
		{
		}

		protected void CachePivObjects(MetricDB db)
		{
			if (this._PivObjects == null) { 
				this._PivObjects = new object[this.Pivs.Length];
				for (int pivID = 0; pivID < this.Pivs.Length; ++pivID) {
					this._PivObjects [pivID] = db [this.Pivs [pivID].objID];
				}
			}
		}

		public void Load(BinaryReader Input)
		{
			var len = Input.ReadInt32 ();
			this.Pivs = CompositeIO<PivotGroup.Pivot>.LoadVector (Input, len, null) as PivotGroup.Pivot[];
			this.Filename = Input.ReadString ();
			this.DiskItems = new DiskList64<ItemPair>(this.Filename, 1024);
		}

		public void Save (BinaryWriter Output)
        {
			Output.Write (this.Pivs.Length);
			CompositeIO<PivotGroup.Pivot>.SaveVector (Output, this.Pivs);
			Output.Write (this.Filename);
            // CompositeIO<ItemPair>.SaveVector (Output, this._Items);
		}

//        protected virtual void SearchExtremes (DynamicSequential idx, List<ItemPair> items, object piv, double alpha_stddev, int min_bs, out IResult near, out IResult far, out DynamicSequential.Stats stats)
//        {
//            throw new NotSupportedException();
//        }

        public virtual void Build (PivotGroup g, string filename)
        {
			this.Pivs = g.Pivs;
//			var num_groups = g.Items.Length;
			this.DiskItems = new DiskList64<ItemPair> (filename, 1024);
			foreach (var p in g.Items) {
				this.DiskItems.Add(p);
			}
        }

		public int SearchKNN (MetricDB db, object q, int K, IResult res, short[] A)
		{
			this.CachePivObjects (db);
			int abs_pos = 0;
			int inner_numdist = 0;
			for (int pivID = 0; pivID < this.Pivs.Length; ++pivID) {
				var piv = this.Pivs [pivID];
				var pivOBJ = this._PivObjects [pivID];
				//foreach (var piv in group._Pivs) {
				// var pivOBJ = this.DB[piv.objID];
				var dqp = db.Dist (q, pivOBJ);
				res.Push (piv.objID, dqp);
				++inner_numdist;
				// checking near ball radius
				if (dqp <= piv.last_near + res.CoveringRadius) {
					var bucket_size = piv.num_near;
					var bucket = this.DiskItems.ReadArray (abs_pos, bucket_size);
					abs_pos += bucket_size;
					foreach (var item in bucket) {
						// checking covering pivot
						if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
							++A [item.objID];
						}
					}
				} else {
					abs_pos += piv.num_near;
				}
				// checking external radius
				if (dqp + res.CoveringRadius >= piv.first_far) {
					var bucket_size = piv.num_far;
					var bucket = this.DiskItems.ReadArray (abs_pos, bucket_size);
					abs_pos += bucket_size;
					foreach (var item in bucket) {
						// checking covering pivot
						if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
							++A [item.objID];
						}
					}
				} else {
					abs_pos += piv.num_far;
				}
				if (dqp + res.CoveringRadius <= piv.last_near || piv.first_far <= dqp - res.CoveringRadius) {
					break;
				}
			}
			return inner_numdist;
		}
	}
}

