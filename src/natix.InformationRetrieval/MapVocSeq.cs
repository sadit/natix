// 
//  Copyright 2012  sadit
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using natix;
using System.Collections;
using System.Collections.Generic;

namespace natix.InformationRetrieval
{
	public class MapVocSeq
	{
		public MapVocSeq ()
		{
		}
		
		/// <summary>
		/// Maps (in-place) the sequence to (lexicographically non-case sensitive) sorting of the vocabulary.
		/// The new vocabulary is returned.
		/// </summary>

		public static IList<string> SortingVoc (IDictionary<string,int> voc, IList<int> seq, IComparer<string> comp = null)
		{
			int[] perm;
			return SortingVoc(voc, seq, out perm, comp);
		}

		public static IList<string> SortingVoc (IDictionary<string,int> voc, IList<int> seq, out int[] perm, IComparer<string> comp = null)
		{
			string[] new_voc;
			BeginSortingVoc (voc, seq, out new_voc, out perm);
			if (comp == null) {
				Array.Sort<string,int> (new_voc, perm);
			} else {
				Array.Sort<string,int> (new_voc, perm, comp);
			}

			EndSortingVoc (seq, perm);
			return new_voc;

		}

		public static IList<string> SortingVoc (IDictionary<string,int> voc, IList<int> seq, Action<IList<string>,IList<int>> sorting)
		{
			int[] perm;
			return SortingVoc (voc, seq, out perm, sorting);
		}

		public static IList<string> SortingVoc (IDictionary<string,int> voc, IList<int> seq, out int[] perm, Action<IList<string>,IList<int>> sorting)
		{
			string[] new_voc;
			BeginSortingVoc (voc, seq, out new_voc, out perm);
			sorting (new_voc, perm);
			EndSortingVoc (seq, perm);
			return new_voc;
		}

		public static void BeginSortingVoc (IDictionary<string,int> voc, IList<int> seq, out string[] new_voc, out int[] perm)
		{
			new_voc = new string[voc.Count];
			perm = new int[voc.Count];
			foreach (var p in voc) {
				new_voc [p.Value] = p.Key;
			}
			for (int i = 0; i < perm.Length; ++i) {
				perm [i] = i;
			}
		}

		public static void EndSortingVoc (IList<int> seq, int[] perm)
		{
			var inv = RandomSets.GetInverse (perm);
			for (int i = 0; i < seq.Count; ++i) {
				seq [i] = inv [seq [i]];
			}
		}
	}
}

