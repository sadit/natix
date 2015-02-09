//
//   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
	public class LSH_AcousticID : LSH
	{
		protected ushort[] H;

		public override void PreBuild(Random rand, object firstObject)
		{
			this.H = new ushort[this.Width];
			int[] u = firstObject as int[];
			var m = u.Length << 3;
			var sel = new PivotSelectorRandom (m, rand);
		
			for (int i = 0; i < this.Width; ++i) {
				var p = sel.NextPivot ();
				this.H [i] = (ushort)p;
			}
			Array.Sort (this.H);
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.H = new ushort[this.Width];
			PrimitiveIO<ushort>.LoadVector(Input, this.Width, this.H);

		}

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			PrimitiveIO<ushort>.SaveVector(Output, this.H);
		}

		/// <summary>
		/// Compute the LSH hashes
		/// </summary>
		public override int ComputeHash (object _u)
		{
			int[] u = (int[]) _u;
			int hash = 0;
			for (int j = 0; j < this.H.Length; j++) {
				// j: position to sample
				// k: sample
				int k = this.H[j] % (u.Length << 5);
				// hash: the hash
				hash ^= ((u[k >> 5] >> (k & 31)) & 1) << (j & 31);
			}

			return hash;
		}
	}
}
