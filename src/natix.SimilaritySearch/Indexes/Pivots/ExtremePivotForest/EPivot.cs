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
	public class EPivot : ILoadSave
	{
		public int objID;
		public double stddev;
		public double mean;
		public double last_near;
		public double first_far;
		public int num_near;
		public int num_far;

		public EPivot()
		{
		}
			
		public EPivot(int objID, double stddev, double mean, double cov_near, double cov_far, int num_near, int num_far)
		{
			this.objID = objID;
			this.stddev = stddev;
			this.mean = mean;
			this.last_near = cov_near;
			this.first_far = cov_far;
			this.num_near=num_near;
			this.num_far=num_far;
		}

		public void Load(BinaryReader Input)
			{
			this.objID = Input.ReadInt32 ();
			this.stddev = Input.ReadDouble();
			this.mean = Input.ReadDouble();
			this.last_near = Input.ReadDouble();
			this.first_far = Input.ReadDouble();
			this.num_near= Input.ReadInt32();
			this.num_far=Input.ReadInt32();
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.objID);
			Output.Write (this.stddev);
			Output.Write (this.mean);
			Output.Write (this.last_near);
			Output.Write (this.first_far);
			Output.Write (this.num_near);
			Output.Write (this.num_far);
		}

		public override string ToString ()
		{
			return string.Format ("[EPivot objID: {0}, stddev: {1}, mean: {2}, last_near: {3}, first_far: {4}, num_near: {5}, num_far: {6}]",
			                      this.objID, this.stddev, this.mean, this.last_near, this.first_far, this.num_near, this.num_far);
		}
				
	}
}

