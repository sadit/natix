//  Copyright 2013 Eric Sadit Tellez Avila <donsadit@gmail.com>
// 
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.IO;
using System.Collections.Generic;
using natix;
using natix.CompactDS;
//using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class KnrFP : ILoadSave
	{
		public List<int[]> Fingerprints;
		public Index IdxRefs;
		public int K = 7;

		public KnrFP ()
		{
		}

		public object this[int objID] {
			get {
				return this.Fingerprints[objID];
			}
		}

		public object Parse(string raw)
		{
			var u = this.IdxRefs.DB.Parse (raw);
			return this.GetFP (u);
		}
		
		public void Load (BinaryReader Input)
		{
			this.IdxRefs = IndexGenericIO.Load (Input);
			var count = Input.ReadInt32 ();
			this.Fingerprints = new List<int[]> (count);
			for (int i = 0; i < count; ++i) {
				var len = Input.ReadInt32 ();
				var array = new int[len];
				this.Fingerprints.Add (array);
				PrimitiveIO<int>.LoadVector (Input, len, array);
			}
			this.K = Input.ReadInt32 ();
		}
		
		public void Save (BinaryWriter Output)
		{
			IndexGenericIO.Save (Output, this.IdxRefs);
			Output.Write (this.Fingerprints.Count);
			foreach (var vec in this.Fingerprints) {
				Output.Write (vec.Length);
				PrimitiveIO<int>.SaveVector (Output, vec);
			}
			Output.Write (this.K);
		}

		public static int[] GetFP(object a, Index refs, int k)
		{
			var knr = refs.SearchKNN (a, k);
			var aseq = new int[knr.Count];

			int i = 0;
			foreach (var p in knr) {
				aseq [i] = p.ObjID;
				++i;
			}
			return aseq;
		}

		public int[] GetFP(object a)
		{
			return GetFP (a, this.IdxRefs, this.K);
		}

		public virtual void Build(MetricDB original, Index refs, int k)
		{
			this.K = k;
			this.IdxRefs = refs;
			var n = original.Count;
			this.Fingerprints = new List<int[]> (n);
			for (int i = 0; i < n; ++i) {
				this.Fingerprints.Add (null);
			}
			var tasks = Environment.ProcessorCount << 3;
			int blocksize = n / tasks;
			int advance = 0;
			long minElapsedTicks = 20000000; // control the print rate
			long prevTicks = DateTime.Now.Ticks;
			long currTicks;
			var create_block = new Action<int> (delegate(int blockID) {
				var sp = blockID * blocksize;
				var ep = Math.Min (n, sp + blocksize);
				currTicks = DateTime.Now.Ticks;
				if (advance == 0 || currTicks - prevTicks > minElapsedTicks) {
					Console.WriteLine ("KnrFP {0}  ({1}/{2}), db: {3}, num_refs: {4}, K: {5}, timestamp: {6}",
					                   this, advance, n, Path.GetFileName(original.Name), this.IdxRefs.DB.Count, this.K, DateTime.Now);
					prevTicks = currTicks;
				}
				for (; sp < ep; ++sp) {
					var fp = this.GetFP(original[sp]);
					this.Fingerprints[sp] = fp;
					advance++;
				}
			});
			LongParallel.For (0, 1 + n / blocksize, create_block);
			Console.WriteLine ("done");
		}
	}
}


