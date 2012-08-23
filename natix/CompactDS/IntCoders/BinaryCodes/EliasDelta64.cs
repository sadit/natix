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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/EliasDelta64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class EliasDelta64 : IIEncoder64
	{
		static EliasGamma32 gammacoder = new EliasGamma32();

		public void Save (BinaryWriter Output)
		{
		}

		public void Load (BinaryReader Input)
		{
		}
		
		public EliasDelta64 ()
		{
		}
		
		public void Encode (IBitStream Buffer, long u)
		{
			if (u < 1) {
				throw new ArgumentOutOfRangeException (String.Format ("Invalid range for elias delta coding, u: {0}", u));
			}
			var log2 = BitAccess.Log2 (u);
			gammacoder.Encode (Buffer, log2);
			//Buffer.Write (u, log2 - 1);
			--log2;
			if (log2 <= 32) {
				Buffer.Write ((uint)u, log2);
			} else {
				Buffer.Write ((uint)u, 32);
				Buffer.Write (u >> 32, log2 - 32);
			}
		}
		
		public long Decode (IBitStream Buffer, BitStreamCtx ctx)
		{
			int len_code = gammacoder.Decode (Buffer, ctx);
			--len_code;
			if (len_code == 0) {
				return 1L;
			} else {
				ulong number; 
				if (len_code <= 32) {
					number = Buffer.Read (len_code, ctx);
				} else {
					number = Buffer.Read (32, ctx);
					number |= Buffer.Read (len_code - 32, ctx) << 32;
				}
				number = (1UL << len_code) | number;
				return (long)number;

			}
		}
	}
}

