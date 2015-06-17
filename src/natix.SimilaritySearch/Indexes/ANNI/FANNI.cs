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
	public class FANNI : MANNIAbstract
	{
		public FANNI ()
		{
		}
		
		public virtual void Build (MetricDB db, ANNISetup setup)
		{
			// num_build_processors = 1;
			this.DB = db;
			var rows = new List<ANNI> ();

			var pivsel = new PivotSelectorRandom(db.Count, RandomSets.GetRandom());

			this.leader = new NANNI();
			var ilc = new ANNI();
			var cost = ilc.InternalBuild (setup, 0, 1.0, db, 2);
			this.leader.Build (ilc);
			int m = this.leader.clusters.Count;
			double review_prob = cost.SingleCost - m; review_prob /= this.DB.Count;
			var min_prob = Math.Sqrt (this.DB.Count) / this.DB.Count;

			while (review_prob > min_prob) {
				var row = new ANNI ();
				rows.Add (row);
				var _cost = row.InternalBuild (setup, m, review_prob, db, 2);
				var _m = row.ACT.Count;
				review_prob *= (_cost.SingleCost - _m) / this.DB.Count;
			}
			this.rows = rows.ToArray ();
		}
	}
}

