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
//   Original filename: natix/CompactDS/IntCoders/SearchingCodesBinary/BinarySearchCoding64.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	public class BinarySearchCoding64 : IIEncoder64
	{
		long MaxValue;
		
		public BinarySearchCoding64 () : this(long.MaxValue)
		{
		}
		
		public BinarySearchCoding64 (long maxvalue)
		{
			this.MaxValue = maxvalue;
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((long)this.MaxValue);
		}

		public void Load (BinaryReader Input)
		{
			this.MaxValue = Input.ReadInt64 ();
		}


		public void Encode (IBitStream stream, long u, long N)
		{
			long min = 0;
			long max = N - 1;
			long mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if (1L == (min & 1L & max)) {
					mid++;
				}
				if (u <= mid) {
					stream.Write (false);
					max = mid;
				} else {
					stream.Write (true);
					min = mid + 1L;
				}
			} while (min < max);
		}
		
		public long Decode (IBitStream stream, long N, BitStreamCtx ctx)
		{
			long min = 0;
			long max = N - 1;
			long mid;
			do {
				mid = (min >> 1) + (max >> 1);
				if (1L == (min & 1L & max)) {
					mid++;
				}
				if (!stream.Read (ctx)) {
					max = mid;
				} else {
					min = mid + 1L;
				}
			} while (min < max);
			return min;	
		}
		
		public void Encode (IBitStream stream, long u)
		{
			this.Encode (stream, u, this.MaxValue);
		}

	
		public long Decode (IBitStream stream, BitStreamCtx ctx)
		{
			return this.Decode (stream, this.MaxValue, ctx);
		}
	}
}

