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
// Eric S. Tellez
// - Load and Save methods
// - Everything was modified to compute slices using radius instead of the percentiles
// - Argument minimum bucket size

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
		public PivotGroup.Pivot[] _Pivs;
        public DiskList64<ItemPair> _Items;
		public string Filename;

		public DiskPivotGroup ()
		{
		}

		public void CachePivObjects(MetricDB db)
		{
			this._PivObjects = new object[this._Pivs.Length];
			for (int pivID = 0; pivID < this._Pivs.Length; ++pivID) {
				this._PivObjects [pivID] = db [this._Pivs [pivID].objID];
			}
		}

		public void Load(BinaryReader Input)
		{
			var len = Input.ReadInt32 ();
			this._Pivs = CompositeIO<PivotGroup.Pivot>.LoadVector (Input, len, null) as PivotGroup.Pivot[];
			this.Filename = Input.ReadString ();
			this._Items = new DiskList64<ItemPair>(this.Filename, 1024);
		}

		public void Save (BinaryWriter Output)
        {
			Output.Write (this._Pivs.Length);
			CompositeIO<PivotGroup.Pivot>.SaveVector (Output, this._Pivs);
			Output.Write (this.Filename);
            // CompositeIO<ItemPair>.SaveVector (Output, this._Items);
		}

        protected virtual void SearchExtremes (DynamicSequential idx, List<ItemPair> items, object piv, double alpha_stddev, int min_bs, out IResult near, out IResult far, out DynamicSequential.Stats stats)
        {
            throw new NotSupportedException();
        }

        public virtual void Build (PivotGroup g, string filename)
        {
			this._Pivs = g._Pivs;
			var num_groups = g._Items.Length;
			this._Items = new DiskList64<ItemPair> (filename, 1024);
			foreach (var p in g._Items) {
				this._Items.Add(p);
			}
        }
	}
}

