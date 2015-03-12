//
//  Copyright 2014  Eric Sadit Tellez Avila
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
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class MILCv2 : MILCAbstract
	{
		public MILCv2 ()
		{
		}
		
		public virtual void Build (MetricDB db, int expected_k, int num_indexes, int num_tasks = -1)
		{
			// num_build_processors = 1;
			this.DB = db;
			--num_indexes;
			this.rows = new ILC[num_indexes];

			var pivsel = new PivotSelectorRandom(db.Count, RandomSets.GetRandom());

			this.leader = new NILC();
			var ilc = new ILC();
			var cost = ilc.InternalBuild (expected_k, 0, 1, db, 2, pivsel);
			this.leader.Build (ilc);
			int m = this.leader.clusters.Count;
			double review_prob = cost.SingleCost - m; review_prob /= this.DB.Count;

//			ParallelOptions ops = new ParallelOptions ();
//			ops.MaxDegreeOfParallelism = num_tasks;
//			Parallel.For (0, num_indexes, ops, (int i) => {
//				this.rows [i] = new ILC ();
//				this.rows [i].InternalBuild (m, review_prob, db, num_indexes, pivsel);
//			});

			Console.WriteLine ("====> num_indexes: {0}", num_indexes);
			LongParallel.For (0, num_indexes, (int i) => {
				this.rows [i] = new ILC ();
				this.rows [i].InternalBuild (expected_k, m, review_prob, db, num_indexes, pivsel);
			}, num_tasks);
		}
	}
}

