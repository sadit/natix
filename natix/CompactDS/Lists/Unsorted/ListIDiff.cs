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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListIDiff.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// List integers using differences (DiffSet bitmap)
	/// </summary>
	/// <exception cref='NotSupportedException'>
	/// Is thrown when an object cannot perform an operation.
	/// </exception>
	public class ListIDiff : ListGenerator<int>, ILoadSave
	{
		public DiffSet dset;
		int last_value = -1;
	
		public ListIDiff (DiffSet d)
		{
			this.dset = d;
			this.last_value = this.dset.Select1(this.dset.Count1);
		}
		
		public ListIDiff (short B)
		{
			this.dset = new DiffSet (B);
			this.last_value = -1;
		}
		
		public ListIDiff ()
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
			if (index == 0) {
				return this.dset.Select1Difference (index + 1);
			} else {
				return this.dset.Select1Difference (index + 1) - 1;
			}
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotSupportedException ();
		}
		
		public override void Add (int item)
		{
			// + 1 because the backend coder and meaning of the skip in bits
			int current = item + 1 + this.last_value;
			this.dset.Add (current, this.last_value);
			this.last_value = current;
		}
		
		public void Save (BinaryWriter Output)
		{
			this.dset.Save (Output);
			Console.WriteLine ("===SAVE NumEnabledBits: {0}, count: {1}", this.dset.Count1, this.dset.Count);
		}
		
		public void Load (BinaryReader Input)
		{
			this.dset = new DiffSet ();
			this.dset.Load (Input);
			Console.WriteLine ("===LOAD NumEnabledBits: {0}, count: {1}", this.dset.Count1, this.dset.Count);
			this.last_value = this.dset.Select1 (this.dset.Count1);
		}
	}
}
