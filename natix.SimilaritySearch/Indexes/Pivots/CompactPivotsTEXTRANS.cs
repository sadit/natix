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
	public class CompactPivotsTLRANS : BasicIndex
	{
		public IList<int> TEXT;
		public MetricDB PIVS;
		public IList<float> STDDEV;
		public int SEARCHPIVS;

		public CompactPivotsTLRANS () : base()
		{
		}
	
		public void Build(CompactPivots pivs)
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.SEARCHPIVS = Input.ReadInt32 ();
			this.PIVS = SpaceGenericIO.SmartLoad(Input, false);
			this.TEXT = ListIGenericIO.Load (Input);
			this.TSEQ = new IList<int>[this.DB.Count];
			for (int i = 0; i < this.TSEQ.Length; ++i) {
				this.TSEQ[i] = ListIGenericIO.Load(Input);
			}
			// this.MEAN = new float[this.PIVS.Count];
			this.STDDEV = new float[this.PIVS.Count];
			//PrimitiveIO<float>.ReadFromFile(Input, this.MEAN.Count, this.MEAN);
			PrimitiveIO<float>.ReadFromFile(Input, this.STDDEV.Count, this.STDDEV);
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write((int)this.SEARCHPIVS);
			SpaceGenericIO.SmartSave (Output, this.PIVS);
			for (int i = 0; i < this.TSEQ.Length; ++i) {
				ListIGenericIO.Save(Output, this.TSEQ[i]);
			}
			// PrimitiveIO<float>.WriteVector(Output, this.MEAN);
			PrimitiveIO<float>.WriteVector(Output, this.STDDEV);
		}
		
		public void Build (CompactPivots idx, int num_pivs, int search_pivs, SequenceBuilder seq_build = null)
		{
			this.DB = idx.DB;
			var P = (idx.PIVS as SampleSpace);
			var S = new int[num_pivs];
			int n = this.DB.Count;
			this.SEARCHPIVS = search_pivs;
			this.STDDEV = new float[num_pivs];
			this.TSEQ = new IList<int>[n];
			var TMP_SEQ = new IList<int>[num_pivs];
			var MAX_SEQ = new short[n];
			var H = new int[16];
			for (int i = 0; i < num_pivs; ++i) {
				var seq = idx.SEQ [i];
				S [i] = P.SAMPLE [i];
				this.STDDEV [i] = idx.STDDEV [i];
				var L = new ListIFS (ListIFS.GetNumBits (seq.Sigma - 1));
				L.Add (0, n);
				var sigma = seq.Sigma;
				for (int s = 0; s < sigma; ++s) {
					var rs = seq.Unravel (s);
					var count = rs.Count1;
					for (int j = 1; j <= count; ++j) {
						var pos = rs.Select1 (j);
						L [pos] = s;
						MAX_SEQ [pos] = (short)Math.Max (s, MAX_SEQ [pos]);
						H[s]++;
					}
				}
				TMP_SEQ [i] = L;
			}
			this.PIVS = new SampleSpace ("", P.DB, S);
			for (int i = 0; i < n; ++i) {
				var L = new ListIFS(ListIFS.GetNumBits(MAX_SEQ[i]));
				for (int p = 0; p < num_pivs; ++p) {
					L.Add(TMP_SEQ[p][i]);
				}
				this.TSEQ[i] = L;
			}
		}

		public virtual int Discretize (double d, float stddev)
		{
			var sym = d / stddev;
			if (sym < 0) {
				return 0;
			}
			return (int)sym;
		}

		protected void ComputeStats(float[] seq, int piv_id)
		{
			float mean = 0;
			float stddev = 0;
			for (int i = 0; i < seq.Length; ++i) {
				mean += seq[i];
			}
			mean = mean / seq.Length;
			for (int i = 0; i < seq.Length; ++i) {
				float x = seq[i] - mean;
				stddev += x * x;
			}
			stddev = (float)Math.Sqrt(stddev / seq.Length);
			// this.MEAN[piv_id] = mean;
			this.STDDEV[piv_id] = stddev;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var m = this.PIVS.Count;
			var n = this.DB.Count;
			var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
			var P = new float[ m ];
			var A = new HashSet<int>();
			for (int piv_id = 0; piv_id < m; ++piv_id) {
				var dqp = this.DB.Dist (q, this.PIVS [piv_id]);
				var i = _PIVS[piv_id];
				A.Add(i);
				P[piv_id] = (float)dqp;
				res.Push (i, dqp);
			}
			for (int i = 0; i < n; ++i) {
				if (A.Contains(i)) {
					continue;
				}
				bool check_object = true;
				var S = this.TSEQ[i];
				for (int piv_id = 0; piv_id < m; ++piv_id) {
					var dqp = P[piv_id];
					var sym = S[piv_id];
					var stddev = this.STDDEV[piv_id];
					var lower = this.Discretize(dqp - res.CoveringRadius, stddev);
					var upper = this.Discretize(dqp + res.CoveringRadius, stddev);
					if (sym < lower || upper < sym ) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					// Console.WriteLine ("CHECKING i: {0}, m: {1}, n: {2}", i, m, n);
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
						var sym = this.TSEQ[i][piv_id];
						var stddev = this.STDDEV[piv_id];
						var lower = this.Discretize(dqp - radius, stddev);
						var upper = this.Discretize(dqp + radius, stddev);
						if (sym < lower || upper < sym ) {
							A.Add (i);
						}
					}
				} else {
					B = new HashSet<int>();
					foreach (var i in A) {
						var sym = this.TSEQ[i][piv_id];
						var stddev = this.STDDEV[piv_id];
						var lower = this.Discretize(dqp - radius, stddev);
						var upper = this.Discretize(dqp + radius, stddev);
						if (sym < lower || upper < sym ) {
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

