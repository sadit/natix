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
//   Original filename: natix/CompactDS/Bitmaps/SArray.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	/// <summary>
	/// sarray. ALENEX 2007. Okanohara & Sadakane. Practical Rank & Select.
	/// </summary>
	public class SArray : RankSelectBase
	{
		int N;
		public IRankSelect H;
		public ListIFS L;

		public override void AssertEquality (IRankSelect obj)
		{
			var other = obj as SArray;
			if (this.N != other.N) {
				throw new ArgumentException (String.Format ("SArray.N inequality. this.N {0}, other.N: {1}",
						this.N, other.N));
			}
			this.H.AssertEquality (other.H);
			Assertions.AssertIList<int> (this.L, other.L, "SArray.L");
		}
	
		public override int Count1 {
			get {
				return this.L.Count;
			}
		}
		
		public int GetNumLowerBits ()
		{
			return this.L.Coder.NumBits;
			//return this.NumLowerBits;
		}
		
		public override int Count {
			get {
				return this.N;
				// return this.L.Count;
			}
		}
				
		public SArray ()
		{
		}
		
		int get_mask ()
		{
			return (1 << this.GetNumLowerBits()) - 1;
		}
		
		public void Build (IList<int> orderedList, int n, byte numLowerBits, BitmapFromBitStream H_builder = null)
		{
			//this.M = orderedList.Count;
			int M = orderedList.Count;
			this.N = n;
			if (M > this.N) {
				Console.WriteLine ("XXXXX LastItem: {0}", orderedList [orderedList.Count - 1]);
				throw new ArgumentOutOfRangeException (String.Format ("SArray N < M, N: {0}, M: {1}", this.N, M));
			}
			if (numLowerBits < 1) {
				numLowerBits = 1;
			}
			// this.NumLowerBits = numLowerBits;
			this.L = new ListIFS (numLowerBits, new BitStream32 ((numLowerBits / 32) * M));
			// Creating bitmaps
			// 2^ (log N - log N / M) = 2^ \log N M / N = M.
			// 2^ (log N - log N / M) = 2^ \log N M / N = M.
			int numpart = (int)Math.Ceiling (Math.Pow (2, (Math.Ceiling (Math.Log (this.N)) - this.GetNumLowerBits ())));
			var BH = new BitStream32 (M + (numpart / 32 + 1));
			int mask = this.get_mask ();
			int prevblock = -1;
			for (int i = 0; i < M; i++) {
				this.L.Add (orderedList [i] & mask);
				int currentblock = orderedList [i] >> this.GetNumLowerBits ();
				if (prevblock != currentblock) {
					while (prevblock < currentblock) {
						BH.Write (false);
						prevblock++;
					}
				}
				BH.Write (true);
			}
			//an additional technical zero
			BH.Write (false, M - prevblock);
			BH.Write (false);
			if (H_builder == null) {
				H_builder = BitmapBuilders.GetGGMN_wt(20);
			}
			//Console.WriteLine ("==== BH.CountBits: {0}, N: {1}, M: {2}, numLowerBits: {3}, BH: {4}", BH.CountBits, N, M, numLowerBits, BH);
			var fb = new FakeBitmap(BH);
			this.H = H_builder(fb);
		}
		
		public static byte Log_N_over_M (int n, int m)
		{
			return (byte)Math.Ceiling( Math.Log(n * 1.0 / m, 2) );
		}
	
		public void Build (IList<int> orderedList, int n = 0, BitmapFromBitStream H_builder = null)
		{
			if (n == 0 && orderedList.Count > 0) {
				n = orderedList[orderedList.Count - 1] + 1;
			}
			byte z = Log_N_over_M(n, orderedList.Count);
			if (z == 0) {
				z++;
			}
			// Console.WriteLine("n: {0}, m: {1}, z: {2}", n, orderedList.Count, z);
			this.Build( orderedList, n, z, H_builder);
		}


		public void Build (IBitStream bitmap, BitmapFromBitStream H_builder = null)
		{
			IList<int> L = new List<int> ();
			for (int i = 0; i < bitmap.CountBits; i++) {
				if (bitmap[i]) {
					L.Add (i);
				}
			}
			this.Build (L, (int)bitmap.CountBits, H_builder);
		}
		
		public override void Save (BinaryWriter bw)
		{
			bw.Write ((int)this.N);
			RankSelectGenericIO.Save(bw, this.H);
			this.L.Save (bw);
		}
		
		public override void Load (BinaryReader br)
		{
			this.N = br.ReadInt32 ();
			this.H = RankSelectGenericIO.Load(br);
			var list = new ListIFS ();
			list.Load (br);
			this.L = list;
		}
		
		public override bool Access (int i)
		{
			int rank = this.Rank1 (i);
			return i == this.Select1 (rank);
		}
		
		public override int Rank1 (int pos)
		{
			if (pos < 0 || this.Count1 == 0) {
				return 0;
			}
			int rank0_prev = 1 + (pos >> this.GetNumLowerBits ());
			int pos_prev = this.H.Select0 (rank0_prev);
			// Remember that $pos = rank0 + rank1 - 1$, thus $rank1 = pos - rank0 + 1$
			//int rank1_prevX = this.H.Rank1 (pos_prev); // prevcount = rank1
			int rank1_prev = pos_prev - rank0_prev + 1;
			//int rank1_nextX = this.H.Rank1 (this.H.Select0 (rank0_prev + 1));
			int pos_next = this.H.Select0 (rank0_prev + 1);
			int rank1_next = pos_next - rank0_prev;
			uint pos_masked = (uint)(this.get_mask () & pos);
			// Console.WriteLine ("xxxxx {0}", nextcount - prevcount);
			int count = rank1_next - rank1_prev;
			if (count < 128) {
				// if (true) {
				// sequential search
				int rank = rank1_prev;
				for (int i = 0; i < count; i++) {
					var u = this.L [rank];
					rank++;
					if (u >= pos_masked) {
						if (u > pos_masked) {
							rank--;
						}
						break;
					}
				}
				return rank;
			} else {
				// binary search
				return 1 + GenericSearch.FindLast<int> ((int)pos_masked, this.L, rank1_prev, rank1_next);
			}
		}
		
		public override int Select1 (int rank)
		{
			if (rank <= 0) {
				return -1;
			}
			int pos_rank = this.H.Select1 (rank);
			// int high_weight = this.H.Rank0 (pos_rank) - 1;
			int high_weight = pos_rank - rank;
			return (high_weight << this.GetNumLowerBits ()) | ((int)this.L [rank - 1]);
		}

		public int Select1_UnraveledSymbol (int rank, ref int pos_rank)
		{
			if (rank <= 0) {
				return -1;
			}
			if (pos_rank == int.MinValue) {
				pos_rank = this.H.Select1 (rank);
			}
			// int high_weight = this.H.Rank0 (pos_rank) - 1;
			int high_weight = pos_rank - rank;
			return (high_weight << this.GetNumLowerBits ()) | ((int)this.L [rank - 1]);
		}

		public IList<int> GetAsIList ()
		{
			return new ListGen<int> (delegate(int i) {
				return this.Select1 (i + 1);
			}, this.Count1);
		}

	}
}
