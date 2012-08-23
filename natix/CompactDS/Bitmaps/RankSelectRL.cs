//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
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
//
//   Original filename: natix/CompactDS/Bitmaps/RankSelectRL.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class RankSelectRL : PlainSortedList
	{
		public override void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.N);
			var list_rl = new ListRL ();
			list_rl.Build (this.sortedList, this.N - 1);
			list_rl.Save (Output);
		}

		public override void Load (BinaryReader Input)
		{
			this.N = Input.ReadInt32 ();
			var list_rl = new ListRL ();
			list_rl.Load (Input);
			this.sortedList = list_rl;
		}

		public RankSelectRL () : base()
		{
		}
		
		public override int Rank1 (int pos)
		{
			if (pos < 0 || this.sortedList.Count < 1) {
				return 0;
			}
			var list_rl = (this.sortedList as  ListRL);
			if (list_rl == null) {
				// Not load-save process was performed (necessary to work on RL encoded integers)
				return 1 + GenericSearch.FindLast<int> (pos, this.sortedList);
			} else {
				var run_id = GenericSearch.FindLast<int> (pos, list_rl.Headers);
				if (run_id < 0) {
					return 0;
				}
				int run_size;
				pos -= list_rl.Headers [run_id];
				var runs = list_rl.Runs;
				var rank = 1 + runs.Select1 (run_id + 1);
				if (run_id + 1 == list_rl.Headers.Count) {
					run_size = runs.Count - rank;
				} else {
					run_size = list_rl.Runs.Select1 (run_id + 2) - rank;
				}
				return rank + Math.Min (run_size, pos);
			}
		}
	}
}

