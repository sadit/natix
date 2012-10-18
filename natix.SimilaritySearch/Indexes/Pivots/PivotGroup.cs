//
//  Copyright 2012  Francisco Santoyo
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
// Eric S. Tellez
// Load and Save methods

using System;
using System.IO;
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class PivotGroup : ILoadSave
	{
		public IList<int> pivots_list;
		public int[] pivots_idx;
		public double[] pivots_dist;

		public PivotGroup ()
		{
		}

		public PivotGroup (int n)
		{
			pivots_list = new List<int>();
			pivots_idx = new int[n];
			pivots_dist = new double[n];
		}

		public void Load(BinaryReader Input)
		{
			this.pivots_list = ListIGenericIO.Load (Input);
			var len = Input.ReadInt32 ();
			this.pivots_idx = (int[])PrimitiveIO<int>.ReadFromFile(Input, len, null);
			this.pivots_dist = (double[])PrimitiveIO<double>.ReadFromFile(Input, len, null);
		}

		public void Save(BinaryWriter Output)
		{
			ListIGenericIO.Save(Output, this.pivots_list);
			Output.Write(this.pivots_idx.Length);
			PrimitiveIO<int>.WriteVector(Output, this.pivots_idx);
			PrimitiveIO<double>.WriteVector(Output, this.pivots_dist);
		}
	}
}

