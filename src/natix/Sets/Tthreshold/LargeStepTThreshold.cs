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
//   Original filename: natix/Sets/Tthreshold/LargeStepTThreshold.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	public class LargeStepTThreshold : ITThresholdAlgorithm
	{
		int splaycomparisons = 0;
		public int CompCounter {
			get {
				return this.SearchAlgorithm.CompCounter + this.splaycomparisons;
			}
		}
		ISearchAlgorithm<int> SearchAlgorithm;
		
		public LargeStepTThreshold (ISearchAlgorithm<int> searchalg)
		{
			this.SearchAlgorithm = searchalg;
		}
		
		void Print (IList<IList<int>> L)
		{
			Console.WriteLine ("***********");
			Console.WriteLine ("List {");
			for(int i = 0; i < L.Count; i++) {
				Print (L[i]);
			}
			Console.WriteLine ("}");
		}
		
		void Print (IList<int> A)
		{
			Console.Write ("= Count: {0}: ", A.Count);
			for (int i = 0; i < A.Count; i++) {
				Console.Write (" {0}", A[i]);
			}
			Console.WriteLine ();
		}
		
		/// <summary>
		/// Advance in all the items from startT to endT, if do_search is true it applies a search algorithm to find the next
		/// position. if do_search is false just advance by one. 
		/// </summary>
		public short SkipInSortedLists (int piv, ListShiftIndex<ListShiftIndex<int>> posting, int startT, int endT, bool do_search)
		{
			int swapIndex = 0;
			short count = 0;
			for (int i = endT; i >= startT && i >= swapIndex; i--) {
				var currentList = posting [i];
				// Console.WriteLine ("............ i: {0}, swapIndex: {1}, do_search: {2} .............>", i, swapIndex, do_search);
				// Print (currentList);
				if (do_search) {
					int occpos;
					if (this.SearchAlgorithm.Search (piv, currentList, out occpos, 0, currentList.Count)) {
						occpos++;
						count++;
					}
					// currentList.AdvanceStartPosition (occpos);
					currentList.Shift (occpos);
				} else {
					count++;
					// currentList.AdvanceStartPosition (1);
					currentList.Shift (1);
				}
				if (currentList.Count < 1) {
					// drop list
					// TODO: Swap to the end and change advance for an "decrease length" function
					posting [i] = posting [swapIndex];
					posting [swapIndex] = currentList;
					swapIndex++;
					i++;
				}
			}
			// posting.AdvanceStartPosition (swapIndex);
			posting.Shift (swapIndex);
			return count;
		}
		
		public void SearchTThreshold (IList<IList<int>> PostingLists, int T, out IList<int> docs, out IList<short> card)
		{
			// K - T = Number of mismatches (errors)
			int suggestedCardinality = 64;
			docs = new List<int> (suggestedCardinality);
			card = new List<short> (suggestedCardinality);
			// we use InternalListClass objects allowing simple coding,
			// hoping that the JIT perform a good work optimizing the final code
			var _posting = new List<ListShiftIndex<int>> (PostingLists.Count);
			// List<InternalListClass<int>> _posting = new List<InternalListClass<int>> (PostingLists.Count);
			
			for (int i = 0; i < PostingLists.Count; i++) {
				var list = PostingLists [i];
				_posting.Add (new ListShiftIndex<int> (list, 0, list.Count));
				//_posting.Add (new InternalListClass<int> (0, PostingLists [i].Count, PostingLists [i]));
			}
			var posting = new ListShiftIndex< ListShiftIndex<int> > (_posting, 0, _posting.Count);
			Comparison<ListShiftIndex<int>> comptop = delegate(ListShiftIndex<int> x, ListShiftIndex<int> y) {
				this.splaycomparisons++;
				return x [0] - y [0];
			};
			while (posting.Count >= T) {
				int endT = T - 1;
				// internal note:
				// we can get fast access using splaytrees but there are other
				// associated operations
				// (test both or analyze them if you want to know more about it)
				//Sorting.Sort<IList<int>> ((IList<IList<int>>)posting, comptop);
				{
					Sorting.Sort<ListShiftIndex<int>> (posting, comptop);
				}
				// Print (posting);
				var p = posting [endT] [0];
				if (posting [0] [0] == p) {
					// Console.WriteLine ("Starting T: {0}, endT: {1}, posting.Count: {2}!!!", T, endT, posting.Count);
					// we have a match
					docs.Add (p);
					// advance from Tindex to |postings|
					while (endT < posting.Count && posting[endT][0] == p) {
						++endT;
					}
					card.Add ((short)endT);
					this.SkipInSortedLists (p, posting, 0, endT - 1, false);
					// Console.WriteLine ("Ending T: {0}, endT: {1}, posting.Count: {2}!!!", T, endT, posting.Count);
				} else {
					// skip all lists behind than Tindex
					// sort only the necessary items (0 to Tindex-1), we ignore this fact
					// because posting is an small set
					var startingLength = posting.Count;
					// Console.WriteLine ("Starting endT-1: {0}", endT - 1);
					short count = this.SkipInSortedLists (p, posting, 0, endT - 1, true);
					// short count = this.SkipInSortedLists (p, posting, 0, posting.Count - 1, true);
					// Console.WriteLine ("---> count: {0}", count);
					if (count > 0) {
						// posting list can reduce its length
						endT -= startingLength - posting.Count;
						int startT = endT;
						// advance from Tindex to |postings|
						while (endT < posting.Count && posting[endT][0] == p) {
							++endT;
							++count;
						}
						// Console.WriteLine ("Final cardinality: {0}, startT: {1}, endT: {2}", count, startT, endT);
						if (count >= T) {
							// we have a match
							docs.Add (p);
							card.Add (count);
						}
						this.SkipInSortedLists (p, posting, startT, endT - 1, false);
					} else if (count > 0) {
						Console.WriteLine ("count > 0");
					}
				}
			}
		}
	}
}

