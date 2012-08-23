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
//   Original filename: natix/SortingSearching/Sorting.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class Sorting
	{
		public class ComparerWrapper<KeyType> : IComparer<KeyType>
		{
			Comparison<KeyType> cmp;
			public ComparerWrapper (Comparison<KeyType> cmp)
			{
				this.cmp = cmp;
			}
			public int Compare (KeyType x, KeyType y)
			{
				return this.cmp (x, y);
			}
		}
		
		// static Random R = new Random();
		static void Swap<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int a, int b)
		{
			KeyType v = Keys[a];
			Keys[a] = Keys[b];
			Keys[b] = v;
			if (Values != null) {
				ValueType u = Values[a];
				Values[a] = Values[b];
				Values[b] = u;
			}
		}
			
		/// <summary>
		/// Single
		/// </summary>
		public static void Sort<KeyType> (IList<KeyType> Keys) where KeyType : IComparable
		{
			Sort<KeyType, KeyType> (Keys, null);
		}

		public static void Sort<KeyType> (IList<KeyType> Keys, Comparison<KeyType> cmpfun)
		{
			Sort<KeyType, KeyType> (Keys, null, cmpfun);
		}
		
		public static void Sort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values) where KeyType : IComparable
		{
			Sort<KeyType, ValueType> (Keys, Values, 0, Keys.Count);
		}

		public static void Sort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, Comparison<KeyType> cmpfun)
		{
			Sort<KeyType, ValueType> (Keys, Values, 0, Keys.Count, cmpfun);
		}

		public static void Sort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int StartIndex, int Length) where KeyType: IComparable
		{
			Sort<KeyType, ValueType> (Keys, Values, StartIndex, Length, (u,v) => u.CompareTo(v));
		}

		public static void Sort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int StartIndex, int Length, Comparison<KeyType> cmpfun)
		{
			var K = Keys as KeyType[];
			if (K != null) {
				var V = Values as ValueType[];
				if (Values == null || V != null) {
					//Array.Sort<KeyType, ValueType> (K, V, StartIndex, Length, new ComparerFromComparison<KeyType> (cmpfun));
					//Array.Sort<KeyType, ValueType> (K, V, StartIndex, Length);
					var cmp = new ComparerFromComparison<KeyType> (cmpfun);
					Array.Sort (K, V, StartIndex, Length, cmp);
					// Array.Sort (K, V, StartIndex, Length);
					// Array.Sort (K);
					return;
				}
			}
			QSort<KeyType, ValueType> (Keys, Values, StartIndex, StartIndex + Length, cmpfun);
		}

		public static void BubbleSort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int startIndex, int length, Comparison<KeyType> cmpfun)
		{
			bsort<KeyType, ValueType> (Keys, Values, startIndex, startIndex + length, cmpfun);
		}

		public static void bsort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int min, int max, Comparison<KeyType> cmpfun)
		{
			bool donext = true;
			while (donext) {
				donext = false;
				for (int i = min + 1; i < max; i++) {
					if (cmpfun (Keys[i - 1], Keys[i]) > 0) {
						Swap<KeyType, ValueType> (Keys, Values, i - 1, i);
						donext = true;
					}
				}
			}
		}
		
		public static void QSort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int startIndex, int length, Comparison<KeyType> cmpfun)
		{
			qsort<KeyType, ValueType> (Keys, Values, startIndex, startIndex + length - 1, cmpfun);
		}
		
		static Random rand = new Random();
		public static void qsort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int low0, int high0, Comparison<KeyType> cmp)
		{
			// adapted from source ./mcs/class/corlib/System/Array.cs, from mono-2.6.4 source
			// the modifications are:
			// IList<T>, generics support
			if (low0 >= high0) {
				return;
			}
			int low = low0;
			int high = high0;

			// Be careful with overflows
			// int mid = low + ((high - low) >> 1);
			int _mid = rand.Next (low, high + 1);
			KeyType objPivot = Keys [_mid];
			
			while (true) {
				// Move the walls in
				while (low < high0 && cmp (objPivot, Keys[low]) > 0) {
					++low;
				}
				while (high > low0 && cmp (objPivot, Keys[high]) < 0) {
					--high;
				}
				if (low <= high) {
					Swap<KeyType, ValueType> (Keys, Values, low, high);
					++low;
					--high;
				} else {
					break;
				}
			}
			if (low0 < high) {
				qsort<KeyType, ValueType> (Keys, Values, low0, high, cmp);
			}
			if (low < high0) {
				qsort<KeyType, ValueType> (Keys, Values, low, high0, cmp);
			}
		}
				
		public static void LocalInsertionSort<KeyType, ValueType> (IList<KeyType> Keys, IList<ValueType> Values, int StartIndex, int Length, Comparison<KeyType> cmpfun)
		{
			if (Values == null) {
				SplayTree<KeyType> tree = new SplayTree<KeyType> (cmpfun);
				Length += StartIndex;
				for (int i = StartIndex; i < Length; i++) {
					tree.Add (Keys[i]);
				}
				Length -= StartIndex;
				for (int i = StartIndex; i < Length; i++) {
					Keys[i] = tree.RemoveFirst ();
				}
			} else {
				var tree = new SplayTree<KeyValuePair<KeyType, ValueType>> ((KeyValuePair<KeyType, ValueType> a, KeyValuePair<KeyType, ValueType> b) => cmpfun (a.Key, b.Key));
				Length += StartIndex;
				for (int i = StartIndex; i < Length; i++) {
					tree.Add (new KeyValuePair<KeyType, ValueType> (Keys[i], Values[i]));
				}
				Length -= StartIndex;
				for (int i = StartIndex; i < Length; i++) {
					var f = tree.RemoveFirst ();
					Keys[i] = f.Key;
					Values[i] = f.Value;
				}
			}
		}
		
	}
}

