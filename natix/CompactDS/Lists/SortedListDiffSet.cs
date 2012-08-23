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
//   Original filename: natix/CompactDS/Lists/SortedListDiffSet.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class SortedListDiffSet : ListGenerator<int>
	{
		public DiffSet dset;
		int last_value = -1;
		
		public SortedListDiffSet (DiffSet d)
		{
			this.dset = d;
			this.last_value = this.dset.Select1(this.dset.Count1);
		}
		
		public SortedListDiffSet ()
		{
			this.dset = new DiffSet ();
			this.last_value = -1;
		}

		public override int Count {
			get {
				return this.dset.Count1;
			}
		}
		
		public override int GetItem (int index)
		{
			return this.dset.Select1 (index + 1);
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotSupportedException ();
		}
		
		public override void Add (int item)
		{
			this.dset.Add (item, this.last_value);
		}
		
		public void Save (BinaryWriter Output)
		{
			this.dset.Save (Output);
		}
		
		public void Load (BinaryReader Input)
		{
			this.dset = new DiffSet ();
			this.dset.Load (Input);
			this.last_value = this.dset.Select1 (this.dset.Count1);
		}
	}
}
