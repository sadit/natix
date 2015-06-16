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
	public class XMANNI : XMANNIAbstract
	{
		public XMANNI ()
		{
		}
		
		void PromotePivots(ANNISetup setup) {
			int step_width = setup.StepWidth / (this.rows.Length + 1);

			LongParallel.For (0, this.rows.Length + 1, (i) => {
				if (i == 0) {
					for (int s = 0; s < step_width; ++s) {
						int nextPivot = setup.Selector.NextPivot();
						this.leader.PromoteObjectToPivot(nextPivot);
					}
				} else {
					var idx = this.rows [i-1];
					for (int s = 0; s < step_width; ++s) {
						int nextPivot = setup.Selector.NextPivot();
						idx.PromoteObjectToPivot(nextPivot);
					}
				}
			});
		}

		public void Build (MetricDB db, ANNISetup setup, int num_indexes, bool optimizeDistances)
		{
			this.DB = db;
			var k = 1 + setup.ExpectedK; // since we use items from the database as training queries
		
			// select the queries to test the construction
			var qlist = RandomSets.GetRandomSubSet(setup.NumberQueries, this.DB.Count);
			this.leader = new XNANNI ();
			this.leader.PartialBuild (db, setup.Selector);
			this.rows = new TANNI[num_indexes - 1];

			for (int i = 0; i < this.rows.Length; ++i) {
				this.rows [i] = new TANNI ();
				this.rows [i].PartialBuild (db, setup.Selector);
			}

			//int step_width = 128;
			double currT = long.MaxValue;
			double prevT = 0;
			double currD = this.DB.Count;
			double prevD = 0;
			int iter = 0;
			Console.WriteLine("xxxxxxxx BEGIN> db: {0}, indexes: {1}, setup: {2}",
			                  Path.GetFileName(this.DB.Name), num_indexes, setup);
			do {
				this.PromotePivots(setup);
				prevT = currT;
				prevD = currD;
				currT = DateTime.Now.Ticks;
				foreach (var qID in qlist) {
					var q = this.DB[qID];
					var res = new Result(k);
					currD += this.InternalSearchKNN(q, k, res);
				}
				currT = DateTime.Now.Ticks - currT;
				currT /= qlist.Length;
				currD /= qlist.Length;
				++iter;

				Console.WriteLine ("======> iter: {0}, timestamp: {1}, setup: {2}", iter, DateTime.Now, setup);
				Console.WriteLine ("------> prevT: {0}, currT: {1}, prevT / currT: {2}", prevT, currT, prevT / currT);
				Console.WriteLine ("------> prevD: {0}, currD: {1}, prevD / currD: {2}", prevD, currD, prevD / currD);
				if (optimizeDistances) {
					if (prevD > currD * (1 + setup.AlphaStop)) {
						break;
					}
				} else {
					if (prevT > currT * (1 + setup.AlphaStop)) {
						break;
					}
				}
			} while (true);
		}
	}
}

