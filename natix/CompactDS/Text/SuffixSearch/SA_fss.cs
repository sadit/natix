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
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class SA_fss
	{
		public IList<int> TXT;
        public SkipListRank<int>[] Char_SA;
        public SkipList2<SkipListRank<int>.DataRank>.Node[] SA_pointers;
        public int[] A;
        public IRankSelect newF;
        public IList<int> charT;
		// int alphabet_numbits;
	
		public SA_fss (IList<int> text, int alphabet_size)
        {
            this.TXT = text;
            var n = text.Count;
            // this.alphabet_numbits = ListIFS.GetNumBits(alphabet_size);
            // this.SA = new int[n];
            //this.Char_Offsets = new int[alphabet_size];
            this.Char_SA = new SkipListRank<int>[alphabet_size];
            var cmp_fun = new Comparison<int> (this.compare_suffixes);
            for (int i = 0; i < alphabet_size; ++i) {
                this.Char_SA [i] = new SkipListRank<int> (cmp_fun);
            }
            this.SA_pointers = new SkipList2<SkipListRank<int>.DataRank>.Node[n];
            for (int suffixID = this.TXT.Count-1; suffixID >= 0; --suffixID) {
                var c = this.TXT [suffixID];
                var list = this.Char_SA [c];
                //Console.WriteLine ("=== adding: {0} ({1})", c, Convert.ToChar(c));
                var p = list.Add (suffixID);
                this.SA_pointers [suffixID] = p;
            }
            this.A = new int[n+1];
            this.A[0] = n;
            int I = 1;
            foreach (var SLR in this.Char_SA) {
                foreach (var data in SLR.SKIPLIST.Traverse()) {
                    this.A[I] = data.Data;
                    ++I;
                }
            }
            this.SA_pointers = null;
            var stream = new BitStream32();
            this.charT = new List<int>();
            stream.Write(true); // $ symbol
            this.charT.Add(0);
            for (int i = 0; i < alphabet_size; ++i) {
                var count = this.Char_SA[i].Count;
                if (count > 0) {
                    stream.Write(true);
                    stream.Write(false, count-1);
                    this.charT.Add(i+1);
                }
                this.Char_SA[i] = null;
            }
            this.Char_SA = null;
            this.newF = BitmapBuilders.GetGGMN_wt(12).Invoke(new FakeBitmap(stream));
		}
		
        /// <summary>
        /// compare two suffixes, the new one and some already stored.
        /// cmp =  0 if TXT[new_suffix] == TXT[stored_suffix] (impossible for this suffix sorting algorithm)
        /// cmp =  1 if TXT[new_suffix] &gt; TXT[stored_suffix]
        /// cmp = -1 if TXT[new_suffix] &le; TXT[stored_suffix]
        /// </summary>

        protected int _compare_suffixes (int new_suffix, int old_suffix)
        {
            var n = this.TXT.Count;
            if (old_suffix + 1 == n) {
                return 1; // stored suffix is lex. smaller
            }
            var new_c = this.TXT [new_suffix + 1];
            var old_c = this.TXT [old_suffix + 1];
            if (new_c == old_c) {
                var next_new = this.Char_SA[new_c].Rank (this.SA_pointers[new_suffix+1]);
                var next_old = this.Char_SA[old_c].Rank (this.SA_pointers[old_suffix+1]);
                return next_new.CompareTo(next_old);
            } else {
                return new_c.CompareTo(old_c);
            }
        }

        protected int compare_suffixes (int new_suffix, int stored_suffix)
        {
            if (new_suffix < stored_suffix) {
                return this._compare_suffixes (new_suffix, stored_suffix);
            } else {
                return -this._compare_suffixes (stored_suffix, new_suffix);
            }
        }

	}
}

