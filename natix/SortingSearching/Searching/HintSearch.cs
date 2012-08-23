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
//   Original filename: natix/SortingSearching/Searching/HintSearch.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	/// <summary>
	///  Hint search. It's not optimized, right now it focus on the reduction of the comparisons
	/// </summary>
	public class HintSearch : ISearchAlgorithm<int>
	{
		int compcounter = 0;
		ISearchAlgorithm<int> BackwardSearch;
		ISearchAlgorithm<int> ForwardSearch;
		int Hint;
		
		public int CompCounter {
			get {
				return this.compcounter + this.BackwardSearch.CompCounter + this.ForwardSearch.CompCounter;
			}
		}

		public HintSearch (int hint) : this(hint, new DoublingSearch<int>(), new DoublingSearch<int>())
		{
		}
		
		/// <summary>
		/// A hint searcher. hint must be a float value between 0 and 1, indicating the cut position.
		/// </summary>
		public HintSearch (int hint, ISearchAlgorithm<int> B, ISearchAlgorithm<int> F)
		{
			this.Hint = hint;
			this.BackwardSearch = B;
			this.ForwardSearch = F;
		}

		public bool Search (int data, IList<int> A, out int occPosition, int min, int max)
		{
			int cmp;
			int hint = this.Hint * A.Count;
			// mid must be the hint position, it must be given in the constructor?
			this.compcounter++;
			cmp = data.CompareTo (A[hint]);
			if (cmp < 0) {
				// Make a list mirror for BackwardSearch
				hint--;
				var Arev = new ListGen<int> ((int iS) => -A[hint - iS], hint - min + 1);
				// Console.WriteLine ("min: {0}, hint: {1}, len: {2}", min, hint, Arev.Count);
				var found = this.BackwardSearch.Search (-data, Arev, out occPosition, 0, Arev.Count);
				// Console.WriteLine ("-- occPos: {0}", occPosition);
				occPosition = hint - occPosition;
				// Console.WriteLine ("-- occPosFinal: {0}, found: {1}, data: {2}, A[occP]: {3}", occPosition, found, data, A[occPosition]);
				return found;
			} else {
				if (cmp == 0) {
					occPosition = hint;
					return true;
				}
				return this.ForwardSearch.Search (data, A, out occPosition, hint + 1, max);
			}
		}
	}
}
