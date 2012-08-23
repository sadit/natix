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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListRL2.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// List RL 2. Encodes an array using a compact representation. Specially useful for lists that
	/// exhibit large runs. Not supports consecutive equal items.
	/// </summary>
	public class ListRL2 : ListGenerator<int>, ILoadSave
	{
		protected ListIFS headers;
		protected IRankSelect lens;
		protected ListSDiffCoderRL diffs;
		
		public ListIFS Headers {
			get {
				return this.headers;
			}
		}
				
		public IRankSelect Lens {
			get {
				return this.lens;
			}
		}
		
		public ListSDiffCoderRL Diffs {
			get {
				return this.diffs;
			}
		}
		
		public ListRL2 ()
		{
			this.BitmapBuilder = BitmapBuilders.GetGGMN_wt(12);
		}

		public BitmapFromBitStream BitmapBuilder {
			get;
			set;
		}
		
		public int BlockSize {
			get {
				return this.diffs.BlockSize;
			}
		}
		public override int Count {
			get {
				return this.lens.Count;
			}
		}
		
		public virtual void Build (IList<int> list, int maxvalue)
		{
			this.Build (list, maxvalue, new EliasDelta());
		}

		public virtual void Build (IList<int> list, int maxvalue, IIEncoder32 icoder, short blocksize = 63)
		{
			int n = list.Count;
			var B_lens = new BitStream32 ();
			var _diffs = new ListSDiffCoder (icoder, blocksize);
			// var _diffs = new ListSDiffCoder ();
			this.headers = new ListIFS ((int)Math.Ceiling (Math.Log (maxvalue + 1, 2)));
			if (list.Count > 0) {
				this.headers.Add (list [0]);
				B_lens.Write (true);
				for (int i = 1; i < n; i++) {
					var d = list [i] - list [i - 1];
					if (d == 0) {
						throw new ArgumentException ("ListRL2 doesn't support equal consecutive items");
					}
					if (d > 0) {
						B_lens.Write (false);
						_diffs.Add (d);
					} else {
						// Console.WriteLine ("XXXX list[{0}] = {1}, list[i-1] = {2}", i, list [i], list [i - 1]);
						this.headers.Add (list [i]);
						B_lens.Write (true);
					}
				}
			}
			var bb = new FakeBitmap ();
			bb.B = B_lens;
			this.lens = this.BitmapBuilder (bb);
			this.diffs = new ListSDiffCoderRL (_diffs, icoder, blocksize);
			/*Console.WriteLine ("******************> ");
			foreach (var u in _diffs) {
				Console.Write (u.ToString () + ", ");
			}
			Console.WriteLine ();
			foreach (var u in this.diffs) {
				Console.Write (u.ToString () + ", ");
			}
			Console.WriteLine ();
			// this.diffs.PrintDebug ();
			Console.WriteLine ("*****************> END BUILD");*/
		}
		
		/*public int FindSumInSortedRange (int sum, int start_pos, int end_pos)
		{
			var sp = this.lens.Rank1 (start_pos);
			var ep = this.lens.Rank1 (end_pos);
			var hp = GenericSearch.FindFirst<int> (sum, this.headers, sp, ep);
			if (hp < 0) {
				return this.diffs.FindSum (final_sp, end_pos); 
			}
		}*/

		public virtual void Load (BinaryReader Input)
		{
			var L = new ListIFS ();
			L.Load (Input);
			this.headers = L;
			this.lens = RankSelectGenericIO.Load (Input);
			this.diffs = new ListSDiffCoderRL ();
			this.diffs.Load (Input); // = RankSelectGenericIO.Load (Input);
		}

		public virtual void Save (BinaryWriter Output)
		{
			this.headers.Save (Output);
			RankSelectGenericIO.Save (Output, this.lens);
			//RankSelectGenericIO.Save (Output, this.diffs);
			this.diffs.Save (Output);
		}
						
		public override int GetItem (int index)
		{
			int rl = 0;
			return this.GetItem (index, new BitStreamCtx (), ref rl);
		}

		public int GetItem (int index, BitStreamCtx ctx, ref int run_len)
		{
			if (index >= this.Count) {
				throw new IndexOutOfRangeException ();
			}
			var rank1 = this.lens.Rank1 (index);
			var pos = this.lens.Select1 (rank1);
			var rank0 = pos - rank1 + 1;
			var count = index - pos;
			var v = this.headers [rank1 - 1];
			// TODO: replace ExtractFrom for SumFromTo
			// TODO: Replace diffs for RL diffset, add DiffSetRL like compressions and hacks
			// Console.WriteLine ("**** pos: {0}, count: {1}, rank0: {2}, rank1: {3}, head: {4}, index: {5}",
			//                    pos, count, rank0, rank1, v, index);
			if (count > 0) {
				// Console.WriteLine ("xxxx rank0: {0}, count: {1}, head: {2}", rank0, count, v);
				/*foreach (var u in this.diffs.ExtractFrom(rank0, count)) {
					v += u;
				}*/
				v += this.diffs.Sum (rank0, count, ctx, ref run_len);
			} else {
				run_len = 0;
			}
			return v;
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}	
	}
}
