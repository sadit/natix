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
//   Original filename: natix/CompactDS/Bitmaps/DiffSetRL.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class DiffSetRL : DiffSet
	{
		int run_len = 0;
		public DiffSetRL () : base()
		{
		}

		public DiffSetRL (short B) : base(B)
		{
		}

		public override void Add (int current, int prev)
		{
			this.AddItem (current, prev);
			// removing the this.Commit() call
		}

		protected override int ReadNext (BitStreamCtx ctx)
		{
			if (run_len > 0) {
				run_len--;
				return 1;
			}
			int d = base.ReadNext (ctx);
			if (d == 1) {
				run_len = base.ReadNext (ctx) - 1;
			}
			return d;
		}
		
		
		protected override void WriteNewDiff (int u)
		{
			if (u == 1) {
				run_len++;
			} else {
				this.Commit ();
				base.WriteNewDiff (u);
			}
		}
		
		protected override void ResetReader ()
		{
			this.run_len = 0;
		}

		
		//static double global_avg_filled = 0;
		//static int global_num_lists = 0;
		//int filled = 0;
		public override void Commit ()
		{
			if (run_len == 0) {
				return;
			}
			//if (run_len == this.B) {
			//	filled++;
			//}
			base.WriteNewDiff (1);
			base.WriteNewDiff (run_len);
			run_len = 0;
		}
		
		public override void Save (BinaryWriter bw)
		{
			this.Commit ();
			//global_num_lists++;
			//int num_blocks = this.Count1 / this.B;
			//double avg_filled = filled * 100.0 / num_blocks;
			//global_avg_filled += avg_filled;
			//Console.WriteLine ("ooooo> filled: {0}, num-blocks: {1}, ratio: {2}, avg-ratio: {3} ",
			//	filled, num_blocks, filled * 100.0 / num_blocks, global_avg_filled/global_num_lists);
			base.Save (bw);
		}
	}
}