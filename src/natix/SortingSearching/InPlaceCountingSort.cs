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
//   Original filename: natix/SortingSearching/InPlaceCountingSort.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class InPlaceCountingSort<ValueType>
	{
		public short numbits;
		public short msd;
		public int[] startpos;
		public int[] finalpos;
		public int mask;
		public int AlphabetSize;
		
		public InPlaceCountingSort (int numbits, int msd)
		{
			this.numbits = (short)numbits;
			this.msd = (short)msd;
			this.startpos = new int[this.Range];
			this.finalpos = new int[this.Range];
			this.mask = this.Range - 1;
			this.AlphabetSize = 0;
		}
		
		public int Range {
			get {
				return 1 << msd;
			}
		}

		public void Clear ()
		{
			this.AlphabetSize = 0;
			for (int i = 0; i < this.startpos.Length; i++) {
				this.startpos[i] = 0;
				this.finalpos[i] = 0;
			}
		}
		
		public int GetId(int item)
		{
			return ( item >> (this.numbits - msd) ) & this.mask;
		}
		
		void ComputeStats (IList<int> A, int start_pos, int end_pos)
		{
			for (int i = start_pos; i <= end_pos; i++) {
				this.finalpos[this.GetId (A[i])]++;
			}
			int acc = start_pos;
			int range = this.Range;
			for (int i = 0; i < range; i++) {
				this.startpos[i] = acc;
				acc += this.finalpos[i];
				this.finalpos[i] = acc;
				if (this.startpos[i] < this.finalpos[i]) {
					this.AlphabetSize++;
				}
			}
		}
		
		void Swap (IList<int> Keys, IList<ValueType> Values, int a, int b)
		{
			int v = Keys[a];
			Keys[a] = Keys[b];
			Keys[b] = v;
			if (Values != null) {
				ValueType u = Values[a];
				Values[a] = Values[b];
				Values[b] = u;
			}
		}
		
		public void Sort (IList<int> Keys, IList<ValueType> Values)
		{
			this.Sort (Keys, Values, 0, Keys.Count);
		}
		
		public void Sort (IList<int> Keys, IList<ValueType> Values, int start_pos, int len)
		{
			this.ComputeStats (Keys, start_pos, start_pos + len - 1);
			int range = this.Range;
			for (int i = 0; i < range; i++) {
				while (this.startpos[i] < this.finalpos[i]) {
					int start = this.startpos[i];
					int raw_key = Keys[start];
					int follow_index = this.GetId (raw_key);
					this.Swap (Keys, Values, this.startpos[follow_index], start);
					this.startpos[follow_index]++;
				}
			}
		}
	}
}

