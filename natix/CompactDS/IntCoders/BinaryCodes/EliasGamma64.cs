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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/EliasGamma64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class EliasGamma64 : IIEncoder64
	{
		/// <summary>
		/// Stores the number of zeros at the beggining
		/// </summary>
		public void Save (BinaryWriter Output)
		{
		}

		public void Load (BinaryReader Input)
		{
		}

		static EliasGamma64 ()
		{
		}

		public EliasGamma64 ()
		{
			// this.Buffer = buffer;
		}

		static UnaryCoding unary = new UnaryCoding();
		public void Encode (IBitStream Buffer, long u)
		{
			if (u < 1) {
				throw new ArgumentOutOfRangeException (String.Format ("Invalid range for elias gamma coding, u: {0}", u));
			}
			var log2 = BitAccess.Log2 (u);
			--log2;
			unary.Encode (Buffer, log2);
			if (log2 <= 32) {
				Buffer.Write ((int)u, log2);
			} else {
				Buffer.Write ((int)u, 32);
				Buffer.Write (u >> 32, log2 - 32);
			}
		}
		
		public long Decode (IBitStream Buffer, BitStreamCtx ctx)
		{
			int numbits = unary.Decode (Buffer, ctx);
			if (numbits == 0) {
				return 1L;
			} else {
				ulong number; 
				if (numbits <= 32) {
					number = Buffer.Read (numbits, ctx);
				} else {
					number = Buffer.Read (32, ctx);
					number |= Buffer.Read (numbits - 32, ctx) << 32;
				}
				number = (1UL << numbits) | number;
				return (long)number;
			}
		}
	}
}

