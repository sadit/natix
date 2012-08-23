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
//   Original filename: natix/CompactDS/Bitmaps/DArray.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class DArray : IRankSelect
	{
		GGMN BaseIndex;
		IList<int> PosAbs;
		GGMN IsLargeBlock;
		IList<int> SavedPos;
		int B;
		
		public DArray ()
		{
		}
		
		public int Count {
			get {
				 return this.BaseIndex.Count;
			}
		}
		
		public int Count1 {
			get {
				return this.Rank1 (this.Count - 1);
			}
		}
		
		public bool Access(int i)
		{
			return this.BaseIndex.Access(i);
		}
		
		public void AssertEquality (IRankSelect obj)
		{
			var other = obj as DArray;
			this.BaseIndex.AssertEquality (other.BaseIndex);
			this.IsLargeBlock.AssertEquality (other.IsLargeBlock);
			Assertions.AssertIList<int> (this.PosAbs, other.PosAbs, "DArray.PosAbs");
			Assertions.AssertIList<int> (this.SavedPos, other.SavedPos, "DArray.SavedPos");
			if (this.B != other.B) {
				throw new ArgumentException ("DArray inequality on B");
			}
		}
		
		public void Build (IList<int> orderedList, short Brank, int Bselect)
		{
			int n = 0;
			if (orderedList.Count > 0) {
				n = orderedList[orderedList.Count - 1] + 1;
			}
			this.Build (orderedList, n, Brank, Bselect);
		}

		public void Build (IList<int> orderedList, int N, short Brank, int Bselect)
		{
			BitStream32 b = new BitStream32 ();
			PlainSortedList s = new PlainSortedList ();
			s.Build (orderedList, (int)N);
			for (int i = 0; i < N; i++) {
				b.Write (s[i]);
			}
			this.Build (b, Brank, Bselect);
		}
		
		public void Build (IBitStream bstream, short Brank, int Bselect)
		{
			this.BuildBackend (bstream.GetIList32 (), Brank, Bselect, (int)bstream.CountBits);
		}
		
		public void BuildBackend (IList<uint> bitblocks, short Brank, int Bselect, int N)
		{
			this.BaseIndex = new GGMN ();
			this.BaseIndex.BuildBackend (bitblocks, N, Brank);
			this.B = Bselect;
			int M = this.BaseIndex.Rank1 (N - 1);
			// Console.WriteLine("XXXX N: {0}, M: {1}", N, M);
			this.PosAbs = new int[(int)Math.Ceiling (M * 1.0 / this.B)];
			int m = 0;
			int i = 0;
			int large_limit = 32 * this.B;
			BitStream32 mark_bits = new BitStream32 (this.PosAbs.Count >> 5);
			int num_saved_slots = 0;
			while (m < M) {
				int start_pos = this.BaseIndex.Select1 (m + 1);
				int Bmin = Math.Min (this.B, M - m);
				int end_pos = this.BaseIndex.Select1 (m + Bmin);
				if (end_pos - start_pos >= large_limit) {
					mark_bits.Write (true);
					num_saved_slots += Bmin;
				} else {
					mark_bits.Write (false);
					// uncomment to make all large blocks (debugging). Comment the previous line to enable to following
					// mark_bits.Write (true);
					// num_saved_slots += Bmin;
				}
				this.PosAbs[i] = start_pos;
				m += this.B;
				i++;
			}
			this.IsLargeBlock = new GGMN ();
			this.IsLargeBlock.Build (mark_bits, 4);
			int R = this.IsLargeBlock.Rank1 (this.IsLargeBlock.Count - 1);
			this.SavedPos = new int[num_saved_slots];
			for (int k = 0; k < R; k++) {
				int index = this.IsLargeBlock.Select1 (k + 1);
				int rank_base = this.B * index;
				int index_base = this.B * k;
				int maxB = Math.Min (this.B, M - rank_base);
				for (int rank_rel = 0; rank_rel < maxB; rank_rel++) {
					var pos = this.BaseIndex.Select1( rank_rel + rank_base + 1);
					this.SavedPos[rank_rel + index_base] = pos;
				}
			}
		}
		
		public int Select1 (int rank)
		{
			if (rank < 1) {
				return -1;
			}
			int index = (rank - 1)/ this.B;
			int start_bit = this.PosAbs[ index ];
			int rank_diff = (rank - 1) - index * this.B;
			if (this.IsLargeBlock[index]) {
				index = (this.IsLargeBlock.Rank1(index) - 1) * this.B + rank_diff ;
				return this.SavedPos[ index ]; 
			} else {
				if (rank_diff == 0) {
					return start_bit;
				}
				int max_count_bits = this.BaseIndex.Count - start_bit;
				return BitAccess.Select1BitArgs( this.BaseIndex.GetBitBlocks(), start_bit+1, max_count_bits, rank_diff);
			}
		}

		public int Rank0 (int I)
		{
			return this.BaseIndex.Rank0 (I);
		}

		public int Rank1 (int I)
		{
			return this.BaseIndex.Rank1 (I);
		}

		public int Select0 (int rank)
		{
			return this.BaseIndex.Select0 (rank);
		}
		
		public void Save (BinaryWriter bw)
		{
			this.Save (bw, true);
		}
		
		public void Save (BinaryWriter bw, bool save_bitmap)
		{
			bw.Write ((int)this.B);
			bw.Write ((int)this.PosAbs.Count);
			bw.Write ((int)this.SavedPos.Count);
			PrimitiveIO<int>.WriteVector (bw, this.PosAbs);
			PrimitiveIO<int>.WriteVector (bw, this.SavedPos);
			this.IsLargeBlock.Save (bw);
			this.BaseIndex.Save (bw, save_bitmap);
		}
		
		public void Load (BinaryReader br)
		{
			this.Load (br, true);
		}
		
		public void Load (BinaryReader br, bool load_bitmap)
		{
			this.B = br.ReadInt32 ();
			int posabslen = br.ReadInt32 ();
			int expposlen = br.ReadInt32 ();
			this.PosAbs = new int[posabslen];
			this.SavedPos = new int[expposlen];
			PrimitiveIO<int>.ReadFromFile (br, posabslen, this.PosAbs);
			PrimitiveIO<int>.ReadFromFile (br, expposlen, this.SavedPos);
			this.IsLargeBlock = new GGMN ();
			this.IsLargeBlock.Load (br);
			this.BaseIndex = new GGMN ();
			this.BaseIndex.Load (br, load_bitmap);
		}
		
		public IList<uint> GetBitBlocks ()
		{
			return this.BaseIndex.GetBitBlocks ();
		}
		
		public void SetBitBlocks (IList<uint> bitblocks)
		{
			this.BaseIndex.SetBitBlocks (bitblocks);
		}
	}
}

