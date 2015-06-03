//
//   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
	public class TMANNI : MANNIAbstract
	{
		public TMANNI ()
		{
		}

		public void Build (MetricDB db, int expected_k, int num_indexes, int num_tasks = -1)
		{
			// num_build_processors = 1;
			this.DB = db;
			var _rows = new ANNI[num_indexes];
			var pivsel = new PivotSelectorRandom(db.Count, RandomSets.GetRandom());
			var step_width = 512 / num_indexes + 8;

			LongParallel.For (0, num_indexes, (int i) => {
				_rows [i] = new ANNI ();
				_rows [i].Build (db, expected_k, step_width, num_indexes, pivsel);
			}, num_tasks);
//			ParallelOptions ops = new ParallelOptions ();
//			ops.MaxDegreeOfParallelism = num_processors;
//			Parallel.For (0, num_indexes, ops, (int i) => {
//				_rows [i] = new ILC ();
//				_rows [i].Build (db, num_indexes, pivsel);
//			});

			this.leader = new NANNI();
			this.leader.Build(_rows [0]);

			this.rows = new ANNI[num_indexes - 1];
			for (int i = 1; i < num_indexes; ++i) {
				this.rows[i - 1] = _rows[i];
			}
		}
	}
}
