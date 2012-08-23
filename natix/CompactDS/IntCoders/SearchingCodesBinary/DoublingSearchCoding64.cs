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
//   Original filename: natix/CompactDS/IntCoders/SearchingCodesBinary/DoublingSearchCoding64.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	public class DoublingSearchCoding64 : IIEncoder64
	{
		BinarySearchCoding64 SecondCoding;
		
		public DoublingSearchCoding64 ()
		{
			this.SecondCoding = new BinarySearchCoding64 (0);
		}

		public void Save (BinaryWriter Output)
		{
		}

		public void Load (BinaryReader Input)
		{
		}

		public void Encode (IBitStream stream, long u)
		{
			long check;
			long min = 0;
			int galloping = 1;
			while (true) {
				check = (1L << galloping) - 1L;
				if (u > check) {
					stream.Write (true);
					min = check + 1L;
					++galloping;
				} else {
					stream.Write (false);
					if (min == 0L) {
						this.SecondCoding.Encode (stream, u, 2);
					} else {
						this.SecondCoding.Encode (stream, u - min, min);
					}
					break;
				}
			}
		}
		
		public long Decode (IBitStream stream, BitStreamCtx ctx)
		{
			long min = 0;
			long check;
			int galloping = 1;
			while (true) {
				check = (1L << galloping) - 1L;
				if (stream.Read (ctx)) {
					min = check + 1L;
					++galloping;
				} else {
					if (min == 0L) {
						return this.SecondCoding.Decode (stream, 2, ctx);
					} else {
						return min + this.SecondCoding.Decode (stream, min, ctx);
					}
				}
			}
		}
	}
}

