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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListIFS.cs
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
	public class ListIFS : ListGenerator<int>, ILoadSave
	{
		public BinaryCoding Coder;
		public IBitStream Stream;
		// int Counter;
		
		public static int GetNumBits (int maxvalue)
		{
			return (int)Math.Ceiling (Math.Log (maxvalue + 1, 2));
		}

		public void Save (BinaryWriter bw)
		{
			bw.Write ((int)this.Coder.NumBits);
			this.Stream.Save (bw);
		}
		
		public void Load (BinaryReader reader)
		{
			var numbits = reader.ReadInt32 ();
			var stream = new BitStream32 ();
			stream.Load (reader);
			this.Build (numbits, stream);
		}

		public ListIFS (int numbits, IBitStream stream) : base()
		{
			this.Build (numbits, stream);
		}
		
		public ListIFS (int numbits) : base()
		{
			this.Build (numbits, new BitStream32 ());
		}
		
	
		public void Build (int numbits, IBitStream stream)
		{
			this.Coder = new BinaryCoding (numbits);
			this.Stream = stream;
		}

		public ListIFS () : base()
		{
			this.Stream = null;
			this.Coder = null;
		}
		
		public override int Count {
			get {
				return (int)(this.Stream.CountBits / this.Coder.NumBits);
			}
		}

		public void Add (int item, int times)
		{
			for (int i = 0; i < times; i++) {
				this.Coder.ArrayAdd (this.Stream, item);
			}
		}

		public override void Add (int item)
		{
			this.Coder.ArrayAdd (this.Stream, item);
		}

		public override int GetItem (int index)
		{
			return this.Coder.ArrayGet (this.Stream, index);
		}
		
		public override void SetItem (int index, int u)
		{
			this.Coder.ArraySet (this.Stream, index, u);
		}
	}
}
