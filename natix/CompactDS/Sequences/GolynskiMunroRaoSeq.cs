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
	public class GolynskiMunroRaoSeq : IRankSelectSeq
	{
		IList<IPermutation> perms;
		IRankSelect B; // counter in blocks
		IRankSelect X; // counter per symbol
		IList<int> Xacc; // accumulated rank for each row in X
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
			this.PermBuilder = PermutationBuilders.GetSuccCyclicPerms (16);
			this.BitmapBuilder = BitmapBuilders.GetGGMN_wt (16);
		}
		
		void Print1Perm (IList<int> p)
		{
			foreach (var u in p) {
				Console.Write ("<{0}>, ", u);
			}
			Console.WriteLine ("<END>");
		}
		
		void PrintPerms ()
		{
			for (int i = 0; i < this.perms.Count; i++) {
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

		public void Build (IList<int> seq, int sigma)
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
			this.perms = new List<IPermutation> (num_blocks);
			for (int i = 0; i < this.n; i+= this.sigma) {
				// writing block separators
				foreach (var b in X_stream) {
					b.Write (false);
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
						X_stream [j].Write (true, c);
						B_stream.Write (true, c);
						foreach (var u in lists[j]) {
							P.Add (u);
						}
					}
				}
				var _perm = this.PermBuilder (P);
				this.perms.Add (_perm);
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
			_X_stream.Write (false);
			B_stream.Write (false);
			this.B = this.BitmapBuilder (new FakeBitmap (B_stream));
			this.X = this.BitmapBuilder (new FakeBitmap (_X_stream));
			this.Xacc = new List<int> ();
			for (int i = 0; i < this.sigma; i++) {
				int acc_rank = this.X.Rank1 (this.X.Select0 (i * num_blocks + 1));
				this.Xacc.Add (acc_rank);
			}
			this.compute_num_blocks ();
		}
		
		public PermutationBuilder PermBuilder {
			get;
			set;
		}
		
		public BitmapFromBitStream BitmapBuilder {
			get;
			set;
		}
		
		
		protected int GetBlockRank (int symbol, int block_id)
		{
			int rank0 = this.num_blocks * symbol + block_id + 1;
			int pos = this.X.Select0 (rank0);
			int rank1 = pos - rank0 + 1;
			//Console.WriteLine ("get-block-rank> symbol: {0}, block_id: {1}, acc-symbol: {2}, rank0: {3}, rank1: {4}, pos: {5}",
			//	symbol, block_id, this.Xacc [symbol], rank0, rank1, pos);
			return rank1 - this.Xacc [symbol];
		}

		public int Access (int pos)
		{
			var block_id = pos / this.sigma;
			var inv = this.perms [block_id].Inverse (pos % this.sigma);
			var abs_symbol = this.B.Rank0 (this.B.Select1 (block_id * this.sigma + inv + 1));
			return (abs_symbol % this.sigma) - 1;
		}
		
		public int Select (int symbol, int abs_rank)
		{
			if (abs_rank < 1) {
				return -1;
			}
			int rank1_X = this.Xacc [symbol] + abs_rank;
			int block_id = this.X.Rank0 (this.X.Select1 (rank1_X)) - 1;
			block_id = block_id % this.num_blocks;
			int rel_rank = abs_rank - this.GetBlockRank (symbol, block_id);
			// now, it looks for the desired symbol in the container block
			var sp_block = block_id * this.sigma;
			var rank0_B = sp_block + symbol + 1;
			var sp = this.B.Select0 (rank0_B);
			// rank1 + rank0 - 1 = sp
			var rank1_B = sp - rank0_B + 1;
			// access to the desired select position, relative to this block
			var s = this.perms [block_id] [(rank1_B % this.sigma) + rel_rank - 1];
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
			/*BitStream32 C = new BitStream32 ((this.B as GGMN).GetBitBlocks ());
			BitStream32 D = new BitStream32 ((this.X as GGMN).GetBitBlocks ());
			Console.WriteLine ("B> " + C.ToString ());
			Console.WriteLine ("X> " + D.ToString ());*/
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
		
		public IRankSelect Unravel (int symbol)
		{
			return new UnraveledSymbol (this, symbol);
		}
		
		public void Load (BinaryReader Input)
		{
			this.n = Input.ReadInt32 ();
			this.sigma = Input.ReadInt32 ();
			var c = Input.ReadInt32 ();
			this.perms = new IPermutation[c];
			for (int i = 0; i < c; i++) {
				this.perms [i] = PermutationGenericIO.Load (Input);
			}
			this.B = RankSelectGenericIO.Load (Input);
			this.X = RankSelectGenericIO.Load (Input);
			var len = Input.ReadInt32 ();
			this.Xacc = new int[len];
			PrimitiveIO<int>.ReadFromFile (Input, len, this.Xacc);
			this.compute_num_blocks ();
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.n);
			Output.Write ((int)this.sigma);
			Output.Write ((int)this.perms.Count);
			foreach (var p in this.perms) {
				PermutationGenericIO.Save (Output, p);
			}
			RankSelectGenericIO.Save (Output, this.B);
			RankSelectGenericIO.Save (Output, this.X);
			Output.Write ((int)this.Xacc.Count);
			PrimitiveIO<int>.WriteVector (Output, this.Xacc);
		}
		
	}
}
