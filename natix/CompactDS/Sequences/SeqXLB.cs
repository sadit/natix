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
//   Original filename: natix/CompactDS/Sequences/SeqXLB.cs
// 

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	/// <summary>
	/// An InvIndexSeq implemented with a single (sparse, SArray64) eXtra Large bitmap
	/// </summary>
	public class SeqXLB : IRankSelectSeq
	{
		IRankSelect64 xl_bitmap;
		int sigma;
		IPermutation perm;
			
		public SeqXLB ()
		{
		}
		
		public void Build (IList<int> seq, int sigma, int t = 16, BitmapFromList64 bitmap_builder = null)
		{
			this.sigma = sigma;
			long n = seq.Count;
			var L = new long[n];
			
			var counters = new int[sigma + 1];
			// counters.Add (0, sigma); <- if ListIFS
			for (int i = 0; i < n; ++i) {
				var sym = seq [i];
				try {
					counters [sym + 1] += 1;
				} catch (Exception e) {
					Console.WriteLine ("===== sigma: {0}, sym: {1}, i: {2}, n: {3}", sigma, sym, i, n);
					throw e;
				}
			}
			for (int i = 1; i <= sigma; ++i) {
				counters [i] += counters [i - 1];
			}
			for (int i = 0; i < n; ++i) {
				var sym = seq [i];
				long long_sym = n * ((long)sym) + i;
				L [counters [sym]] = long_sym;
				counters [sym] += 1;
			}
			counters = null;
			/**** slow construction ****/
			/*****
			for (int i = 0; i < n; ++i) {
				long s = seq [i];
				L [i] = n * s + i;
				//L.Add (n * s + i);
			}
			Array.Sort (L);
			*****/
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetSArray64 ();
			}
			this.xl_bitmap = bitmap_builder (L, n * sigma);
			// now building the permutation for access
			var p = new ListGen_MRRR ();
			p.Build (this.GetNotIdxPERM (), t, null);
			this.perm = p;
		}
		
		public int Count {
			get {
				return (int)this.xl_bitmap.Count1;
			}
		}
		

		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.sigma);
			RankSelect64GenericIO.Save (Output, this.xl_bitmap);
			this.perm.Save (Output);
		}
		
		public void Load (BinaryReader Input)
		{
			this.sigma = Input.ReadInt32 ();
			this.xl_bitmap = RankSelect64GenericIO.Load (Input);
			var p = new ListGen_MRRR ();
			p.Load (Input);
			p.SetPERM (this.GetNotIdxPERM ());
			this.perm = p;
		}
		
		public IList<int> GetNotIdxPERM ()
		{
			long n = this.Count;
			var gen = new ListGen<int> (delegate(int i) {
				return (int)(this.xl_bitmap.Select1 (i + 1) % n);
			}, (int)n);
			return gen;
		}
		
		public IPermutation GetPERM ()
		{
			return this.perm;
		}
		
		public int Sigma {
			get {
				return this.sigma;
			}
		}
		
		// deprecated
		public int AccessSequential (int pos)
		{
			// DONE: Convert this into a permutation index using the golynski large perm. scheme
			long n = this.Count;
			for (int i = 0; i < this.sigma; ++i) {
				if (this.xl_bitmap.Access (i * n + pos)) {
					return i;
				}
			}
			throw new ArgumentOutOfRangeException ();
		}
		
		public int Access (int pos)
		{
			var i = this.perm.Inverse (pos);
			return (int)(this.xl_bitmap.Select1 (i + 1) / this.Count);
		}

		public int Rank (int symbol, int _pos)
		{
			if (_pos < 0) {
				return 0;
			}
			if (symbol == 0) {
				return (int)this.xl_bitmap.Rank1 (_pos);
			}
			long pos = symbol * ((long)this.Count);
			var rank_a = this.xl_bitmap.Rank1 (pos + _pos);
			var rank_b = this.xl_bitmap.Rank1 (pos - 1);
			return (int)(rank_a - rank_b);
		}

		public int Select (int symbol, int _rank)
		{
			if (_rank < 1) {
				return -1;
			}
			if (symbol == 0) {
				return (int)this.xl_bitmap.Select1 (_rank);
			}
			long pos = symbol * ((long)this.Count);
			var rank = this.xl_bitmap.Rank1 (pos - 1);
			var p = this.xl_bitmap.Select1 (rank + _rank) - pos;
			return (int)p;
		}
		
		public int Rank (int symbol, int _pos, UnraveledSymbolXLB unraveled_ctx)
		{
			if (_pos < 0) {
				return 0;
			}
			if (symbol == 0) {
				return (int)this.xl_bitmap.Rank1 (_pos);
			}
			long pos = symbol * ((long)this.Count);
			if (unraveled_ctx.prevrank == int.MinValue) {
				unraveled_ctx.prevrank = (int)this.xl_bitmap.Rank1 (pos - 1);
			}
			var rank_a = this.xl_bitmap.Rank1 (pos + _pos);
			return (int)(rank_a - unraveled_ctx.prevrank);
		}

		public int Select (int symbol, int _rank, UnraveledSymbolXLB unraveled_ctx)
		{
			if (_rank < 1) {
				return -1;
			}
			if (symbol == 0) {
				return (int)this.xl_bitmap.Select1 (_rank);
			}
			long pos = symbol * ((long)this.Count);
			if (unraveled_ctx.prevrank == int.MinValue) {
				unraveled_ctx.prevrank = (int)this.xl_bitmap.Rank1 (pos - 1);
			}
			var p = this.xl_bitmap.Select1 (unraveled_ctx.prevrank + _rank, unraveled_ctx) - pos;
			return (int)p;
		}
		
		public IRankSelect Unravel (int symbol)
		{
			return new UnraveledSymbolXLB(this, symbol);
			// return new UnraveledSymbol (this, symbol);
		}

        public IList<int> GetRawSeq ()
        {
            return RankSelectSeqGenericIO.ToIntArray(this, false);
        }

	}
}