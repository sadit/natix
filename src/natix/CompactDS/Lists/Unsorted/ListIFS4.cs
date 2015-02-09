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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListIFS4.cs
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
	public class ListIFS4 : ListGenerator<int>, ILoadSave
	{
		public IList<byte> Data;
		int n;

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.n);
			Output.Write (this.Data.Count);
			PrimitiveIO<byte>.SaveVector(Output, this.Data);
		}
		
		public void Load (BinaryReader Input)
		{
			this.n = Input.ReadInt32 ();
			var len = Input.ReadInt32();
			this.Data = PrimitiveIO<byte>.LoadVector(Input, len, null);
		}

		public ListIFS4 () : base()
		{
			this.Data = new List<byte>();
			this.n = 0;
		}
		
		public override int Count {
			get {
				return this.n;
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
			var _n = this.Count;
			if ((_n & 1) == 1) {
				var lastpos = this.Data.Count - 1;
				int d = this.Data [lastpos];
				d |= item << 4;
				this.Data [lastpos] = (byte)(d & 255);
			} else {
				this.Data.Add ((byte)(item & 15));
			}
			this.n++;
		}

		public override int GetItem (int index)
		{
			if ((index & 1) == 1) {
				return this.Data [index >> 1] >> 4;
			} else {
				return (this.Data [index >> 1]) & 15;
			}
		}

		public override void SetItem (int index, int u)
		{
			int d = this.Data[index >> 1];
			u = u & 0x00ff;
			if ((index & 1) == 1) {
				d = d & 0x00ff;
				d |= (u << 4);
			} else {
				d &= 0xff00;
				d |= u;
			}
			this.Data [index >> 1] = (byte) d;
		}
	}
}
