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
//   Original filename: natix/SortingSearching/SortSeq.cs
// 
using System;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class SortSeq<ValueType>
	{
//		int alphabet_numbits;
//		IList<IList<int>> Keys;
//		IList<ValueType> Values;
//		
//		public SortSeq (int alphabet_size)
//		{
//			this.alphabet_numbits = (int)Math.Ceiling (Math.Log (alphabet_size+1, 2));
//		}
//		
//		public void Sort (IList<IList<int>> Keys, IList<ValueType> Values, int start_index, int count)
//		{
//			this.Keys = Keys;
//			this.Values = Values;
//			this.Sort (0, count);
//		}
//		
//		public void Sort (IList<IList<int>> Keys, IList<ValueType> Values)
//		{
//			this.Sort (Keys, Values, 0, Keys.Count);
//		}
//		
//		void Sort (int start_index, int count)
//		{
//			this._numerical_sort(0, start_index, start_index + count - 1);
//		}
//
//		void _numerical_sort (int offset, int low, int high)
//		{
//			if (offset > 1) {
//				this._sort (offset, low, high);
//			} else {
//				var S = new InPlaceCountingSort<IList<int>> (this.alphabet_numbits, this.alphabet_numbits);
//				var G = new ListGen2<int> ((int x) =>
//				{
//					return this._get_char (offset, x);
//				}, null, this.Keys.Count);
//				Console.WriteLine ("**** _numerical_sort low: {0}, high: {1}, offset: {2}", low, high, offset);
//				S.Sort (G, this.Keys, low, high - low + 1);
//				int startIndex = low;
//				for (int i = 0; i < S.finalpos.Length; i++) {
//					int finalIndex = S.finalpos[i];
//					int len = finalIndex - startIndex;
//					if (len > 1) {
//						this._numerical_sort (offset + 1, startIndex, finalIndex - 1);
//					}
//					startIndex = finalIndex;
//				}
//			}
//		}
//
//		void _swap (int a, int b)
//		{
//			// Console.WriteLine ("a: {0}, b: {1}, len: {2}",a,b, this.SA.Length);
//			var v = this.Keys[a];
//			this.Keys[a] = this.Keys[b];
//			this.Keys[b] = v;
//			if (this.Values != null) {
//				ValueType u = this.Values[a];
//				this.Values[a] = this.Values[b];
//				this.Values[b] = u;
//			}
//
//		}
//
//		int _get_char (int offset, int i)
//		{
//			// Console.WriteLine ("****** _get_char  i: {0}, keys.count: {1}, offset: {2}", i, Keys.Count, offset);
//			var key = this.Keys[i];
//			if (offset < key.Count) {
//				return key[offset] + 1;
//			} else {
//				return 0;
//			}
//		}
//
//		struct stack_sort
//		{
//			public int offset;
//			public int low;
//			public int high;
//
//			public stack_sort (int _offset, int _low, int _high)
//			{
//				this.offset = _offset;
//				this.low = _low;
//				this.high = _high;
//			}
//		}
//		
//		void _sort (int offset, int low0, int high0)
//		{
//			Stack<stack_sort> stack = new Stack<stack_sort> ();
//			stack.Push (new stack_sort (offset, low0, high0));
//			while (stack.Count > 0) {
//				var s = stack.Pop ();
//				offset = s.offset;
//				low0 = s.low;
//				high0 = s.high;
//				if (low0 >= high0) {
//					continue;
//				}
//				int[] start_pos = new int[3];
//				int[] final_pos = new int[3];
//				int mid = low0 + ((high0 - low0) >> 1);
//				var piv = this._get_char (offset, mid);
//				for (int i = low0; i <= high0; i++) {
//					var c = this._get_char (offset, i);
//					if (c < piv) {
//						final_pos[0]++;
//					} else if (c == piv) {
//						final_pos[1]++;
//					} else {
//						final_pos[2]++;
//					}
//				}
//				start_pos[0] = low0;
//				final_pos[0] += low0;
//				for (int i = 1; i < final_pos.Length; i++) {
//					start_pos[i] = final_pos[i - 1];
//					final_pos[i] += final_pos[i - 1];
//				}
//				for (int i = 0; i < final_pos.Length; i++) {
//					while (start_pos[i] < final_pos[i]) {
//						var start = start_pos[i];
//						var c = this._get_char (offset, start);
//						if (c < piv) {
//							if (i != 0) {
//								this._swap (start, start_pos[0]);
//							}
//							start_pos[0]++;
//						} else if (c == piv) {
//							if (i != 1) {
//								this._swap (start, start_pos[1]);
//							}
//							start_pos[1]++;
//						} else {
//							if (i != 2) {
//								this._swap (start, start_pos[2]);
//							}
//							start_pos[2]++;
//						}
//					}
//				}
//				int L = low0;
//				int R = final_pos[0] - 1;
//				if (L < R) {
//					stack.Push (new stack_sort (offset, L, R));
//				}
//				if (piv > 0) {
//					L = final_pos[0];
//					R = final_pos[1] - 1;
//					if (L < R) {
//						stack.Push (new stack_sort (offset + 1, L, R));
//					}
//				}
//				L = final_pos[1];
//				R = high0;
//				if (L < R) {
//					stack.Push (new stack_sort (offset, L, R));
//				}
//			}
//		}
	}
}
