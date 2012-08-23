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
//   Original filename: natix/CompactDS/IntCoders/SearchingCodesBinary/BinarySearchCoding.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	public class BinarySearchCoding : IIEncoder32
	{
		int Size;
		
		public BinarySearchCoding () : this(32)
		{
		}
		
		public BinarySearchCoding (int size)
		{
			this.Size = size;
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.Size);
		}

		public void Load (BinaryReader Input)
		{
			this.Size = Input.ReadInt32 ();
		}


		public void Encode (IBitStream stream, int u, int N)
		{
			int min = 0;
			int max = N - 1;
			int mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if (1 == (min & 1 & max)) {
					mid++;
				}
				if (u <= mid) {
					stream.Write (false);
					max = mid;
				} else {
					stream.Write (true);
					min = mid + 1;
				}
			} while (min < max);
		}
		
		public int Decode (IBitStream stream, int N, BitStreamCtx ctx)
		{
			int min = 0;
			int max = N - 1;
			int mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if (1 == (min & 1 & max)) {
					mid++;
				}
				if (!stream.Read (ctx)) {
					max = mid;
				} else {
					min = mid + 1;
				}
			} while (min < max);
			return min;		
		}
		
		public void Encode (IBitStream stream, int u)
		{
			this.Encode (stream, u, this.Size);
		}

	
		public int Decode (IBitStream stream, BitStreamCtx ctx)
		{
			return this.Decode (stream, this.Size, ctx);
		}
	}
}

