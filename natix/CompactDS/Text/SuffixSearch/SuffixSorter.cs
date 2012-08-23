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
//   Original filename: natix/CompactDS/Text/SuffixSearch/SuffixSorter.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class SuffixSorter
	{
		public IList<int> TXT;
		public int[] SA;
		public IList<int> charT;
		public GGMN newF;
		// int alphabetSize;
		int alphabet_numbits;
		
		public SuffixSorter (IList<int> text, int alphabet_size)
		{
			this.TXT = text;
			this.alphabet_numbits = (int)Math.Ceiling (Math.Log (alphabet_size, 2));
			int len = this.TXT.Count + 1;
			this.SA = new int[len];
			for (int i = 0; i < len; i++) {
				this.SA[i] = i;
			}
		}
		
		public void Sort ()
		{
			this._numerical_sort (0, 0, this.SA.Length - 1);
			// this._sort(0, 0, this.SA.Length-1);
			// creating auxiliar structures and bitmaps
			this.charT = new List<int> ();
			var B_newF = new BitStream32 ();
			this.charT.Add ('$'); // the special lexicographically smaller symbol
			B_newF.Write (true);
			for (int i = 1; i < this.SA.Length; i++) {
				var c = this.TXT[this.SA[i]];
				if (i == 1) {
					this.charT.Add (c);
					B_newF.Write (true);
				} else {
					if (this.charT[this.charT.Count - 1] != c) {
						this.charT.Add (c);
						B_newF.Write (true);
					} else {
						B_newF.Write (false);
					}
				}
			}
			this.newF = new GGMN ();
			this.newF.Build (B_newF, 8);
		}
		
		public void _numerical_sort (int offset, int low, int high)
		{
			if (offset > 1) {
				this._sort (offset, low, high);
			} else {
				var S = new InPlaceCountingSort<int> (alphabet_numbits, alphabet_numbits);
				var G = new ListGen2<int> ((int x) =>
					{
					var c = this._get_char (offset, x);
					if (c < 0) {
						return 0;
					}
					return c;
				}, null, this.SA.Length);
				// Console.WriteLine ("LLLLLL low: {0}, high: {1}", low, high);
				S.Sort (G, this.SA, low, high - low + 1);
				int startIndex = low;
				for (int i = 0; i < S.finalpos.Length; i++) {
					int finalIndex = S.finalpos[i];
					int len = finalIndex - startIndex;
					if (len > 1) {
						this._numerical_sort (offset + 1, startIndex, finalIndex - 1);
					}
					startIndex = finalIndex;
				}
			}
		}

//		public void _numerical_sort (int offset, int low, int high)
//		{
//			if (offset > 5) {
//				this._sort (offset, low, high);
//			} else {
//				var S = new InPlaceCountingSort<int> (8, 8);
//				while (true) {
//					var G = new ListGen2<int> ((int x) =>
//					{
//						var c = this._get_char (offset, x);
//						if (c < 0) {
//							return 0;
//						}
//						return c;
//					}, null, this.SA.Length);
//					S.Sort (G, this.SA, low, high - low + 1);
//					
//					int startIndex = low;
//					for (int i = 0; i < S.finalpos.Length; i++) {
//						int finalIndex = S.finalpos[i];
//						int len = finalIndex - startIndex;
//						if (len > 1) {
//							this._sort (offset + 1, startIndex, finalIndex - 1);
//						}
//						startIndex = finalIndex;
//					}
//				}
//			}
//		}

		void _swap (int a, int b)
		{
			// Console.WriteLine ("a: {0}, b: {1}, len: {2}",a,b, this.SA.Length);
			int v = this.SA[a];
			this.SA[a] = this.SA[b];
			this.SA[b] = v;
		}
		
		int _get_char (int offset, int i)
		{
			var p = this.SA[i];
			if (p + offset < this.TXT.Count) {
				return this.TXT[p + offset];
			} else {
				return -1;
			}
		}
		
		struct stack_sort
		{
			public int offset;
			public int low;
			public int high;
			
			public stack_sort (int _offset, int _low, int _high)
			{
				this.offset = _offset;
				this.low = _low;
				this.high = _high;
			}
		}
		
		long XXX = 0;
		//int NUMCALLS = 0;
		void _sort (int offset, int low0, int high0)
		{
			Stack<stack_sort> stack = new Stack<stack_sort> ();
			stack.Push (new stack_sort (offset, low0, high0));
			while (stack.Count > 0) {
				var s = stack.Pop ();
				offset = s.offset;
				low0 = s.low;
				high0 = s.high;
				/*if (high0 - low0 > 16 || offset < 4) {
					//Console.WriteLine ("REALLY LARGE {0}, low: {1}, high: {2}, offset: {3}, stack: {4}",
					//	high0 - low0 + 1, low0, high0, offset, stack.Count);
					this._numerical_sort (offset, low0, high0);
					continue;
				}*/
				/*if (XXX % 1000000 == 0) {
					Console.WriteLine ("X: {4}, offset: {0}, low: {1}, high: {2}, stack: {3}", offset, low0, high0, stack.Count, XXX);
				}*/
				XXX++;
				if (low0 >= high0) {
					continue;
				}
				int[] start_pos = new int[3];
				int[] final_pos = new int[3];
				int mid = low0 + ((high0 - low0) >> 1);
				var piv = this._get_char (offset, mid);
				for (int i = low0; i <= high0; i++) {
					var c = this._get_char (offset, i);
					/*if (offset > 10000 && i == low0) {
						Console.WriteLine ("0000> offset: {0}, low: {1}, high: {2}, stack: {3}", offset, low0, high0, stack.Count);
						Console.WriteLine ("0000> piv: {0}, c: {1}", piv, c);
						Console.WriteLine ("0000> text-low: {0}, text-high: {1}", this.SA[low0], this.SA[high0]);
					}*/
					if (c < piv) {
						final_pos[0]++;
					} else if (c == piv) {
						final_pos[1]++;
					} else {
						final_pos[2]++;
					}
				}
				start_pos[0] = low0;
				final_pos[0] += low0;
				for (int i = 1; i < final_pos.Length; i++) {
					start_pos[i] = final_pos[i - 1];
					final_pos[i] += final_pos[i - 1];
				}
				for (int i = 0; i < final_pos.Length; i++) {
					while (start_pos[i] < final_pos[i]) {
						var start = start_pos[i];
						var c = this._get_char (offset, start);
						if (c < piv) {
							if (i != 0) {
								this._swap (start, start_pos[0]);
							}
							start_pos[0]++;
						} else if (c == piv) {
							if (i != 1) {
								this._swap (start, start_pos[1]);
							}
							start_pos[1]++;
						} else {
							if (i != 2) {
								this._swap (start, start_pos[2]);
							}
							start_pos[2]++;
						}
					}
				}
				int L = low0;
				int R = final_pos[0] - 1;
				if (L < R) {
					stack.Push (new stack_sort (offset, L, R));
				}
				if (piv >= 0) {
					L = final_pos[0];
					R = final_pos[1] - 1;
					if (L < R) {
						stack.Push (new stack_sort (offset + 1, L, R));
					}
				}
				L = final_pos[1];
				R = high0;
				if (L < R) {
					stack.Push (new stack_sort (offset, L, R));
				}
			}
		}
	}
}

//		void _sort (int offset, int low0, int high0)
//		{
//			Stack<stack_sort> stack = new Stack<stack_sort> ();
//			stack.Push (new stack_sort (offset, low0, high0));
//			long X = 0;
//			while (stack.Count > 0) {
//				var s = stack.Pop ();
//				offset = s.offset;
//				low0 = s.low;
//				high0 = s.high;
//				if (X % 1000000 == 0) {
//					Console.WriteLine ("X: {4}, offset: {0}, low: {1}, high: {2}, stack: {3}", offset, low0, high0, stack.Count, X);
//				}
//				X++;
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
//				if (piv >= 0) {
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

//			Console.WriteLine ("offset: {0}, low0: {1}, high0: {2}", offset, low0, high0);
//			
//			// adapted from source ./mcs/class/corlib/System/Array.cs, from mono-2.6.4 source
//			if (low0 >= high0) {
//				return;
//			}
//			int[] start_pos = new int[3];
//			int[] final_pos = new int[3];
//			int mid = low0 + ((high0 - low0) >> 1);
//			var piv = this._get_char (offset, mid);
//			for (int i = low0; i <= high0; i++) {
//				var c = this._get_char (offset, i);
//				if (c < piv) {
//					final_pos[0]++;
//				} else if (c == piv) {
//					final_pos[1]++;
//				} else {
//					final_pos[2]++;
//				}
//			}
//			start_pos[0] = low0;
//			final_pos[0] += low0;
//			for (int i = 1; i < final_pos.Length; i++) {
//				start_pos[i] = final_pos[i - 1];
//				final_pos[i] += final_pos[i - 1];
//			}
//			for (int i = 0; i < final_pos.Length; i++) {
//				while (start_pos[i] < final_pos[i]) {
//					var start = start_pos[i];
//					var c = this._get_char (offset, start);
//					if (c < piv) {
//						if (i != 0) {
//							this._swap (start, start_pos[0]);
//						}
//						start_pos[0]++;
//					} else if (c == piv) {
//						if (i != 1) {
//							this._swap (start, start_pos[1]);
//						}
//						start_pos[1]++;
//					} else {
//						if (i != 2) {
//							this._swap (start, start_pos[2]);
//						}
//						start_pos[2]++;
//					}
//				}
//			}
//			int L = low0;
//			int R = final_pos[0] - 1;
//			if (L < R) {
//				this._sort (offset, L, R);
//			}
//			if (piv >= 0) {
//				L = final_pos[0];
//				R = final_pos[1] - 1;
//				if (L < R) {
//					this._sort (offset + 1, L, R);
//				}
//			}
//			L = final_pos[1];
//			R = high0;
//			if (L < R) {
//				this._sort (offset, L, R);
//			}
