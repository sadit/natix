// //
// //   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
// //
// //   Licensed under the Apache License, Version 2.0 (the "License");
// //   you may not use this file except in compliance with the License.
// //   You may obtain a copy of the License at
// //
// //       http://www.apache.org/licenses/LICENSE-2.0
// //
// //   Unless required by applicable law or agreed to in writing, software
// //   distributed under the License is distributed on an "AS IS" BASIS,
// //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //   See the License for the specific language governing permissions and
// //   limitations under the License.
using System;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class PivotSelectorSSS : PivotSelector
	{
		public List<int> pivs;
		public int curr;
		public Random rand;
		public MetricDB db;
		public double alpha;
		public double dmax;

		public PivotSelectorSSS ()
		{
		}

		protected double EstimateMaxDistance(MetricDB db, double prob)
		{
			var rand = RandomSets.GetRandom ();
			double max = 0;

			for (int i = 0; i < db.Count; ++i) {
				var q = db[i];
				if (rand.NextDouble() <= prob) {
					for (int uID = 0; uID < db.Count; ++uID) {
						var u = db [uID];
						var d = db.Dist(q, u);
						if (d > max) {
							max = d;
						}
					}
				}
			}
			return max;
		}

		protected void AppendPivot(MetricDB db, double alpha, double dmax, int objID)
		{
			double dmin = Double.MaxValue;
			var obj = db [objID];
			for (int i = 0; i < pivs.Count; ++i) {
				var u = db [pivs[i]];
				var d = db.Dist (obj, u);
				if (d < dmin) {
					dmin = d;
				}
			}
			if (dmin / dmax < alpha) {
				return;
			}
			Console.WriteLine ("**** computing pivot alpha={0}, pivots={1}, {2}", alpha, pivs.Count, DateTime.Now);
			this.pivs.Add (objID);
		}

		public PivotSelectorSSS(MetricDB db, double alpha = 0.4, Random rand = null)
		{
			this.db = db;
			this.alpha = alpha;
			var n = db.Count;
			this.dmax = this.EstimateMaxDistance (db, Math.Sqrt(n) / (double)n);
			if (rand == null) {
				rand = new Random ();
			}
			this.rand = rand;
			this.Reset ();
		}

		public void Reset()
		{
			this.pivs = new List<int> ();
			this.curr = 0;

			foreach (var i in RandomSets.GetRandomPermutation(this.db.Count, rand)){
				this.AppendPivot (db, alpha, dmax, i); 
			}
		}

		public override int NextPivot ()
		{
			lock (this) {
				if (this.curr >= this.pivs.Count) {
					this.Reset ();
				}
				int piv = this.pivs [this.curr];
				++this.curr;
				return piv;
			}
		}
	}
}

