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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListSDiff64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace natix.CompactDS
{
	public class ListSDiff64 : ListGenerator<int>, ILoadSave
	{
		static IIEncoder32 Coder = new ZeroCoding(new EliasDelta(), 1);
		public IBitStream Stream;
		IList<long> Offsets;
		public int BlockSize = 63;
		int M;
		
		public ListSDiff64 ()
		{
			this.Stream = new BitStream32 ();
			this.Offsets = new List<long> ();
		}
		
		public ListSDiff64 (int B) : this()
		{
			this.BlockSize = B;
		}
		
		public override int Count {
			get {
				return this.M;
			}
		}
		
		public override void Add (int u)
		{
			this.M++;
			Coder.Encode (this.Stream, u);
			if (this.M % this.BlockSize == 0) {
				this.Offsets.Add (this.Stream.CountBits);
			}
		}
		
		public override int GetItem (int index)
		{
			return this.GetItem (index, new BitStreamCtx());
		}
		
		public int GetItem (int index, BitStreamCtx ctx)
		{

			int offset_index = index / this.BlockSize;
			if (offset_index == 0) {
				ctx.Seek(0);
			} else {
				ctx.Seek(this.Offsets[offset_index-1]);
			}
			int left = 1 + index - offset_index * this.BlockSize;
			int res = -1;
			for (int i = 0; i < left; i++) {
				res = Coder.Decode(this.Stream, ctx);
			}
			return res;
		}
		
		public IEnumerable<int> ExtractFrom (int start, int count)
		{
			BitStreamCtx ctx = new BitStreamCtx ();
			yield return this.GetItem (start, ctx);
			for (int i = 1; i < count; i++) {
				yield return Coder.Decode (this.Stream, ctx);
			}
		}
		
		public void Load (BinaryReader Input)
		{
			this.BlockSize = Input.ReadInt32 ();
			this.M = Input.ReadInt32 ();
			this.Stream.Load (Input);
			int num_offsets = Input.ReadInt32();
			this.Offsets = new long[num_offsets];
			PrimitiveIO<long>.ReadFromFile(Input, num_offsets, this.Offsets);
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.BlockSize);
			Output.Write ((int)this.M);
			this.Stream.Save(Output);
			Output.Write((int) this.Offsets.Count);
			PrimitiveIO<long>.WriteVector(Output, this.Offsets);
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotSupportedException ();
		}
	}
}

