//
//  Copyright 2013     Eric Sadit Tellez Avila
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

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{

	public class PivotSelector
	{
		int n;
		Random rand;
		HashSet<int> already_pivot;
		
		public PivotSelector(int n, Random rand)
		{
			this.n = n;
			this.rand = rand;
			this.already_pivot = new HashSet<int>();
		}
		
		public int NextPivot()
		{
			int piv;
			do {
				piv = this.rand.Next (0, this.n);
			} while (this.already_pivot.Contains(piv));
			this.already_pivot.Add (piv);
			return piv;
		}

		public static  void EstimateQueryStatistics(MetricDB DB, Random rand, int num_queries, int sample_size, out double mean, out double varY, out double qrad)
		{
			var n = DB.Count;
			var N = num_queries * sample_size;
			mean = 0.0;
			var square_mean = 0.0;
			qrad = 0;
			for (int qID = 0; qID < num_queries; ++qID) {
				var q = DB[ rand.Next(0, n) ];
				var min = double.MaxValue;
				for (int sampleID = 0; sampleID < sample_size; ++sampleID) {
					var u = DB[ rand.Next(0, n) ];
					var d = DB.Dist(q, u);
					mean += d / N;
					square_mean += d * d / N;
					if (d > 0) {
						min = Math.Min(min, d);
					}
				}
				qrad = Math.Max (min, qrad);
//				if (qrad == 0) {
//					qrad = min;
//				} else {
//					qrad = (min + qrad) * 0.5;
//				}
			}
			varY = square_mean - mean * mean;
		}
		
		public static void EstimatePivotStatistics(MetricDB DB, Random rand, object piv, int sample_size, out double mean, out double variance, out double qrad)
		{
			var n = DB.Count;
			mean = 0.0;
			var square_mean = 0.0;
			qrad = 0;
			var min = double.MaxValue;
			for (int sampleID = 0; sampleID < sample_size; ++sampleID) {
				var u = DB[ rand.Next(0, n) ];
				var d = DB.Dist(piv, u);
				mean += d / sample_size;
				square_mean += d * d / sample_size;
				if (d > 0) {
					min = Math.Min (min, d);
				}
			}
			// qrad = Math.Max (min, qrad);
			if (qrad == 0) {
				qrad = min;
			} else {
				qrad = (min + qrad) * 0.5;
			}
			variance = square_mean - mean * mean;
		}

	}
}