//
//  Copyright 2013  Francisco Santoyo, and Eric Sadit Tellez Avila
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
	public class HyperplaneFP : BasicIndex
	{
		//public BinH8Space dbsignatures;
		public BinQ8HammingSpace Fingerprints;
		public int MaxCandidates=100000;
		public MetricDB Sample;
		public Index InternalIndex;

		public HyperplaneFP ()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.Sample = SpaceGenericIO.SmartLoad (Input, false);
			this.InternalIndex = IndexGenericIO.Load (Input);
			this.Fingerprints = this.InternalIndex.DB as BinQ8HammingSpace;
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.Sample);
			IndexGenericIO.Save (Output, this.InternalIndex);
		}

		public byte[] GetFP(object a){
			int numsamplerefs = Sample.Count;
			int numpairs = numsamplerefs >> 1;
			var row = new byte[(int)Math.Ceiling(numsamplerefs/16.0)];
			for(int i = 0; i < numpairs; ++i){
				var c = i << 1;
				var dist1 = this.DB.Dist(a, this.Sample[c]);
				var dist2 = this.DB.Dist(a, this.Sample[c+1]);
				if (dist1 <= dist2){
					BitAccess.SetBit(row,i);
				} else {
					BitAccess.ResetBit(row,i);
				}
			}
			return row;
		}

		public virtual void Build(MetricDB db, int num_pairs, int maxCandidates = -1){
			this.DB = db;
			this.Fingerprints = new BinQ8HammingSpace (1); 
			this.Sample = new SampleSpace("", this.DB, num_pairs * 2);
			this.MaxCandidates = maxCandidates;
			var n = this.DB.Count;
			var A = new byte[n][];
			int pc = this.DB.Count / 100 + 1;
			int advance = 0;
			var create_one = new Action<int> (delegate(int i) {
				var fp = this.GetFP(this.DB[i]);
				A[i] = fp;
				if (advance % pc == 0) {
					Console.WriteLine ("DEBUG {0}  ({1}/{2}), db: {3}, num_pairs: {4}, timestamp: {5}", this, advance, n, db.Name, num_pairs, DateTime.Now);
				}
				advance++;
			});
			ParallelOptions ops = new ParallelOptions();
			ops.MaxDegreeOfParallelism = -1;
			Parallel.For (0, n, create_one);
			foreach (var fp in A) {
				this.Fingerprints.Add( fp );
			}
			var s = new Sequential ();
			s.Build (this.Fingerprints);
			this.InternalIndex = s;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			int maxC = this.MaxCandidates;
			if (maxC < 0) {
				maxC = this.DB.Count;
			}
			var cand = this.InternalIndex.SearchKNN(q, maxC);
			foreach (var p in cand) {
				var d = this.DB.Dist(this.DB[p.ObjID], q);
				res.Push (p.ObjID, d);
			}
			return res;
		}
	}
}

