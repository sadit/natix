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
//   Original filename: natix/SimilaritySearch/Indexes/BinPermsTwoBit.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Brief permutations encoded with two bits. Approximated index.
	/// </summary>
	public class BinPermsTwoBit : BinPerms
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public BinPermsTwoBit () : base()
		{
		}

		/// <summary>
		/// Returns the length in bytes of the encoded permutation
		/// </summary>
		public override int GetDimLengthInBytes (int invlen)
		{
			return invlen >> 2;
		}

		/// <summary>
		///  Encode the inverse permutation into a bit-string / brief permutation
		/// </summary>
		public override byte[] Encode (Int16[] inv)
		{
			int len = this.GetDimLengthInBytes (inv.Length);
			//Console.WriteLine ("InvLen: {0}, LenBin: {1}, Mod: {2}", inv.Length, len, this.Mod);
			byte[] res = new byte[len];
			int M = inv.Length / 4;
			this.permcenter = true;
			for (int i = 0, c = 0; i < len; i++) {
				int b = 0;
				for (int bit = 0; bit < 8; bit += 2,c++) {
					int C = c;
					if ((((int)(C / M)) % 3) > 0) {
						if (this.permcenter) {
							C += M;
						}
					}
					int m = inv[c] - C;
					if (Math.Abs (m) > this.MOD) {
						// Console.WriteLine ("XXX m: {0}", m);
						if (m < 0) {
							b |= (1 << bit);
						} else {
							b |= (2 << bit);
						}
					}
				}
				res[i] = (byte)b;
			}
			return res;
		}
	}
}
