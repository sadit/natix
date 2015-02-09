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
//   Original filename: natix/SimilaritySearch/Indexes/LSC_H8.cs
// 
// Added comments at 2014-09-23, by Eric
//
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NDesk.Options;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// LSH for Binary hamming space
	/// </summary>
	public class LSC_H8 : LSC
	{

		/// <summary>
		/// Compute the LSH hashes
		/// </summary>
		public override int ComputeHash (object _u)
		{
			IList<byte> u = (IList<byte>) _u;
			int hash = 0;
			for (int j = 0; j < this.H.Length; j++) {
				// j: position to sample
				// k: sample
				int k = this.H[j] % (u.Count << 3);
				// hash: the hash
				// u[k >> 3] access to the desired byte
				// k & 7 is the index inside the byte
				// So, (u[k >> 3] >> (k & 7)) access to the k-th bit in u
				// & 1 clears all other bits
				var w = (u[k >> 3] >> (k & 7)) & 1; 
				// aligns w in the target's j position, module 32 (hash has int32 type)
				w <<= j & 31;
				hash ^=  w; // combines w against the hash using xor
			}
			return hash;
		}
	}
}
