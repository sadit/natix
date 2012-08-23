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
//   Original filename: natix/SortingSearching/FastSortSqrt.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class FastSortSqrt<KeyType, ValueType>
	{
		protected short numbits;
		protected short msd;
		protected int[] startpos;
		protected int[] finalpos;
		protected long mask;
		protected Func<KeyType,long> AsLong;
			
		public FastSortSqrt (int numbits, Func<KeyType, long> asLong)
		{
			this.numbits = (short)numbits;
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
			get {
				return 1 << msd;
			}
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
				this.finalpos[this.GetId (this.AsLong(A[i]))]++;
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
			//try {
				v = Keys[a];
			//} catch (ArgumentOutOfRangeException e) {
			//	Console.WriteLine ("XXXXXXX a: {0}, b: {1}", a, b);
			//	throw e;
			//}
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
					int follow_index = this.GetId (this.AsLong(Keys[start]));
					if (i != follow_index) {
						this.Swap (Keys, Values, this.startpos[follow_index], start);
					}
					this.startpos[follow_index]++;
				}
			}
		}
		
		public void Sort (IList<KeyType> Keys, IList<ValueType> Values)
		{
			int msd = (int)Math.Ceiling (numbits * 0.5);
			this.Reset (numbits, msd, true);
			this.CountingSort (Keys, Values);
			int[] counters = new int[this.finalpos.Length];
			int prev = 0;
			for (int i = 0; i < counters.Length; i++) {
				counters[i] = this.finalpos[i] - prev;
				prev = this.finalpos[i];
			}
			this.Reset (this.numbits - msd, this.numbits - msd, false);
			int acc = 0;
			//Console.WriteLine ("msd: {0}, numbits: {1}, this.msd: {2}, mask: {3}",
			//	msd, this.numbits, this.msd, this.mask);
			// TODO: parallel processing of each block
			for (int i = 0; i < counters.Length; i++) {
				int c = counters[i];
				if (c == 0) {
					continue;
				}
				this.Clear ();
				ListShiftIndex<KeyType> K = new ListShiftIndex<KeyType> (Keys, acc, c);
				if (Values == null) {
					this.CountingSort (K, null);
				} else {
					ListShiftIndex<ValueType> V = new ListShiftIndex<ValueType> (Values, acc, c);
					this.CountingSort (K, V);
				}
				acc += counters[i];
			}
		}
	}
}

