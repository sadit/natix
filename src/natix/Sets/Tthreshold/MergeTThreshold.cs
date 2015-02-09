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
//   Original filename: natix/Sets/Tthreshold/MergeTThreshold.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.Sets
{
	
	public class MergeTThreshold : ITThresholdAlgorithm
	{
		public int CompCounter {
			get;
			set;
		}
		public MergeTThreshold ()
		{
		}
		
		int ShiftFirst (IList<IList<int>> PostingLists, SplayTree<int> pqueue, int[] Start)
		{
			int pIndex = pqueue.RemoveFirst ();
			var startIndex = Start[pIndex];
			int docid = PostingLists[pIndex][startIndex];
			startIndex++;
			if (startIndex < PostingLists[pIndex].Count) {
				Start[pIndex] = startIndex;
				pqueue.Add (pIndex);
			}
			return docid;
		}
		
		public void SearchTThreshold (IList<IList<int>> PostingLists, int T, out IList<int> docs, out IList<short> card)
		{
			// K - T = Number of mismatches (errors)
			int suggestedCardinality = 1024;
			docs = new List<int> (suggestedCardinality);
			card = new List<short> (suggestedCardinality);
			int docsI = 0;
			var Start = new int[PostingLists.Count];
			Comparison<int> comptop = delegate(int x, int y) {
				this.CompCounter++;
				return PostingLists[x][Start[x]] - PostingLists[y][Start[y]];
			};
			SplayTree<int> pqueue = new SplayTree<int> (comptop);
			for (int i = 0; i < PostingLists.Count; i++) {
				pqueue.Add (i);
			}
			docs.Add (ShiftFirst (PostingLists, pqueue, Start));
			card.Add (1);
			while (pqueue.Count > 0) {
				int docid = ShiftFirst (PostingLists, pqueue, Start);
				this.CompCounter++;
				if (docid == docs[docsI]) {
					card[docsI]++;
				} else {
					if (card[docsI] < T) {
						docs[docsI] = docid;
						card[docsI] = 1;
					} else {
						docs.Add (docid);
						card.Add (1);
						docsI++;
					}
				}
			}
			// The last item docs[docsI] can holds vals[docsI] < T
			// Console.WriteLine ("===> cards: ");
			if (card[docsI] < T) {
				card.RemoveAt (docsI);
				docs.RemoveAt (docsI);
			}
		}
	}
}

