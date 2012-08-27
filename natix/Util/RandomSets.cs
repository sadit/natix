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
//   Original filename: natix/Util/RandomSets.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix
{
	/// <summary>
	/// Random sets. A number of useful methods to create and sample random sets, subsets, and permutations.
	/// </summary>
	public class RandomSets
	{
		/// <summary>
		/// Takes "count" items from the range [0,n-1]
		/// </summary>
		public static int[] GetRandomSubSet (int count, int n)
		{
			return GetRandomSubSet (0, count, n);
		}
		
		/// <summary>
		/// Expands a range into a list (it uses a list generator) [min,max-1]
		/// </summary>
		/// <returns>
		/// The expanded range.
		/// </returns>
		/// <param name='min'>
		/// Minimum.
		/// </param>
		/// <param name='max'>
		/// Max.
		/// </param>
		public static IList<int> GetExpandedRange (int min, int max)
		{
			return new ListGen<int> ((int i) => min + i, max - min);
		}

		/// <summary>
		/// Expands a range into a list (it uses a list generator) [0,count-1]
		/// </summary>
		/// <returns>
		/// The expanded range.
		/// </returns>
		/// <param name='count'>
		/// Count.
		/// </param>
		public static IList<int> GetExpandedRange (int count)
		{
			return new ListGen<int> ((int i) => i, count);
		}

		/// <summary>
		///  A random subset of size n, in random order, of items in the range [min_value,max_value).
		/// </summary>
		public static int[] GetRandomSubSet (int min_value, int max_value, int n)
		{
			if (max_value - min_value < n) {
				n = max_value - min_value;// print some warning message
			}
			var marked = new BitStream32 ();
			marked.Write (false, n);
			Random rand = new Random ();
			// start new implementation
			int[] R = new int[n];
			int c = 0;
			while (c < n) {
				var r = rand.Next (n);
				if (marked [r]) {
					continue;
				}
				R [c] = r + min_value;
				marked [r] = true;
				c++;
			}
			return R;
		}

		/// <summary>
		/// Gets the identity permutation of size n
		/// </summary>
		/// <returns>
		public static int[] GetIdentity (int n)
		{
			var I = new int[n];
			for (int i = 0; i < n; ++i) {
				I[i] = i;
			}
			return I;
		}

		/// <summary>
		/// Select "samplesize" items randomly from "list"
		/// </summary>
		public static T[] GetRandomSubSet<T> (IList<T> list, int samplesize)
		{
			var I = GetRandomSubSet (0, list.Count - 1, samplesize);
			int len = I.Length;
			T[] R = new T[len];
			for (int i = 0; i < len; i++) {
				R [i] = list [I [i]];
			}
			return R;
		}
		/// <summary>
		/// Shuffles the list, using a random permutation (Knuth's Fisher-Yates shuffle)
		/// </summary>
		public static void RandomShuffle<T> (IList<T> list)
		{
			int n = list.Count;
			Random rand = new Random ();
			for (int i = 0; i < n; i++) {
				int j = rand.Next (i, n); // the range is [i,n)
				var tmp = list [i];
				list [i] = list [j];
				list [j] = tmp;
			}
		}
		
		/// <summary>
		/// Gets the random permutation.
		/// </summary>
		/// <returns>
		/// The random permutation.
		/// </returns>
		/// <param name='n'>
		/// N.
		/// </param>
		public static int[] GetRandomPermutation (int n)
		{
			int[] P = new int[n];
			for (int i = 0; i < n; i++) {
				P [i] = i;
			}
			RandomShuffle<int> (P);
			return P;
		}
		
		/// <summary>
		/// Returns the inverse of the permutation "P"
		/// </summary>
		public static int[] GetInverse (IList<int> P)
		{
			int n = P.Count;
			var Inv = new int[n];
			for (int i = 0; i < n; i++) {
				Inv [P [i]] = i;
			}
			return Inv;
		}
	}
}

