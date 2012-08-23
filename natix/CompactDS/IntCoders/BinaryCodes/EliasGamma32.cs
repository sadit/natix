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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/EliasGamma32.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class EliasGamma32 : IIEncoder32
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

		static EliasGamma32 ()
		{
			// the table stores the number of zeros at the beggining of the byte
			// NOT USED YET, BE AWARE THAT IT'S INVERTED!
			/*PrefixSize = new byte[256];
			PrefixSize[0] = 8;
			int pos = 0;
			for (int i = 0; i < 8; i++) {
				int w = 1 << i;
				Console.WriteLine ("==> i: {0}, w: {1}", i, w, 8-i);
				for (; w > 0; w--,pos++) {
					PrefixSize[pos] = (byte)(8 - i);
				}
			}*/
		}

		// BitmapList Buffer;
		public EliasGamma32 ()
		{
			// this.Buffer = buffer;
		}

		static UnaryCoding unary = new UnaryCoding();
		public void Encode (IBitStream Buffer, int u)
		{
			if (u < 1) {
				throw new ArgumentOutOfRangeException (String.Format ("Invalid range for elias gamma coding, u: {0}", u));
			}
			var log2 = BitAccess.Log2 (u);
			--log2;
			unary.Encode (Buffer, log2);
			Buffer.Write (u, log2);
		}
		
		public int Decode (IBitStream Buffer, BitStreamCtx ctx)
		{
			// int numbits = unary.Decode (Buffer, ctx);
			// the idea is to replace unary coder by explicit inline code such that (hopefully)
			// both "code" and "numbits" are stored into registers
			var M = (int)Math.Min (32, Buffer.CountBits - ctx.Offset);
			var code = (uint)Buffer.Read (M, ctx);
			if ((code & 0x1) == 1) {
				ctx.Offset -= M - 1;
				return 1;
			}
			if (code == 0) {
				throw new ArgumentException ("Integers larger than 31 bits are not supported by EliasGamma32");
			}
			int numbits = 0;
			// Console.WriteLine ("xxxxxxxxxxxxxxxxxxxxxxxxxxxxx ");
			// Console.WriteLine ("xxxxx start-read> offset: {0},\t numbits: {1},\t code: {2}", ctx.Offset, numbits, BitAccess.ToAsciiString (code));
			while ((code & 0xFF) == 0) {
				numbits += 8;
				code >>= 8;
			}
			if ((code & 0xF) == 0) {
				numbits += 4;
				code >>= 4;
			}
			while ((code & 0x1) == 0) {
				numbits += 1;
				code >>= 1;
			}
			code >>= 1;
			// Console.WriteLine ("xxxxx unary-read> offset: {0},\t numbits: {1},\t code: {2}", ctx.Offset, numbits, BitAccess.ToAsciiString (code));
			if (numbits >= 16) {
				int in_cache = M - 1 - numbits;
				code |= ((uint)Buffer.Read (numbits - in_cache, ctx)) << in_cache;
			} else {
				ctx.Offset -= M - ((numbits << 1) + 1);
				code &= (1u << numbits) - 1;
			}
			// Console.WriteLine ("xxxxx final-read0> offset: {0},\t numbits: {1},\t code: {3},\t number: {2}", ctx.Offset, numbits, code, BitAccess.ToAsciiString (code));
			code |= (1u << numbits);
			// Console.WriteLine ("xxxxx final-read1> offset: {0},\t numbits: {1},\t code: {3},\t number: {2}", ctx.Offset, numbits, code, BitAccess.ToAsciiString (code));
			return (int)code;
		}
	}
}

