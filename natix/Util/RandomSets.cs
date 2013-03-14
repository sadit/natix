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
		public static T[] GetRandomSubSet<T> (int count, IList<T> list, int random_seed = -1)
		{
			var I = GetRandomSubSet(count, list.Count, random_seed);
			T[] R = new T[count];
			for (int i = 0; i < count; i++) {
				R [i] = list [I [i]];
			}
			return R;
		}

		/// <summary>
		/// Takes "count" items from the range [0,n-1]. Only useful for count << n
		/// </summary>
		public static int[] GetRandomSubSet (int count, int n, int random_seed = -1)
		{
			var rand = GetRandom(random_seed);
			var D = new HashSet<int>();
			int[] L = new int[count];
			while (D.Count < count) {
				var p = rand.Next(n);
				if (D.Contains(p)) {
					continue;
				}
				L[D.Count] = p;
				D.Add(p);
			}
			return L;
		}

		/// <summary>
		/// Shuffles the list, using a random permutation (Knuth's Fisher-Yates shuffle)
		/// </summary>
		public static void RandomShuffle<T> (IList<T> list, int random_seed = -1)
		{
			int n = list.Count;
			Random rand = GetRandom(random_seed);
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
		public static int[] GetRandomPermutation (int n, int random_seed = -1)
		{
			int[] P = new int[n];
			for (int i = 0; i < n; i++) {
				P [i] = i;
			}
			RandomShuffle<int> (P, random_seed);
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

		static Random _SeedGenerator = new Random();
		// static object _SeedMonitor = new object();
		public static Random GetRandom (int random_seed)
        {
            if (random_seed < 0) {
                return new Random (GetRandomInt ());
            } else {
                return new Random (random_seed);
            }
		}

		public static int GetRandomInt()
		{
			int seed;
			//lock (_SeedMonitor) {
            seed = _SeedGenerator.Next();
			// }
			return seed;
		}
	}
}

