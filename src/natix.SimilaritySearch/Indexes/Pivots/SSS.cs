//
//  Copyright 2014  Eric S. Tellez <donsadit@gmail.com>
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
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class SSS: PivotsAbstract
	{
		public SSS ()
		{

		}

		protected double EstimateMaxDistance(double prob)
		{
			var rand = RandomSets.GetRandom ();
			double max = 0;
			for (int i = 0; i < this.DB.Count; ++i) {
				var q = this.DB[i];
				if (rand.NextDouble() <= prob) {
					for (int uID = 0; uID < this.DB.Count; ++uID) {
						var u = this.DB [uID];
						var d = this.DB.Dist(q, u);
						if (d > max) {
							max = d;
						}
					}
				}
			}
			return max;
		}

		protected void AppendPivot(double alpha, double dmax, int objID, List<int> pivs)
		{
			double dmin = Double.MaxValue;
			var obj = this.DB [objID];
			for (int i = 0; i < pivs.Count; ++i) {
				var u = this.DB [pivs[i]];
				var d = this.DB.Dist (obj, u);
				if (d < dmin) {
					dmin = d;
				}
			}
			if (dmin / dmax < alpha) {
				return;
			}
			Console.WriteLine ("**** computing pivot alpha={0}, pivots={1}, {2}", alpha, pivs.Count, DateTime.Now);
			pivs.Add (objID);
		}

		public void Build(MetricDB db, double alpha, int max_pivots = int.MaxValue, int NUMBER_TASKS = -1)
		{
			this.DB = db;
			var dmax = this.EstimateMaxDistance (Math.Sqrt(this.DB.Count) / ((double)this.DB.Count));

			var pivs = new List<int> ();

			for (int i = 0; i < this.DB.Count && pivs.Count < max_pivots; ++i) {
				this.AppendPivot (alpha, dmax, i, pivs);
			}

			if (pivs.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty set of pivots");
			}
			this.Build (this.DB, new SampleSpace ("", this.DB, pivs), NUMBER_TASKS);
		}
	}
}
