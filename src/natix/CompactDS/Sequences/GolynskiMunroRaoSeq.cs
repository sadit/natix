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
//   Original filename: natix/CompactDS/Sequences/GolynskiMunroRaoSeq.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class GolynskiMunroRaoSeq : Sequence
	{
		CyclicPerms_MRRR[] perms;
		//IPermutation[] perms;
		Bitmap B; // counter in blocks
		Bitmap X; // counter per symbol
		// IList<int> Xacc; // accumulated rank for each row in X
		int sigma;
		int n;
		// not saved
		int num_blocks;

		void compute_num_blocks ()
		{
			this.num_blocks = (int)Math.Ceiling (n * 1.0 / this.sigma);
		}

		public int Count {
			get {
				return this.n;
			}
		}

		public int Sigma {
			get {
				return this.sigma;
			}
		}
	
		public GolynskiMunroRaoSeq ()
		{
		}
		
		void Print1Perm (IPermutation p)
		{
			for (int i = 0; i < p.Count; ++i) {
				Console.Write ("<{0}>, ", p.Direct(i));
			}
			Console.WriteLine ("<END>");
		}
		
		void PrintPerms ()
		{
			for (int i = 0; i < this.perms.Length; i++) {
				Console.WriteLine ("===== permutation of block {0}", i);
				this.Print1Perm (this.perms [i]);
				Console.WriteLine ();
			}
		}

		public void BuildPermInvIndex (IList<int> seq, int start_index, int sigma, List<int>[] lists)
		{
			// Console.WriteLine ("XXXXXXXXXX seq.Count: {0}, sigma: {1}, lists.Count: {2}", seq.Count, sigma, lists.Length);
			// for (int i = 0; i < sigma; i++) {	lists [i].Clear (); }
			foreach (var list in lists) {
				// all must be cleared, not just the sigma (simplifies the last block)
				list.Clear ();
			}
			for (int i = 0; i < sigma; i++) {
				var symbol = seq [i + start_index];
				// Console.WriteLine ("XXX symbol: {0}", symbol);
				lists [symbol].Add (i);
			}
		}

		public void Build (IList<int> seq, int sigma, BitmapFromBitStream bitmap_builder, int cyclic_perm_t)
		{
			// NOTE: Please check sigma <=> BlockSize in this method
			this.sigma = sigma;
			this.n = seq.Count;
			var B_stream = new BitStream32 ();
			var X_stream = new BitStream32[ sigma ];
			for (int i = 0; i < sigma; i++) {
				X_stream [i] = new BitStream32 ();
			}
			var lists = new List<int>[sigma];
			for (int i = 0; i < sigma; i++) {
				lists [i] = new List<int> ();
			}
			int num_blocks = (int)Math.Ceiling (this.n * 1.0 / this.sigma);
			//this.perms = new IPermutation[num_blocks];
			this.perms = new CyclicPerms_MRRR[num_blocks];
			for (int i = 0, I = 0; i < this.n; i+= this.sigma, ++I) {
				// writing block separators
				foreach (var b in X_stream) {
					b.Write (true);
				}
				// clearing perm B
				// selecting block size 
				int s = Math.Min (this.n - i, this.sigma);
				this.BuildPermInvIndex (seq, i, s, lists);
				var P = new List<int> (s);
				for (int j = 0; j < this.sigma; j++) {
					var c = lists [j].Count;
					B_stream.Write (false);
					if (c > 0) {
						X_stream [j].Write (false, c);
						B_stream.Write (true, c);
						foreach (var u in lists[j]) {
							P.Add (u);
						}
					}
				}
				//var _perm = perm_builder(P);
				//this.perms[I] = _perm;
				this.perms [I] = (CyclicPerms_MRRR)PermutationBuilders.GetCyclicPermsListIFS(cyclic_perm_t).Invoke (P);
			}
			var _X_stream = X_stream [0];
			
			for (int i = 1; i < X_stream.Length; i++) {
				var _X_curr = X_stream [i];
				for (int j = 0; j < _X_curr.CountBits; j++) {
					// esto se podria hace por entero en lugar de bit
					_X_stream.Write (_X_curr [j]);
				}
			}
			// If we write a zero at the end of the streams the code is simplified
			_X_stream.Write (true);
			B_stream.Write (false);
			this.B = bitmap_builder (new FakeBitmap (B_stream));
			this.X = bitmap_builder (new FakeBitmap (_X_stream));
			this.compute_num_blocks ();
		}
		
		protected int GetBlockRank (int symbol, int block_id)
		{
			int rank1 = this.num_blocks * symbol + block_id + 1;
			int pos = this.X.Select1 (rank1);
			int rank0 = pos - rank1 + 1;
			var m = symbol*this.num_blocks + 1;
			return rank0 - this.X.Select1(m) + m - 1;
		}

		public int Access (int pos)
		{
			var block_id = pos / this.sigma;
			var inv = this.perms [block_id].Inverse (pos % this.sigma);
			var abs_symbol = this.B.Rank0 (this.B.Select1 (block_id * this.sigma + inv + 1));
			// Console.WriteLine ("abs_symbol: {0}", abs_symbol);
			return (abs_symbol-1) % this.sigma;
		}
		
		public int Select (int symbol, int abs_rank)
		{
			if (abs_rank < 1) {
				return -1;
			}
			var m = symbol*this.num_blocks+1;
			int rank0_X = this.X.Select1(m) - m + 1 + abs_rank;
			int block_id = this.X.Rank1 (this.X.Select0 (rank0_X)) - 1;
			block_id = block_id % this.num_blocks;
			int rel_rank = abs_rank - this.GetBlockRank (symbol, block_id);
			// now, it looks for the desired symbol in the container block
			var sp_block = block_id * this.sigma;
			var rank0_B = sp_block + symbol + 1;
			var sp = this.B.Select0 (rank0_B);
			// rank1 + rank0 - 1 = sp
			var rank1_B = sp - rank0_B + 1;
			// access to the desired select position, relative to this block
			var s = this.perms [block_id].Direct( (rank1_B % this.sigma) + rel_rank - 1);
			s += sp_block;
			return s;
		}
	
		public int Rank (int symbol, int pos)
		{
			if (pos < 0) {
				return 0;
			}
			// determines the container block
			int block_id = pos / this.sigma;
			// Console.WriteLine ("============== Rank> symbol: {0}, pos: {1}, block_id: {2}", symbol, pos, block_id);
			int block_rank = 0;
			if (block_id > 0) {
				block_rank = this.GetBlockRank (symbol, block_id);
			}
			int rank0 = block_id * this.sigma + symbol + 1;
			var sp = this.B.Select0 (rank0);
			var ep = this.B.Select0 (rank0 + 1);
			int rel_rank = 0;
			int len = ep - sp - 1;
			if (len > 0) {
				int rank1 = sp - rank0 + 1;
				//var list = new ListShiftIndex<int> (this.perms [block_id], sp % this.sigma, len);
				var list = new ListShiftIndex<int> (this.perms [block_id], rank1 % this.sigma, len);
				rel_rank = 1 + GenericSearch.FindFirst<int> (pos % this.sigma, list);
			}
			/*Console.WriteLine ("=== rank0: {0}, sp: {1}, ep: {2}, block_rank: {3}, perm_sp: {4}, perm_ep: {5}, len: {6}, rel_rank: {7}",
				rank0, sp, ep, block_rank, sp % this.sigma, ep - sp - 1, len, rel_rank);			*/
			return block_rank + rel_rank;
		}
		
		public Bitmap Unravel (int symbol)
		{
			return new UnraveledSymbol (this, symbol);
		}
		
		public void Load (BinaryReader Input)
		{
			this.n = Input.ReadInt32 ();
			this.sigma = Input.ReadInt32 ();
			var c = Input.ReadInt32 ();
			this.perms = new CyclicPerms_MRRR[c];
			for (int i = 0; i < c; i++) {
				this.perms [i] = GenericIO<CyclicPerms_MRRR>.Load (Input);
			}
			this.B = GenericIO<Bitmap>.Load (Input);
			this.X = GenericIO<Bitmap>.Load (Input);
			/*var len = Input.ReadInt32 ();
			this.Xacc = new int[len];
			PrimitiveIO<int>.ReadFromFile (Input, len, this.Xacc);*/
			this.compute_num_blocks ();
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.n);
			Output.Write ((int)this.sigma);
			Output.Write ((int)this.perms.Length);
			foreach (var p in this.perms) {
				GenericIO<CyclicPerms_MRRR>.Save (Output, p);
			}
			GenericIO<Bitmap>.Save (Output, this.B);
			GenericIO<Bitmap>.Save (Output, this.X);
			/*Output.Write ((int)this.Xacc.Count);
			PrimitiveIO<int>.WriteVector (Output, this.Xacc);*/
		}
		
	}
}
