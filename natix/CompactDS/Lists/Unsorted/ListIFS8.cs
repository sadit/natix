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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListIFS8.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// List of integers of fixed size
	/// </summary>
	public class ListIFS8 : ListGenerator<int>, ILoadSave
	{
		public IList<byte> Data;

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.Data.Count);
			PrimitiveIO<byte>.SaveVector(Output, this.Data);
		}
		
		public void Load (BinaryReader Input)
		{
			var len = Input.ReadInt32();
			this.Data = PrimitiveIO<byte>.LoadVector(Input, len, null);
		}

		public ListIFS8 () : base()
		{
			this.Data = new List<byte>();
		}
		
		public override int Count {
			get {
				return this.Data.Count;
			}
		}

		public void Add (int item, int times)
		{
			for (int i = 0; i < times; i++) {
				this.Add (item);
			}
		}

		public override void Add (int item)
		{
			this.Data.Add((byte)(item & 255));
		}

		public override int GetItem (int index)
		{
			return this.Data[index];
		}

		public override void SetItem (int index, int u)
		{
			this.Data[index] = (byte) ( u & 255);
		}
	}
}
