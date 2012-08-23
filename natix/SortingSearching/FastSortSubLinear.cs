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
//   Original filename: natix/SortingSearching/FastSortSubLinear.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	/// <summary>
	/// Fast (integer MSD inplace raddix sort) sort
	/// Without recycling all possible memory and with unoptimized recursive calls.
	/// </summary>
	public class FastSortSubLinear<KeyType, ValueType>
	{
		protected short numbits;
		protected short msd;
		protected int[] startpos;
		protected int[] finalpos;
		protected long mask;
		protected float alpha;
		protected Func<KeyType, long> AsLong;

		public FastSortSubLinear (int numbits, int msd, Func<KeyType, long> asLong)
		{
			this.numbits = (short)numbits;
			this.msd = (short)msd;
			this.AsLong = asLong;
			
		}

		public void Reset (int numbits, int msd, bool new_arrays)
		{
			this.numbits = (short)numbits;
			this.msd = (short)msd;
			this.mask = this.Range - 1;
			if (new_arrays) {
				this.startpos = new int[this.Range];
				this.finalpos = new int[this.Range];
			}
		}

		public int Range {
			get { return 1 << msd; }
		}

		public void Clear ()
		{
			int range = this.Range;
			for (int i = 0; i < range; i++) {
				this.startpos[i] = 0;
				this.finalpos[i] = 0;
			}
		}

		public int GetId (long item)
		{
			var w = (item >> (this.numbits - msd));
			return (int)(w & this.mask);
		}

		void ComputeStats (IList<KeyType> A)
		{
			for (int i = 0; i < A.Count; i++) {
				this.finalpos[this.GetId (this.AsLong (A[i]))]++;
			}
			int range = this.Range;
			int acc = 0;
			for (int i = 0; i < range; i++) {
				this.startpos[i] = acc;
				acc += this.finalpos[i];
				this.finalpos[i] = acc;
			}
		}

		void Swap (IList<KeyType> Keys, IList<ValueType> Values, int a, int b)
		{
			KeyType v;
			v = Keys[a];
			Keys[a] = Keys[b];
			Keys[b] = v;
			if (Values != null) {
				ValueType u = Values[a];
				Values[a] = Values[b];
				Values[b] = u;
			}
		}

		void CountingSort (IList<KeyType> Keys, IList<ValueType> Values)
		{
			this.ComputeStats (Keys);
			int range = this.Range;
			for (int i = 0; i < range; i++) {
				while (this.startpos[i] < this.finalpos[i]) {
					int start = this.startpos[i];
					int follow_index = this.GetId (this.AsLong (Keys[start]));
					if (i != follow_index) {
						this.Swap (Keys, Values, this.startpos[follow_index], start);
					}
					this.startpos[follow_index]++;
				}
			}
		}

		public void Sort (IList<KeyType> Keys, IList<ValueType> Values)
		{
			// Console.WriteLine ("numbits: {0}, msd: {1}", this.numbits, this.msd);
			// int msd = (int)Math.Ceiling (numbits * this.alpha);
			int _msd = this.msd;
			this.msd = Math.Min (this.msd, this.numbits);
			this.Reset (numbits, this.msd, true);
			this.CountingSort (Keys, Values);
			if (this.msd < _msd) {
				return;
			}
			int[] counters = this.startpos;
			int prev = 0;
			for (int i = 0; i < counters.Length; i++) {
				counters[i] = this.finalpos[i] - prev;
				prev = this.finalpos[i];
			}
			int acc = 0;
			ListShiftIndex<KeyType> _Keys = Keys as ListShiftIndex<KeyType>;
			if (_Keys == null) {
				_Keys = new ListShiftIndex<KeyType> (Keys, 0, Keys.Count);
			}
			ListShiftIndex<ValueType> _Values = Values as ListShiftIndex<ValueType>;
			if (Values != null && _Values == null) {
				_Values = new ListShiftIndex<ValueType> (Values, 0, Values.Count);
			}
			for (int i = 0; i < counters.Length; i++) {
				int c = counters[i];
				if (c == 0) {
					continue;
				}
				// this.Clear ();
				ListShiftIndex<KeyType> K = new ListShiftIndex<KeyType> (_Keys.L, _Keys.startIndex + acc, c);
				var InnerSort = new FastSortSubLinear<KeyType, ValueType> (this.numbits - this.msd, this.msd, this.AsLong);
				if (Values == null) {
					InnerSort.Sort(K, null);
				} else {
					ListShiftIndex<ValueType> V = new ListShiftIndex<ValueType> (_Values.L, _Values.startIndex + acc, c);
					InnerSort.Sort (K, V);
				}
				acc += counters[i];
			}
		}
	}
}

