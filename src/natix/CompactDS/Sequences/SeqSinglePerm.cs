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
//   Original filename: natix/CompactDS/Sequences/GolynskiSinglePermSeq.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class SeqSinglePerm : Sequence
	{
		public IPermutation PERM;
		public Bitmap LENS;
		
		public int Count {
			get {
				return this.PERM.Count;
			}
		}

		public int Sigma {
			get {
				// we write an additional 1 to the end
				return this.LENS.Count1 - 1;
			}
		}

		public SeqSinglePerm ()
		{
		}
		
		public void Build (IList<int> seq, int sigma, PermutationBuilder perm_builder, BitmapFromBitStream bitmap_builder)
		{
			// A counting sort construction of the permutation
			var counters = new int[sigma];
			foreach (var s in seq) {
				if (s + 1 < sigma) {
					counters [s + 1]++;
				}
			}
			for (int i = 1; i < sigma; i++) {
				counters [i] += counters [i - 1];
			}
			var n = seq.Count;
			var P = new int[n];
			for (int i = 0; i < n; i++) {
				var sym = seq [i];
				var pos = counters [sym];
				P [pos] = i;
				counters [sym] = pos + 1;
			}
			// the bitmap to save the lengths
			var lens = new BitStream32 ();
			int prevc = 0;
			foreach (var c in counters) {
				var len = c - prevc;
				prevc = c;
				lens.Write (true);
				lens.Write (false, len);
			}
			// an additional 1 to the end, to simplify source code
			lens.Write (true);

			var bb_lens = new FakeBitmap (lens);
			this.LENS = bitmap_builder(bb_lens);
			this.PERM = perm_builder(P);
		}
		
		void PrintArray (string msg, IList<int> P)
		{
			Console.WriteLine (msg);
			foreach (var x in P) {
				Console.Write (x.ToString () + ",");
			}
			Console.WriteLine ("<end>");

		}

		public int Access (int pos)
		{
			
			var inv = this.PERM.Inverse (pos);
            // var index = this.LENS.Select0 (inv+1);
			// var abs_symbol = this.LENS.Rank1 (index) - 1;
            //return abs_symbol;
            var rank0 = inv+1;
            var seqpos = this.LENS.Select0(rank0);
            // rank0 + rank1 = pos + 1, then: rank1 = pos + 1 - rank0
            var rank1  = seqpos + 1 - rank0;
            return rank1 - 1;
		}
		
		public int Select (int symbol, int abs_rank)
		{
			if (abs_rank < 1) {
				return -1;
			}
			symbol++;
			var pos = this.LENS.Select1 (symbol);
			var rank0 = pos + 1 - symbol;
			return this.PERM [abs_rank + rank0 - 1];
		}

		public int Rank (int symbol, int pos)
		{
			if (pos < 0) {
				return 0;
			}
			symbol++;
			var pos_start = this.LENS.Select1 (symbol);
			var rank0_start = pos_start + 1 - symbol;
			var pos_end = this.LENS.Select1 (symbol + 1);
			var rank0_end = pos_end - symbol;			
			var count = rank0_end - rank0_start;
			// TODO: replace both methods by RL2 primitives to produce a faster rank
			if (count < 32) {
				// fast sequential access for small ranges
				for (int i = 0; i < count; i++) {
					var u = this.PERM [rank0_start + i];
					if (u > pos) {
						return i;
					}
					if (u == pos) {
						return i+1;
					}
				}
				return count;
			} else {
				var list = new ListShiftIndex<int> (this.PERM, rank0_start, count);
				return 1 + GenericSearch.FindFirst<int> (pos, list);
			}
		}
		
		public Bitmap Unravel (int symbol)
		{
			return new UnraveledSymbolSSP (this, symbol);
		}
		
		public void Load (BinaryReader Input)
		{
			this.PERM = GenericIO<IPermutation>.Load (Input);
			this.LENS = GenericIO<Bitmap>.Load (Input);
		}
		
		public void Save (BinaryWriter Output)
		{
			GenericIO<IPermutation>.Save (Output, this.PERM);
			GenericIO<Bitmap>.Save (Output, this.LENS);
		}


        public class UnraveledSymbolSSP : UnraveledSymbol
        {
            SeqSinglePerm SEQ;
            int start_index;

            public UnraveledSymbolSSP(SeqSinglePerm seq, int sym) : base(seq, sym)
            {
                this.SEQ = seq;
                // this.symbol = sym;
                ++sym;
                var pos = seq.LENS.Select1 (sym);
                var rank0 = pos + 1 - sym;
                this.start_index = rank0 - 1;
            }

            public override int Select1(int abs_rank)
            {
                if (abs_rank < 1) {
                    return -1;
                }
                return this.SEQ.PERM[abs_rank + this.start_index];
            }           
        }
	}
}
