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
//   Original filename: natix/CompactDS/Sequences/InvIndexSketchBuilder.cs
// 
using System;
using System.Collections.Generic;
using natix;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class InvIndexSketchBuilder
	{
		bool PqueueHasNext (SplayTree<int> pqueue, int next, int[] pos, IList<IList<int>> invindex)
		{
			return invindex[next][pos[next]] + 1 == pqueue.GetFirstWithSplay ();
		}
		
		IList< IList<int> > invindex;
		int len;
		IList<int> pos;
		// PQueueSL<int> pqueue;
		Comparison<int> cmp_pos;
		PQueueSL<int>[] pqueue_array;
		int msd_block;
		int lsd_block;
		
		public InvIndexSketchBuilder (IList<IList<int>> _invindex, int len)
		{
			this.invindex = _invindex;
			this.len = len;
			var numbits = (int)Math.Ceiling(Math.Log (this.len, 2));
			this.msd_block = (int)Math.Ceiling(numbits / 1.5);
			this.lsd_block  = numbits - this.msd_block;
			int alphabet_size = this.invindex.Count;
			this.pos = new int[alphabet_size];
			this.cmp_pos = delegate(int x, int y) {
				x = this.invindex[x][pos[x]];
				y = this.invindex[y][pos[y]];
				return x.CompareTo (y);
			};
			this.pqueue_array = new PQueueSL<int>[1 << msd_block];
			int num_blocks = 1 << msd_block;
			for (int i = 0; i < num_blocks; i++) {
				//this.pqueue_array[i] = new PQueueSL<int> (invindex.Count, this.cmp_pos);
				this.pqueue_array[i] = new PQueueSL<int> (this.cmp_pos);
			}
		}
		
		void PushInvList (int index)
		{
			var ilist = this.invindex[index];
			var ipos = pos[index];
			if (ipos < ilist.Count) {
				// Console.WriteLine ("n: {0}, ipos: {1}", ilist.Count, ipos);
				int key = ilist[ipos] >> this.lsd_block;
				this.pqueue_array[key].Push (index);
			}
		}
		
		int __num_pops = 0;
		int PopInvList ()
		{
			var pqueue = this.pqueue_array[this.__num_pops >> this.lsd_block];
			var index = pqueue.Pop ();
			this.__num_pops++;
			/*if (pqueue.Count == 0) {
				this.pqueue_array[__currentBlock] = null;
				__currentBlock++;
			}*/
			return index;
		}

		public void Build (IList<int> sketch, int alphabet_block)
		{
			int alphabet_size = invindex.Count;
			for (int i = 0; i < alphabet_size; i++) {
				this.PushInvList (i);
			}
			for (int i = 0; i < len; i++) {
				int current = this.PopInvList ();
				// var invlist = this.invindex[current];
				// Console.Write (voc[current].ToString ());
				this.pos[current]++;
				//if (this.pos[current] < invlist.Count) {
					this.PushInvList (current);
				//}
				current /= alphabet_block;
				sketch.Add (current);
				if (i % 1048576 == 0) {
					Console.WriteLine ("sketch pos: {0}, text-len: {1}, advance: {2:0.00}%", i, len, i * 100.0 / len);
				}
			}
		}
	}
}

