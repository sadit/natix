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
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class CompactPivotsLRANS : BasicIndex
	{
		public IList<int>[] DIST;
		public MetricDB PIVS;
		public IList<float> STDDEV;
		public IList<float> MEAN;
		/// <summary>
		/// The alpha_stddev. A positive value used to multiply the stddev in order to manipulate
		/// the number of rings
		/// </summary>
		public double alpha_stddev; 
		public int MAX_SYMBOL = 7;

		public CompactPivotsLRANS () : base()
		{
		}

		public virtual void Load_DIST(BinaryReader Input)
		{
			this.DIST = new IList<int>[this.PIVS.Count];
			for (int i = 0; i < this.PIVS.Count; ++i) {
				this.DIST[i] = ListIGenericIO.Load(Input);
			}
		}

		public virtual void Save_DIST (BinaryWriter Output)
		{
			for (int i = 0; i < this.PIVS.Count; ++i) {
				ListIGenericIO.Save(Output, this.DIST[i]);
			}
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.PIVS = SpaceGenericIO.SmartLoad(Input, false);
			this.Load_DIST(Input);
			// this.MEAN = new float[this.PIVS.Count];
			this.MAX_SYMBOL = Input.ReadInt32 ();
			this.alpha_stddev = Input.ReadSingle();
			//PrimitiveIO<float>.ReadFromFile(Input, this.MEAN.Count, this.MEAN);
			this.STDDEV = PrimitiveIO<float>.ReadFromFile(Input, this.PIVS.Count, null);
			this.MEAN = PrimitiveIO<float>.ReadFromFile(Input, this.PIVS.Count, null);
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.PIVS);
			this.Save_DIST(Output);
			// PrimitiveIO<float>.WriteVector(Output, this.MEAN);
			Output.Write((int) this.MAX_SYMBOL);
			Output.Write((float) this.alpha_stddev);
			PrimitiveIO<float>.WriteVector(Output, this.STDDEV);
			PrimitiveIO<float>.WriteVector(Output, this.MEAN);
		}

		public virtual void Build (LAESA idx, int num_pivs, int num_rings, ListIBuilder list_builder = null)
		{
			// setting up MAX_SYMBOL and alpha_stddev values
			{
				num_rings = Math.Max (8, num_rings);
				num_rings = 1 << ((int)Math.Ceiling (Math.Log (num_rings, 2)));
				this.MAX_SYMBOL = num_rings - 1;
				this.alpha_stddev = 8.0 / num_rings;
			}
			this.DB = idx.DB;
			var P = (idx.PIVS as SampleSpace);
			var S = new int[num_pivs];
			int n = this.DB.Count;
			this.STDDEV = new float[num_pivs];
			this.MEAN = new float[num_pivs];
			this.DIST = new IList<int>[num_pivs];
			int I = 0;
			Action<int> build_one_pivot = delegate(int p) {
				S [p] = P.SAMPLE [p];
				var D = new List<float>(idx.DIST[p]);
				this.ComputeStats(D, p);
				var stddev = this.STDDEV[p];
				var mean = this.MEAN[p];
				var L = new ListIFS(ListIFS.GetNumBits(this.MAX_SYMBOL));
				for (int i = 0; i < n; ++i) {
					var d = D[i];
					var sym = this.Discretize(d, stddev, mean);
					L.Add (sym);
				}
				if (list_builder == null) {
					this.DIST[p] = L;
				} else {
					this.DIST[p] = list_builder(L, this.MAX_SYMBOL);
				}
				if (I % 10 == 0 ) {
					Console.Write ("== advance: {0}/{1}, ", I, num_pivs);
					if (I % 50 == 0) {
						Console.WriteLine ();
					}
				}
				I++;
			};
			Parallel.For(0, num_pivs, build_one_pivot);
			this.PIVS = new SampleSpace ("", P.DB, S);
			Console.WriteLine ("=== done Build CompactPivotsLRANS");
		}

		public virtual int Discretize (double d, float stddev, float mean)
		{
			// we suppose a gaussian distribution
			//var sym = d / (stddev * this.alpha_stddev);
			var sym = (d - mean) / stddev;
			sym += 4;
			sym /= this.alpha_stddev;
			if (sym < 0) {
				return 0;
			}
			if (sym > this.MAX_SYMBOL) {
				return this.MAX_SYMBOL;
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
			this.MEAN[p] = mean;
			this.STDDEV[p] = stddev;
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
				for (int piv_id = 0; piv_id < m; ++piv_id) {
					var dqp = P[piv_id];
					var seq = this.DIST[piv_id];
					var sym = seq[i];
					var stddev = this.STDDEV[piv_id];
					var mean = this.MEAN[piv_id];
					var lower = this.Discretize(Math.Abs (dqp - res.CoveringRadius), stddev, mean);
					var upper = this.Discretize(dqp + res.CoveringRadius, stddev, mean);
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
				var seq = this.DIST[piv_id];
				if (A == null) {
					A = new HashSet<int>();
					for (int i = 0; i < n; ++i) {
						var sym = seq[i];
						var stddev = this.STDDEV[piv_id];
						var mean = this.MEAN[piv_id];
						var lower = this.Discretize(Math.Abs (dqp - radius), stddev, mean);
						var upper = this.Discretize(dqp + radius, stddev, mean);
						if (sym < lower || upper < sym ) {
							A.Add (i);
						}
					}
				} else {
					B = new HashSet<int>();
					foreach (var i in A) {
						var sym = seq[i];
						var stddev = this.STDDEV[piv_id];
						var mean = this.MEAN[piv_id];
						var lower = this.Discretize(Math.Abs (dqp - radius), stddev, mean);
						var upper = this.Discretize(dqp + radius, stddev, mean);
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

