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
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class KnrFP : MetricDB
	{
		public StringSpace<int> Fingerprints;
		public Index IdxRefs;
		public int K = 7;

		public KnrFP ()
		{
		}

		public string Name {
			get;
			set;
		}

		public int Count {
			get {
				return this.Fingerprints.Count;
			}
		}

		public int NumberDistances {
			get {
				return this.Fingerprints.NumberDistances;
			}
		}

		public double Dist(object a, object b)
		{
			return this.Fingerprints.Dist (a, b);
		}

		public object this[int objID] {
			get {
				return this.Fingerprints[objID];
			}
		}

		public object Parse(string raw, bool isquery)
		{
			var u = this.IdxRefs.DB.Parse (raw, isquery);
			return this.GetFP (u);
		}
		
		public void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			this.IdxRefs = IndexGenericIO.Load (Input);
			this.Fingerprints = SpaceGenericIO.SmartLoad (Input, false) as StringSpace<int>;
			this.K = Input.ReadInt32 ();
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write (this.Name);
			IndexGenericIO.Save (Output, this.IdxRefs);
			SpaceGenericIO.SmartSave (Output, this.Fingerprints);
			Output.Write (this.K);
		}

		public static int[] GetFP(object a, Index refs, int k)
		{
			var knr = refs.SearchKNN (a, k);
			var aseq = new int[knr.Count];

			int i = 0;
			foreach (var p in knr) {
				aseq [i] = p.docid;
				++i;
			}
			return aseq;
		}

		public int[] GetFP(object a)
		{
			return GetFP (a, this.IdxRefs, this.K);
		}

		public KnrFP (KnrFP inputDB, int new_n, int new_K = -1)
		{
			this.K = new_K;
			this.Fingerprints = new StringSpace<int> ();
			this.Fingerprints.seqs.Capacity = new_n;
			this.IdxRefs = inputDB.IdxRefs;
			if (new_K <= 0) {
				for (int i = 0; i < new_n; ++i) {
					var u = inputDB.Fingerprints.seqs [i];
					this.Fingerprints.Add (u);
				}
			} else {
				if (new_K > inputDB.Fingerprints.seqs [0].Length) {
					throw new ArgumentOutOfRangeException("new_K > old_K need a complete re-construction of the transformation");
				}
				for (int i = 0; i < new_n; ++i) {
					var u = inputDB.Fingerprints.seqs [i];
					var v = new int[new_K];
					for (int j = 0; j < new_K; ++j) {
						v [j] = u [j];
					}
					this.Fingerprints.Add (v);
				}
			}
		}

		public virtual void Build(MetricDB original, Index refs, int k)
		{
			this.K = k;
			this.Fingerprints = new StringSpace<int> ();
			this.Fingerprints.seqs.Capacity = original.Count;
			this.IdxRefs = refs;
			var n = original.Count;
			var A = new int[n][];
			int blocksize = 10000;
			int pc = original.Count / 100 + 1;
			int advance = 0;
			var create_block = new Action<int> (delegate(int blockID) {
				var sp = blockID * blocksize;
				var ep = Math.Min (n, sp + blocksize);
				for (; sp < ep; ++sp) {
					var fp = this.GetFP(original[sp]);
					A[sp] = fp;
					if (advance % pc == 0) {
						Console.WriteLine ("KnrFP {0}  ({1}/{2}), db: {3}, num_refs: {4}, timestamp: {5}", this, advance, n, original.Name, this.IdxRefs.DB.Count, DateTime.Now);
					}
					advance++;
				}
			});
			ParallelOptions ops = new ParallelOptions();
			ops.MaxDegreeOfParallelism = -1;
			Parallel.For (0, 1+n/blocksize, create_block);
			foreach (var fp in A) {
				this.Fingerprints.Add( fp );
			}
		}
	}
}


