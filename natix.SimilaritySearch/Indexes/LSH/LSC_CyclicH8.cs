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
//   Original filename: natix/SimilaritySearch/Indexes/LSC_CyclicH8.cs
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
	public class LSC_CyclicH8 : LSC
	{
		public int max_len;


		public override void Build (MetricDB db, int sampleSize, SequenceBuilder seq_builder = null, Func<int, object> get_item = null)
		{
			this.max_len = 0;
			for (int i = 0; i < db.Count; i++) {
				var u = (IList<byte>)db[i];
				this.max_len = Math.Max (this.max_len, u.Count);
			}
			base.Build(db, sampleSize, seq_builder, get_item);
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.max_len = Input.ReadInt32();
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write((int)this.max_len);
		}


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
				int byte_id = k >> 3;
				byte_id = byte_id % u.Count;
				// hash: the hash
				hash ^= ((u[byte_id] >> (k & 7)) & 1) << (j & 31);
			}
			return hash;
		}
	}
}
