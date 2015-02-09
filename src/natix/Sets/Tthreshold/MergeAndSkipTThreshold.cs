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
//   Original filename: natix/Sets/Tthreshold/MergeAndSkipTThreshold.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	
	public class MergeAndSkipTThreshold : ITThresholdAlgorithm
	{
		ISearchAlgorithm<int> SearchAlgorithm;
		
		public int CompCounter {
			get;
			set;
		}
		public MergeAndSkipTThreshold (ISearchAlgorithm<int> searchalg)
		{
			this.SearchAlgorithm = searchalg;
		}
		
		public void SearchTThreshold (IList<IList<int>> PostingLists, int T, out IList<int> docs, out IList<short> card)
		{
			// K - T = Number of mismatches (errors)
			int suggestedCardinality = 1024;
			docs = new List<int> (suggestedCardinality);
			card = new List<short> (suggestedCardinality);
			// ***** INIT SPLAYTREE PQUEUE
			int[] First = new int[PostingLists.Count];
			Comparison<int> comptop = delegate(int x, int y) {
				this.CompCounter++;
				return PostingLists[x][First[x]] - PostingLists[y][First[y]];
			};
			SplayTree<int> pqueue = new SplayTree<int> (comptop);
			for (int i = 0; i < PostingLists.Count; i++) {
				pqueue.Add (i);
			}
			// ***** STARTING METHOD
			List<int> eqstack = new List<int> (PostingLists.Count);
			int head_pIndex = pqueue.RemoveFirst ();
			int head_startIndex = First[head_pIndex];
			var head_postingList = PostingLists[head_pIndex];
			int head_docid = head_postingList[head_startIndex];
			head_startIndex++;
			if (head_startIndex < head_postingList.Count) {
				First[head_pIndex] = head_startIndex;
				eqstack.Add (head_pIndex);
			}
			short current_count = 1;
			while (pqueue.Count + current_count >= T) {
				Console.WriteLine ("xxxx> pqueue.Count: {0}", pqueue.Count);
				int inner_pIndex = pqueue.RemoveFirst ();
				int inner_startIndex = First[inner_pIndex];
				var inner_postingList = PostingLists[inner_pIndex];
				int inner_docid = inner_postingList[inner_startIndex];
				inner_startIndex++;
				First[inner_pIndex] = inner_startIndex;
				if (head_docid == inner_docid) {
					if (inner_startIndex < inner_postingList.Count) {
						eqstack.Add (inner_pIndex);
					}
					current_count++;
					if (pqueue.Count == 0) {
						docs.Add (head_docid);
						card.Add (current_count);
						foreach (var u_pIndex in eqstack) {
							pqueue.Add (u_pIndex);
						}
						eqstack.Clear ();
						current_count = 0;
					}
				} else {
					if (current_count < T) {
						foreach (var u_pIndex in eqstack) {
							// Insert into the queue but set Start[u_pIndex] to the insertion rank of inner_docid
							int u_startIndex;
							var u_postingList = PostingLists[u_pIndex];
							this.SearchAlgorithm.Search (inner_docid, u_postingList, out u_startIndex, First[u_pIndex], u_postingList.Count);
							// if true we increment_counter and perform an internal iteration
							if (u_startIndex < u_postingList.Count) {
								First[u_pIndex] = u_startIndex;
								pqueue.Add (u_pIndex);
							}
						}
					} else {
						docs.Add (head_docid);
						card.Add (current_count);
						// Console.WriteLine ("--------------> head_docid: {0}, current_count: {1}", head_docid, current_count);
						foreach (var u_pIndex in eqstack) {
							pqueue.Add (u_pIndex);
						}
					}
					eqstack.Clear ();
					if (inner_startIndex < inner_postingList.Count) {
						eqstack.Add (inner_pIndex);
					}
					head_docid = inner_docid;
					head_pIndex = inner_pIndex;
					head_startIndex = inner_startIndex;
					head_postingList = inner_postingList;
					current_count = 1;
				}
			}
		}
	}
}

