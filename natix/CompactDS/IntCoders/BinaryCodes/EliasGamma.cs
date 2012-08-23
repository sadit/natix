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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/EliasGamma.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class EliasGamma : IIEncoder32
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

		static EliasGamma ()
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
		public EliasGamma ()
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
			int numbits = unary.Decode (Buffer, ctx);
			if (numbits == 0) {
				return 1;
			} else {
				int number = (int) Buffer.Read (numbits, ctx);
				return (1 << numbits) | number;
			}
		}
	}
}

