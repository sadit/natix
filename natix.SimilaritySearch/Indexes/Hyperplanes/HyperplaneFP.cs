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
using System.Collections.Generic;
using natix;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	public class HyperplaneFP : BasicIndex
	{
		//public BinH8Space dbsignatures;
		public BinQ8HammingSpace Fingerprints;
		public int MaxCandidates=100000;
		public MetricDB Sample;
		Index internalIndex;

		public HyperplaneFP ()
		{
		}

		public byte[] GetFP(object a){
			int numsamplerefs = Sample.Count;
			int numpairs = numsamplerefs / 2;
			var row = new byte[(int)Math.Ceiling(numsamplerefs/16.0)];
			for(int i=0; i<numpairs; i++){
				var dist1 = this.DB.Dist(a,this.Sample[2*i]);
				var dist2 = this.DB.Dist(a,this.Sample[2*i+1]);
				
				if(dist1 <= dist2){
					BitAccess.SetBit(row,i);
				}
				else{
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

			int pc = this.DB.Count / 100 + 1;
			for(int i=0; i < this.DB.Count; i++){
				this.Fingerprints.Add( this.GetFP(this.DB[i]) );
				if (i % pc == 0) {
					Console.WriteLine ("Mapping object: ({0}/{1}), db: {2}, num_pairs: {3}",i,this.DB.Count, db.Name, num_pairs);
				}
			}
			var s = new Sequential ();
			s.Build (db);
			this.internalIndex = s;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			int maxC = this.MaxCandidates;
			if (maxC < 0) {
				maxC = this.DB.Count;
			}
			var cand = this.internalIndex.SearchKNN(q, maxC);
			foreach (var p in cand) {
				var d = this.DB.Dist(this.DB[p.docid], q);
				res.Push (p.docid, d);
			}
			return res;
		}
		
	}
}

