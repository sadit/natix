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
//
using System;
using System.IO;
using System.Collections.Generic;
using natix;
using natix.CompactDS;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class HFP : MetricDB
	{
		public BinQ8HammingSpace Fingerprints;
		public MetricDB Pairs;
		
		public HFP ()
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
			var u = this.Pairs.Parse (raw, isquery);
			return this.GetFP (u);
		}
		
		public void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			this.Pairs = SpaceGenericIO.SmartLoad (Input, false);
			this.Fingerprints = SpaceGenericIO.SmartLoad(Input, false) as BinQ8HammingSpace;
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write (this.Name);
			SpaceGenericIO.SmartSave (Output, this.Pairs);
			SpaceGenericIO.SmartSave (Output, this.Fingerprints);
		}
		
		public byte[] GetFP(object a)
		{
			int numpairs = this.Pairs.Count >> 1;
			var row = new byte[(int)Math.Ceiling(numpairs/8.0)];
			for (int i = 0; i < numpairs; ++i){
				var c = i << 1;
				var dist1 = this.Pairs.Dist(a, this.Pairs[c]);
				var dist2 = this.Pairs.Dist(a, this.Pairs[c+1]);
				if (dist1 <= dist2){
					BitAccess.SetBit(row,i);
				} else {
					BitAccess.ResetBit(row,i);
				}
			}
			return row;
		}
		
		public virtual void Build(MetricDB original, MetricDB pairs)
		{
			this.Fingerprints = new BinQ8HammingSpace (1);
			this.Pairs = pairs;
			var n = original.Count;
			var A = new byte[n][];
			int blocksize = 1000;
			int pc = original.Count / 100 + 1;
			int advance = 0;
			var create_block = new Action<int> (delegate(int blockID) {
				var sp = blockID * blocksize;
				var ep = Math.Min (n, sp + blocksize);
				for (; sp < ep; ++sp) {
					var fp = this.GetFP(original[sp]);
					A[sp] = fp;
					if (advance % pc == 0) {
						Console.WriteLine ("DEBUG {0}  ({1}/{2}), db: {3}, num_pairs: {4}, timestamp: {5}", this, advance, n, original.Name, this.Pairs.Count/2, DateTime.Now);
					}
					advance++;
				}
			});
			ParallelOptions ops = new ParallelOptions();
			ops.MaxDegreeOfParallelism = 1;
			Parallel.For (0, 1 + n/blocksize, create_block);
			foreach (var fp in A) {
				this.Fingerprints.Add( fp );
			}
		}
	}
}


