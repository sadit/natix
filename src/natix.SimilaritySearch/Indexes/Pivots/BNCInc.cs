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
	public class BNCInc : PivotsAbstract
	{
		public BNCInc ()
		{
		}

		protected double Lmax(List<int> pivs, object p, object q)
		{
			var dmax = 0.0;
			foreach (var pivID in pivs) {
				var piv = this.DB [pivID];
				var dp = this.DB.Dist(piv, p);
				var dq = this.DB.Dist(piv, q);
				var dpart = Math.Abs (dp - dq);
	
				if (dpart > dmax) {
					dmax = dpart;
				}
			}
			return dmax;
		}

		protected double AvgDist(List<int> pivs, List<Tuple<int, int>> pairs)
		{
			double avg = 0.0;
			foreach (var tuple in pairs) {
				avg += this.Lmax (pivs, this.DB [tuple.Item1], this.DB [tuple.Item2]);
			}
			return avg / pairs.Count;
		}

		protected void AddPivot(int sampleSize, List<int> pivs, List<Tuple<int, int>> pairs)
		{
			var sample = RandomSets.GetRandomSubSet (sampleSize, this.DB.Count);
			int maxpiv = 0;
			double max = 0;
			pivs.Add (-1);
			foreach (var pivID in sample) {
				pivs [pivs.Count - 1] = pivID;
				var avg = this.AvgDist (pivs, pairs);
				if (avg > max) {
					max = avg;
					maxpiv = pivID;
				}
			}
			pivs [pivs.Count - 1] = maxpiv;
		}

		public void Build(MetricDB db, int numPivots, int NUMBER_TASKS=-1)
		{
			this.DB = db;
			List<Tuple<int, int>> pairs = new List<Tuple<int, int>> ();
			var sampleSize = (int)Math.Sqrt (this.DB.Count);
			var randset = RandomSets.GetRandomSubSet (2 * sampleSize, this.DB.Count);

			for (int i = 0; i < randset.Length; i+=2) {
				pairs.Add (new Tuple<int, int>(randset [i], randset [i + 1]));
			}				
			List<int> pivs = new List<int> ();

			for (int i = 0; i < numPivots; ++i) {
				if (i % 10 == 0) {
					Console.WriteLine ("**** computing pivot {0}/{1} {2}", i + 1, numPivots, DateTime.Now);
				}
				this.AddPivot (sampleSize, pivs, pairs);
			}
			this.Build (this.DB, new SampleSpace ("", this.DB, pivs), NUMBER_TASKS);
		}
	}
}
