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
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class TMILC : TMILCAbstract
	{
		public TMILC ()
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

			this.leader = new TNILC ();
			this.leader.PartialBuild (db, pivsel);
			this.rows = new TILC[num_indexes - 1];

			for (int i = 0; i < this.rows.Length; ++i) {
				this.rows [i] = new TILC ();
				this.rows [i].PartialBuild (db, pivsel);
			}

			int step_width = 512 / num_indexes + 8;

			//int step_width = 128;
			long curr = long.MaxValue;
			long prev = 0L;
			int iter = 0;

			Console.WriteLine("xxxxxxxx BEGIN> db: {0}, step: {1}, indexes: {2}, k: {3}",
			                  Path.GetFileName(this.DB.Name), step_width, num_indexes, k);
			do {
				this.PromotePivots(step_width, pivsel);
				prev = curr;
				curr = DateTime.Now.Ticks;
				foreach (var qID in qlist) {
					var q = this.DB[qID];
					var res = new Result(k);
					this.SearchKNN(q, k, res);
				}
				curr = DateTime.Now.Ticks - curr;
				++iter;
				Console.WriteLine("xxxxxxxx> iter: {0}, current-search-time: {1}, timestamp: {2}",
				                  iter, TimeSpan.FromTicks(curr).TotalSeconds / (qlist.Count), DateTime.Now);
			} while (prev > curr * 1.001);
		}
	}
}

