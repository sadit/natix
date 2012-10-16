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
	public class CompactPivotsTEXTRANS : BasicIndex
	{
		public IList<int> TEXT;
		public MetricDB PIVS;
		public IList<float> STDDEV;
		static int MAX_SYMBOL = 7;

		public CompactPivotsTEXTRANS () : base()
		{
		}
	
		public void Build(CompactPivots pivs)
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.PIVS = SpaceGenericIO.SmartLoad(Input, false);
			this.TEXT = ListIGenericIO.Load (Input);
			// this.MEAN = new float[this.PIVS.Count];
			this.STDDEV = new float[this.PIVS.Count];
			//PrimitiveIO<float>.ReadFromFile(Input, this.MEAN.Count, this.MEAN);
			PrimitiveIO<float>.ReadFromFile(Input, this.STDDEV.Count, this.STDDEV);
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.PIVS);
			ListIGenericIO.Save(Output, this.TEXT);
			PrimitiveIO<float>.WriteVector(Output, this.STDDEV);
		}
		
		public void Build (LAESA idx, int num_pivs,  ListIBuilder list_builder = null)
		{
			this.DB = idx.DB;
			var P = (idx.PIVS as SampleSpace);
			var S = new int[num_pivs];
			int n = this.DB.Count;
			this.STDDEV = new float[num_pivs];
			var L = new ListIFS(ListIFS.GetNumBits(MAX_SYMBOL));
			L.Add(0, n * num_pivs);
			for (int p = 0; p < num_pivs; ++p) {
				S [p] = P.SAMPLE [p];
				var D = new List<float>(idx.DIST[p]);
				this.ComputeStats(D, p);
				for (int i = 0; i < n; ++i) {
					var sym = this.Discretize(D[i], this.STDDEV[p]);
					L[num_pivs*i+p] = sym;
				}
				if (p % 10 == 0 || p + 1 == num_pivs) {
					Console.Write ("== advance: {0}/{1}, ", p, num_pivs);
					if (p % 100 == 0 || p + 1 == num_pivs) {
						Console.WriteLine ();
					}
				}
			}
			this.PIVS = new SampleSpace ("", P.DB, S);
			//var _L = new ListEqRL();
			//_L.Build(L, MAX_SYMBOL);
			if (list_builder == null) {
				this.TEXT = L;
			} else {
				this.TEXT = list_builder(L, MAX_SYMBOL);
			}
		}

		public virtual int Discretize (double d, float stddev)
		{
			var sym = d / stddev;
			if (sym < 0) {
				return 0;
			}
			if (sym > MAX_SYMBOL) {
				return MAX_SYMBOL;
			}
			return (int)sym;
		}

		protected void ComputeStats(IList<float> seq, int p)
		{
			float mean = 0;
			float stddev = 0;
			int n = seq.Count;
			for (int i = 0; i < n; ++i) {
				mean += seq[i];
			}
			mean = mean / n;
			for (int i = 0; i < n; ++i) {
				float x = seq[i] - mean;
				stddev += x * x;
			}
			stddev = (float)Math.Sqrt(stddev / n);
			// this.MEAN[piv_id] = mean;
			this.STDDEV[p] = stddev;
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var m = this.PIVS.Count;
			var n = this.DB.Count;
			var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
			var A = new HashSet<int>();
			var P = new float[ m ];
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
				var sp = i * m;
				bool review = true;
				for (int p = 0; p < m; ++p) {
					var sym = this.TEXT[sp + p];
					// Console.WriteLine ("i: {0}, sp: {1}, p: {2}, pos: {3}, sym: {4}", i, sp, p, sp + p, sym);
					var dqp = P[p];
					var stddev = this.STDDEV[p];
					var lower = this.Discretize(Math.Abs (dqp - res.CoveringRadius), stddev);
					var upper = this.Discretize(dqp + res.CoveringRadius, stddev);
					if (sym < lower || upper < sym ) {
						review = false;
						break;
					}
				}
				if (review) {
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
			var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
			var P = new float[ m ];
			var res = new Result(this.DB.Count, false);
			for (int p = 0; p < m; ++p) {
				var dqp = this.DB.Dist (q, this.PIVS [p]);
				var i = _PIVS[p];
				A.Add(i);
				P[p] = (float)dqp;
				res.Push (i, dqp);
			}
			for (int i = 0; i < n; ++i) {
				var sp = i * m;
				bool review = true;
				for (int p = 0; p < m; ++p) {
					var sym = this.TEXT[sp + p];
					var dqp = P[p];
					var stddev = this.STDDEV[p];
					var lower = this.Discretize(Math.Abs (dqp - radius), stddev);
					var upper = this.Discretize(dqp + radius, stddev);
					if (sym < lower || upper < sym ) {
						review = false;
						break;
					}
				}
				if (review) {
					var d = this.DB.Dist(this.DB[i], q);
					if (d <= radius) {
						res.Push(i, d);
					}
				}
			}
			return res;
		}
	}
}

