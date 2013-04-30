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
	public class EPList : ILoadSave
	{			
		public EPivot[] Pivs;
		public ItemPair[] Items;
		
		public EPList ()
		{
		}
		 
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
	}
}

