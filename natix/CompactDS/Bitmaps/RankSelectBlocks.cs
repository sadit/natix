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
//   Original filename: natix/CompactDS/Bitmaps/RankSelectBlocks.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class RankSelectBlocks : RankSelectBase
	{
		/// <summary>
		/// The bitmap to index rank, select, access.
		/// *** Description:
		/// Parametrized solution.
		/// *** Time:
		/// Depending on parameters, smallest O(1).
		/// *** Storage:
		/// n + o(n)
		/// </summary>
		IList<uint> BitBlocks;
		int N;
		/// <summary>
		/// Absolute values
		/// </summary>
		int[] SuperBlocks;
		/// <summary>
		/// Relative values to absrank
		/// </summary>
		short[] Blocks;
		/// <summary>
		/// How many items per relative register.
		/// </summary>
		short BlockSize;
		/// <summary>
		/// How many relatives per absolute register.
		/// </summary>
		short SuperBlockSize;
		int bits_per_super_block;
		int bits_per_block;
		
		public override void AssertEquality (IRankSelect obj)
		{
			var other = obj as RankSelectBlocks;
			if (this.SuperBlockSize != other.SuperBlockSize) {
				throw new ArgumentException (String.Format ("RankSelectBlocks.SuperBlockSize inequality"));
			}
			if (this.BlockSize != other.BlockSize) {
				throw new ArgumentException (String.Format ("RankSelectBlocks.SuperBlockSize inequality"));
			}
			if (this.bits_per_block != other.bits_per_block) {
				throw new ArgumentException (String.Format ("RankSelectBlocks.bits_per_block inequality"));
			}
			if (this.bits_per_super_block != other.bits_per_super_block) {
				throw new ArgumentException (String.Format ("RankSelectBlocks.bits_per_super_block inequality"));
			}
			Assertions.AssertIList<short> (this.Blocks, other.Blocks, "RankSelectBlocks.Blocks");
			Assertions.AssertIList<int> (this.SuperBlocks, other.SuperBlocks, "RankSelectBlocks.SuperBlocks");
			Assertions.AssertIList<uint> (this.BitBlocks, other.BitBlocks, "RankSelectBlocks.BitBlocks");
			if (this.N != other.N) {
				throw new ArgumentException ("RankSelectBlocks.N inequality");
			}
		}

		public override int Count {
			get { return this.N; }
		}

		public override bool this[int i] {
			get { return BitAccess.GetBit (this.BitBlocks, i); }
		}

		public override void Save (BinaryWriter bw)
		{
			bw.Write (this.SuperBlockSize);
			bw.Write (this.BlockSize);
			bw.Write (this.SuperBlocks.Length);
			PrimitiveIO<int>.WriteVector (bw, this.SuperBlocks);
			bw.Write (this.Blocks.Length);
			PrimitiveIO<short>.WriteVector (bw, this.Blocks);
			bw.Write(this.BitBlocks.Count);
			PrimitiveIO<uint>.WriteVector (bw, this.BitBlocks);
			bw.Write (this.N);
		}

		public override void Load (BinaryReader reader)
		{
			short sbsize = reader.ReadInt16 ();
			short bsize = reader.ReadInt16 ();
			int numsuperblocks = reader.ReadInt32 ();
			var sblocks = new int[numsuperblocks];
			PrimitiveIO<int>.ReadFromFile (reader, numsuperblocks, sblocks);
			int numblocks = reader.ReadInt32 ();
			var blocks = new short[numblocks];
			PrimitiveIO<short>.ReadFromFile (reader, numblocks, blocks);
			var numbitblocks = reader.ReadInt32 ();
			var bitblocks = new uint[numbitblocks];
			PrimitiveIO<uint>.ReadFromFile (reader, numbitblocks, bitblocks);
			var numbits = reader.ReadInt32 ();
			this.SetState (bitblocks, numbits, sbsize, bsize, sblocks, blocks);
		}		

		void SetState (IList<uint> bitblocks, int numbits, short superBlockSize, short blockSize,
			int[] superBlocks, short[] blocks)
		{
			this.BitBlocks = bitblocks;
			this.N = numbits;
			this.SuperBlockSize = superBlockSize;
			this.BlockSize = blockSize;
			this.bits_per_block = this.BlockSize * 32;
			this.bits_per_super_block = this.SuperBlockSize * this.bits_per_block;
			this.Blocks = blocks;
			this.SuperBlocks = superBlocks;
		}
		
		public RankSelectBlocks ()
		{
		}
		
		public RankSelectBlocks (BitStream32 bitmap, short superBlockSize, short blockSize)
		{
			this.SetState (bitmap.GetIList32(), (int)bitmap.CountBits, superBlockSize, blockSize, null, null);
			// Checking ranges of our data types
			int bs = this.BitBlocks.Count;
			this.SuperBlocks = new int[bs / (this.SuperBlockSize * this.BlockSize)];
			this.Blocks = new short[bs / this.BlockSize - this.SuperBlocks.Length];
			int abs = 0;
			short rel = 0;
			//Console.WriteLine ("rel-rank: {0}, abs-rank: {1}, bitmap-ints: {2}", this.Blocks.Length, this.SuperBlocks.Length, this.Bitmap.CountUInt32);
			var Lbuffer = bitmap.GetIList32 ();
			for (int relindex = 0, absindex = 0, index = 0, relcounter = this.SuperBlockSize - 1;
				relindex <= this.Blocks.Length;
				index += this.BlockSize,relcounter--) {
				int count = Math.Min (this.BlockSize, bitmap.Count32 - index);
				// Console.WriteLine ("index: {0}, count: {1}, block-size: {2}, super-block-size: {3}, bitmap-uintsize: {4}",
				//	index, count, this.BlockSize, this.SuperBlockSize, bitmap.CountUInt32);
				rel += (short)this.SeqRank1 (Lbuffer, index, count, -1);
				// Console.WriteLine ("absindex: {0}, abs: {1}, relcounter", absindex, abs, relcounter);
				if (relcounter == 0) {
					abs += rel;
					if (absindex < this.SuperBlocks.Length) {
						// Console.WriteLine ("========> absindex: {0}, abs: {1}, rel: {2}", absindex, abs, rel);
						this.SuperBlocks[absindex] = abs;
					}
					relcounter = this.SuperBlockSize;
					absindex++;
					rel = 0;
				} else {
					if (relindex < this.Blocks.Length) {
						this.Blocks[relindex] = rel;
					}
					relindex++;
				}
			}
			
			/*Console.WriteLine ("---- Precomputed tables");
			Console.WriteLine ("rel-rank: {0}, abs-rank: {1}, bitmap-ints: {2}",
				this.Blocks.Length, this.SuperBlocks.Length, this.Bitmap.CountUInt32);
			Console.WriteLine ("---- Absolute values");
			for (int i = 0; i < this.SuperBlocks.Length; i++) {
				Console.WriteLine ("TABLE ABS i: {0}, abs_rank: {1}, bit-position: {2}",
					i, this.SuperBlocks[i], 32 * (i + 1) * this.BlockSize * this.SuperBlockSize);
			}
			Console.WriteLine ("---- Relative values");
			for (int i = 0; i < this.Blocks.Length; i++) {
				Console.WriteLine ("TABLE REL i: {0}, rel_rank: {1}, bit-position: {2}",
					i, this.Blocks[i], (i + 1) * 32 * this.BlockSize);
			}*/
		}
		
		protected virtual int SeqRank1 (IList<uint> B, int index, int count, int bitpos)
		{
			return BitAccess.Rank1 (B, index, count, bitpos);
		}

		protected virtual int SeqSelect1 (IList<uint> B, int index, int count, int rank)
		{
			return BitAccess.Select1 (B, index, count, rank);
		}

		public override int Rank1 (int bit_index)
		{
			if (bit_index < 0) {
				return 0;
			}
			int container_index = (bit_index) >> 5;
			int start_index = (container_index) / this.BlockSize;
			int abs_index = start_index / this.SuperBlockSize;
			int rel_index = start_index;
			int sba_index = abs_index;
			start_index *= this.BlockSize;
			// the whole thing is to floor in blocksize blocks
			int acc_rank = 0;
			if (abs_index > 0) {
				acc_rank += this.SuperBlocks[abs_index - 1];
				abs_index *= this.SuperBlockSize;
			}
			if (rel_index > abs_index) {
				acc_rank += this.Blocks[rel_index - sba_index - 1];
			}
			var uuu = this.SeqRank1(this.BitBlocks, start_index, container_index - start_index,
				(bit_index & 31));
			acc_rank += uuu;
			return acc_rank;
		}
		
		int SelectBackend (int I, IList<int> AbsRank, IList<short> RelRank, IList<uint> bitmap)
		{
			if (I < 1) {
				return -1;
			}
			// 00R00R00R00A00R00R00R00A00R00R00R00A
			//   0  1  2     3  4  5     6  7  8
			//            0           1           2
			// 012345678901234567890123456789012345
			// X         X         X         X     
			// TODO: check selects for out of bounds
			int abs_pos = GenericSearch.FindFirst<int> (I, AbsRank, 0, AbsRank.Count);
			int pos = 0;
			int min = 0;
			if (abs_pos >= 0 && I == AbsRank[abs_pos]) {
				abs_pos--;
			}
			// Console.WriteLine ("ABS I: {0}, AbsRank.Count: {1}, abs_pos: {2}",
			//    I, AbsRank.Count, abs_pos);

			if (abs_pos >= 0) {
				pos = (abs_pos + 1) * this.SuperBlockSize;
				I -= AbsRank[abs_pos];
				min = pos - abs_pos - 1;
			}
			int diff = 0;
			{
				int max = Math.Min (min + this.SuperBlockSize - 1, RelRank.Count);
				int rel_pos = GenericSearch.FindFirst<short> ((short)I, RelRank, min, max);
				if (rel_pos >= 0 && I == RelRank[rel_pos]) {
					rel_pos--;
				}
				// Console.WriteLine ("REL I: {0}, RelRank.Count: {1}, min: {2}, max: {3}, rel_pos: {4}",
				// 	I, RelRank.Count, min, max, rel_pos);
				if (rel_pos >= min) {
					diff = rel_pos + 1 - min;
					I -= RelRank[rel_pos];
				}
			}
			pos += diff;
			pos *= this.BlockSize;
			if (I > 0) {
				if (pos == 0) {
					return this.SeqSelect1 (bitmap, 0, this.BlockSize, I);
				} else {
					return (pos << 5) + this.SeqSelect1 (bitmap, pos, this.BlockSize, I);
				}
			} else {
				return pos << 5;
			}
		}

		public override int Select1 (int I)
		{
			return this.SelectBackend (I, this.SuperBlocks, this.Blocks, this.BitBlocks);
		}

		int _Rank0_Abs (int i)
		{
			// return (i + 1) * this.bits_per_super_block - this.SuperBlocks[i];
			return (i+1) * this.bits_per_super_block - this.SuperBlocks[i];
		}

		short _Rank0_Rel (int i)
		{
			// 00R00A00R00A00R00A00
			//   0     1     2 
			//      0     1     2
			// 00R00R00A00R00R00A00
			//   0  1     2  3
			//         0        1  
			// 00R00R00R00A00R00R00R00A00R00R00R00A
			//   0  1  2     3  4  5     6  7  8
			//            0           1           2
			// 012345678901234567890123456789012345
			// X         X         X         X     
			//
			int a = (i % (this.SuperBlockSize - 1)) + 1;
			return (short)( a * this.bits_per_block - this.Blocks[i]);
		}
		
		ListGen<int> AbsRankComp;
		ListGen<short> RelRankComp;
		ListGen<uint> BitmapComp;
		
		public override int Select0 (int I)
		{
			if (this.AbsRankComp == null) {
				this.AbsRankComp = new ListGen<int> ((int i) => _Rank0_Abs (i), this.SuperBlocks.Length);
				this.RelRankComp = new ListGen<short> ((int i) => _Rank0_Rel (i), this.Blocks.Length);
				this.BitmapComp = new ListGen<uint> ((int i) => ~this.BitBlocks[i], this.BitBlocks.Count);
			}
			return this.SelectBackend (I, this.AbsRankComp, this.RelRankComp, this.BitmapComp);
		}
	}
}
