//
//  Copyright 2012  Eric Sadit Tellez Avila
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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class LAESA : BasicIndex
	{
		public IList<float>[] DIST;
		public MetricDB PIVS;

		public LAESA ()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.PIVS = SpaceGenericIO.SmartLoad(Input, false);
			this.DIST = new IList<float>[this.PIVS.Count];
			for (int i = 0; i < this.PIVS.Count; ++i) {
				this.DIST[i] = PrimitiveIO<float>.ReadFromFile(Input, this.DB.Count, null);
			}
			// this.MEAN = new float[this.PIVS.Count];
			// this.STDDEV = new float[this.PIVS.Count];
			//PrimitiveIO<float>.ReadFromFile(Input, this.MEAN.Count, this.MEAN);
			// PrimitiveIO<float>.ReadFromFile(Input, this.STDDEV.Count, this.STDDEV);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.PIVS);
			for (int i = 0; i < this.PIVS.Count; ++i) {
				PrimitiveIO<float>.WriteVector(Output, this.DIST[i]);
			}
		}

		public void Build (LAESA idx, int num_pivs)
		{
			this.DB = idx.DB;
			var P = (idx.PIVS as SampleSpace);
			var S = new int[num_pivs];
			this.DIST = new IList<float>[num_pivs];
			for (int i = 0; i < num_pivs; ++i) {
				S [i] = P.SAMPLE [i];
				this.DIST[i] = idx.DIST[i];
			}
			this.PIVS = new SampleSpace("", P.DB, S);
		}

		public void Build (MetricDB db, int num_pivs)
		{
			this.DB = db;
			this.PIVS = new SampleSpace("", db, num_pivs);
			var n = db.Count;
			this.DIST = new IList<float>[num_pivs];
			for (int i = 0; i < num_pivs; ++i) {
				var seq = new float[n];
				for (int j = 0; j < n; ++j) {
					var d = this.DB.Dist (this.PIVS[i], this.DB [j]);
					seq[j]  = (float)d;
					// Console.WriteLine ("=======> d {0}", d);
				}
				this.DIST[i] = seq;
				if (i % 10 == 0) {
					Console.WriteLine ("XXX advance: {0}/{1}", i+1, num_pivs);
				}
			}
		}


		public override IResult SearchKNN (object q, int K, IResult res)
		{		
			var m = this.PIVS.Count;
			var n = this.DB.Count;
			var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
			var A = new HashSet<int>();
			var P = new float[m];
			for (int piv_id = 0; piv_id < m; ++piv_id) {
				var i = _PIVS[piv_id];
				var d = this.DB.Dist(q, this.DB[i]);
				P[piv_id] = (float)d;
				res.Push(i, d);
				A.Add(i);
			}
			// todo: randomize
			for (int i = 0; i < n; ++i) {
				if (A.Contains(i)) {
					continue;
				}
				bool check_object = true;
				for (int piv_id = 0; piv_id < m; ++piv_id) {
					var dqp = P[piv_id];
					var dpu = this.DIST[piv_id][i];
					if (Math.Abs (dqp - dpu) > res.CoveringRadius) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					res.Push(i, this.DB.Dist(q, this.DB[i]));
				}
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
			var m = this.PIVS.Count;
			var n = this.DB.Count;
			HashSet<int> A = null;
			HashSet<int> B = null;
			for (int piv_id = 0; piv_id < m; ++piv_id) {
				var dqp = this.DB.Dist (q, this.PIVS [piv_id]);
				if (A == null) {
					A = new HashSet<int>();
					for (int i = 0; i < n; ++i) {
						var dpu = this.DIST[piv_id][i];
						if (Math.Abs (dqp - dpu) <= radius) {
							A.Add(i);
						}
					}
				} else {
					B = new HashSet<int>();
					foreach (var i in A) {
						var dpu = this.DIST[piv_id][i];
						if (Math.Abs (dqp - dpu) <= radius) {
							B.Add(i);
						}
					}
					A = B;
				}
			}
			var res = new Result(this.DB.Count, false);
			foreach (var docid in A) {
				var d = this.DB.Dist(this.DB[docid], q);
				if (d <= radius) {
					res.Push(docid, d);
				}
			}
			return res;
		}
	}
}

