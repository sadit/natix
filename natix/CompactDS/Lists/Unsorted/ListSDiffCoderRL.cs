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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListSDiffCoderRL.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// List short integers coder R. Encodes only numbers bigger equal to 1.
	/// Consecutive sequences of 1's are priorized.
	/// </summary>
	public class ListSDiffCoderRL : ListSDiffCoder
	{	
		public ListSDiffCoderRL () : base()
		{
		}
	
		public ListSDiffCoderRL (ListSDiffCoder list)
		{
			this.Coder = list.Encoder;
			this.BlockSize = list.BlockSize;
			this.Build (list);
		}
		
		public ListSDiffCoderRL (IList<int> list, IIEncoder32 coder, short blocksize)
		{
			this.Coder = coder;
			this.BlockSize = blocksize;
			this.Build (list);
		}

		void Build (IList<int> list)
		{
			int run_len = 0;	
			foreach (var d in list) {
				if (d == 1) {
					// inside a run
					run_len++;
					if ((this.M + run_len) % this.BlockSize == 0) {
						this._AddRL (run_len);
						run_len = 0;
					}
				} else {
					if (run_len > 0) {
						// a run finished, flush the run and add the current d
						this._AddRL (run_len);
						run_len = 0;
					}
					this._Add (d);
				}
			}
			if (run_len > 0) {
				this._AddRL (run_len);
				run_len = 0;
			}
			// this.PrintDebug ();
		}
		
		void _AddRL (int run_len)
		{
			this.M += run_len;
			this.Coder.Encode (this.Stream, 1);
			this.Coder.Encode (this.Stream, run_len);
			if (this.M % this.BlockSize == 0) {
				this.Offsets.Add ((int)this.Stream.CountBits);
			}
		}

		void _Add (int u)
		{
			this.M++;
			Coder.Encode (this.Stream, u);
			if (this.M % this.BlockSize == 0) {
				this.Offsets.Add ((int)this.Stream.CountBits);
			}
		}
		
		public void PrintDebug ()
		{
			var ctx = new BitStreamCtx ();
			ctx.Seek (0);
			int i = 0;
			while (ctx.Offset < this.Stream.CountBits) {
				var c = this.Coder.Decode (this.Stream, ctx);
				Console.WriteLine ("=> i: {0}, c: {1}", i, c);
			}
		}
		
		public override void Add (int item)
		{
			throw new NotSupportedException ();
		}
		
		public override int GetItem (int index)
		{
			int run_len = 0;
			return this.GetItem (index, new BitStreamCtx (), ref run_len);
		}
		
		public int GetItem (int index, BitStreamCtx ctx, ref int run_len)
		{
			run_len = this.LocateAt (index - 1, ctx);
			return this.GetNext (ctx, ref run_len);
		}
		
		/// <summary>
		/// Locates ctx at index. Returns the remaining run_len value if any
		/// </summary>
		/// <returns>
		protected int LocateAt (int index, BitStreamCtx ctx)
		{
			if (index == -1) {
				ctx.Seek (0);
				return 0;
			}
			int offset_index = index / this.BlockSize;
			if (offset_index == 0) {
				ctx.Seek (0);
			} else {
				ctx.Seek (this.Offsets [offset_index - 1]);
			}
			int left = 1 + index - offset_index * this.BlockSize;
			int run_len = 0;
			for (int i = 0; i < left;) {
				if (run_len > 0) {
					// run_len--;
					i += run_len;
					run_len = 0;
					if (left < i) {
						run_len = i - left;
						i = left;
					}
				} else {
					var res = this.GetNext (ctx);
					if (res == 1) {
						run_len = this.GetNext (ctx) - 1;
					}
					i++;
				}
			}
			// Console.WriteLine ("** index: {0}, run_len: {1}, left: {2}", index, run_len, left);
			return run_len;
		}

		public override IEnumerable<int> ExtractFrom (int start, int count)
		{
			var ctx = new BitStreamCtx ();
			int run_len = this.LocateAt (start - 1, ctx);
			int v;
			if (run_len == 0) {
				v = this.GetNext (ctx);
				if (v == 1) {
					run_len = this.GetNext (ctx) - 1;
				}
			} else {
				v = 1;
				run_len--;
			}
			yield return v;
			for (int i = 1; i < count; i++) {
				if (run_len > 0) {
					run_len--;
					// v = 1;
				} else {
					v = this.GetNext (ctx);
					if (v == 1) {
						run_len = this.GetNext (ctx) - 1;
					}
				}
				yield return v;
			}
		}
		
		public int GetNext (BitStreamCtx ctx, ref int run_len)
		{
			if (run_len > 0) {
				run_len--;
				return 1;
			}
			var p = this.GetNext (ctx);
			if (p == 1) {
				run_len = this.GetNext (ctx) - 1;
			}
			return p;
		}

		public int Sum (int start, int count, BitStreamCtx ctx, ref int run_len)
		{
			run_len = this.LocateAt (start - 1, ctx);
			int sum = this.GetNext (ctx, ref run_len);
			count--;
			if (count > 0 && run_len > 0) {
				sum += run_len;
				count -= run_len;
				run_len = 0;
			}
			if (count < 0) {
				sum += count;
				run_len = -count;
				return sum;
			}
			while (count > 0) {
				int v = this.GetNext (ctx, ref run_len);
				sum += v;
				count--;
				if (count > 0 && v == 1) {
					count -= run_len;
					sum += run_len;
					if (count < 0) {
						sum += count;
						run_len = -count;
					} else {
						run_len = 0;
					}
				}
			}
			return sum;
		}
		
		/// <summary>
		/// Finds the sum or the insertion position of sum.
		/// </summary>
		/// <returns>
		public int FindSum (int start, int sum, BitStreamCtx ctx, ref int run_len)
		{
			int rank = 0;
			run_len = this.LocateAt (start - 1, ctx);
			int current_sum = this.GetNext (ctx, ref run_len);
			if (current_sum > sum) {
				return rank;
			}
			current_sum += run_len;
			rank += run_len;
			run_len = 0;
			if (current_sum > sum) {
				run_len = current_sum - sum;
				rank -= run_len;
				return rank;
			}
			while (current_sum < sum) {
				int next_diff = this.GetNext (ctx, ref run_len);
				current_sum += next_diff;
				if (current_sum > sum) {
					break;
				}
				rank++;
				if (current_sum == sum) {
					break;
				}
				current_sum += run_len;
				rank += run_len;
				run_len = 0;
				if (current_sum > sum) {
					run_len = current_sum - sum;
					rank -= run_len;
					break;
				}
			}
			return rank;
		}

	}
}