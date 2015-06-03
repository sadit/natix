//
//   Copyright 2015 Eric S. Tellez <eric.tellez@infotec.com.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.IO;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class DMANNI : TMANNIAbstract
	{
		public DMANNI ()
		{
		}

		void PromotePivots(int step_width, PivotSelector pivsel) {
			LongParallel.For (0, this.rows.Length + 1, (i) => {
				if (i == 0) {
					for (int s = 0; s < step_width; ++s) {
						int nextPivot = pivsel.NextPivot();
						this.leader.PromoteObjectToPivot(nextPivot);
					}
				} else {
					var idx = this.rows [i-1];
					for (int s = 0; s < step_width; ++s) {
						int nextPivot = pivsel.NextPivot();
						idx.PromoteObjectToPivot(nextPivot);
					}
				}
			});
		}

		public void Build (MetricDB db, int k, int num_indexes)
		{
			this.DB = db;
			++k; // since we use items from the database as training queries

			PivotSelectorRandom pivsel = new PivotSelectorRandom (db.Count, RandomSets.GetRandom ());
		
			// select the queries to test the construction
			var qlist = new List<int>();
			for (int i = 0; i < 64; ++i) {
				qlist.Add(pivsel.NextPivot());
			}

			this.leader = new TNANNI ();
			this.leader.PartialBuild (db, pivsel);
			this.rows = new TANNI[num_indexes - 1];

			for (int i = 0; i < this.rows.Length; ++i) {
				this.rows [i] = new TANNI ();
				this.rows [i].PartialBuild (db, pivsel);
			}

			int step_width = 512 / num_indexes + 8;

//			if (num_indexes > 8) {
//				if (num_indexes < 16) {
//					step_width = (int)Math.Ceiling (128.0 / num_indexes) + 8;
//				} else if (num_indexes < 32) {
//					step_width = (int)Math.Ceiling (256.0 / num_indexes) + 8;
//				} else {
//					step_width = (int)Math.Ceiling (512.0 / num_indexes) + 8;
//				}
//			}
			// step_width = 128;

			long curr = long.MaxValue;
			long prev = 0L;
			int iter = 0;

			Console.WriteLine("xxxxxxxx BEGIN {0}> db: {1}, step: {2}, indexes: {3}, k: {4}",
			                  this, Path.GetFileName(this.DB.Name), step_width, num_indexes, k);
			do {
				this.PromotePivots(step_width, pivsel);
				prev = curr;
				curr = db.NumberDistances;
				foreach (var qID in qlist) {
					var q = this.DB[qID];
					var res = new Result(k);
					this.SearchKNN(q, k, res);
				}
				curr = db.NumberDistances - curr;
				++iter;
				Console.WriteLine("xxxxxxxx> iter: {0}, total-cost: {1}, timestamp: {2}",
				                  iter, curr / qlist.Count, DateTime.Now);
			} while (prev > curr * 1.01);
		}
	}
}
