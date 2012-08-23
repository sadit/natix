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
//   Original filename: natix/CompactDS/BitStreams/BitAccess64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class BitAccess64
	{
		// Overload for uint
		public static bool GetBit (IList<UInt64> bitmap, long i)
		{
			int shift_right = (int) (i & 63);
			return (((bitmap[(int)(i >> 6)] >> shift_right) & 1) != 0);
		}

		public static void SetBit (IList<UInt64> bitmap, long i)
		{
			int quotient = (int)(i >> 6);
			UInt64 b = bitmap[quotient];
			b |= 1ul << ((int)(i & 63));
			bitmap[quotient] = b;
		}

		public static void ResetBit (IList<UInt64> bitmap, long i)
		{
			int quotient = (int)(i >> 6);
			UInt64 b = bitmap [quotient];
			b &= ~(1ul << ((int)(i & 63)));
			bitmap [quotient] = b;
		}
		

		public static int Select1 (UInt64 bitmap, int rank)
		{
			int popcount = 0;
			int pos = 0;
			while (pos < 64) {
				popcount = Bits.PopCount8[bitmap & 255];
				if (popcount >= rank) {
					break;
				}
				rank -= popcount;
				bitmap >>= 8;
				pos += 8;
			}
			for (int i = 0; i < 8; ++i, ++pos) {
				if ((bitmap & 1) == 1) {
					--rank;
					if (rank == 0) {
						break;
					}
				}
				bitmap >>= 1;
			}
			if (pos > 63) {
				throw new ArgumentOutOfRangeException ("BitAccesss uint Select1 for an undefined value");
			}
			return pos;
		}
		
		public static int Select1 (IList<UInt64> bitmap, int start, int atmostcount, int rank)
		{
			//Console.WriteLine ("--BitAccess.Select1 start: {0}, atmostcount: {1}, rank: {2}, array-size: {3}",
			//	start, atmostcount, rank, bitmap.Count);
			if (rank == 0) {
				return -1;
			}
			int r = rank;
			// int s = start;
			int pos = 0;
			for (atmostcount += start; start < atmostcount; ++start) {
				UInt64 b = bitmap[start];
				int popcount = Rank1 (b, 63);
				if (popcount >= rank) {
					/*Console.WriteLine ("XXXXX-> pos: {0}, b: {1}, popcount: {2}, rank: {3}, bits: {4}",
						pos, b, popcount, rank, BinaryHammingSpace.ToAsciiString(b));
						*/
					return pos + Select1 (b, rank);
				}
				rank -= popcount;
				pos += 64;
			}
			/*for (; s < count; s++) {
				Console.WriteLine("s: {0}, bits: {1}", s, BinaryHammingSpace.ToAsciiString(bitmap[s]));
			}*/
			throw new ArgumentOutOfRangeException (String.Format(
					"BitAccess.Select1 rank: {0}. There are {1} items outside of the bounds", r, rank));
		}
		
		public static int Rank1 (UInt64 bitmap, int bitpos)
		{
			if (bitpos < 0) {
				return 0;
			}
			bitpos++;
			int popcount = 0;
			while (bitpos > 8) {
				popcount += Bits.PopCount8[bitmap & 255];
				bitpos -= 8;
				bitmap >>= 8;
			}
			popcount += Bits.PopCount8[bitmap & ((1ul << bitpos) - 1)];
			return popcount;
		}

		public static int Rank1 (IList<UInt64> bitmap, int start, int count, int bitpos)
		{
			// Console.WriteLine ("--BitAccess.Rank1 start: {0}, count: {1}, bitexcess: {2}", start, count, bitpos);
			int popcount = 0;
			UInt64 u;
			for (count += start; start < count; start++) {
				u = bitmap[start];
				popcount += Bits.PopCount8[u & 255];
				popcount += Bits.PopCount8[(u >> 8) & 255];
				popcount += Bits.PopCount8[(u >> 16) & 255];
				popcount += Bits.PopCount8[(u >> 24) & 255];
				popcount += Bits.PopCount8[(u >> 32) & 255];
				popcount += Bits.PopCount8[(u >> 40) & 255];
				popcount += Bits.PopCount8[(u >> 48) & 255];
				popcount += Bits.PopCount8[(u >> 56) & 255];
			}
			if (count < bitmap.Count && bitpos >= 0) {
				popcount += Rank1 (bitmap[count], bitpos);
			}
			return popcount;
		}
		
		// Overloads for IList<byte>
		public static bool GetBit (IList<byte> bitmap, int i)
		{
			return (((bitmap[i >> 3] >> (i & 7)) & 1) != 0);
		}

		public static void SetBit (IList<byte> bitmap, int i)
		{
			byte b = bitmap[i >> 3];
			b |= (byte)(1 << (i & 7));
			bitmap[i >> 3] = b;
		}

		public static void ResetBit (IList<byte> bitmap, int i)
		{
			byte b = bitmap[i >> 3];
			b &= (byte)(~(1 << (i & 7)));
			bitmap[i >> 3] = b;
		}
		
		// Overloads for int
		public static bool GetBit (UInt64 bitmap, int i)
		{
			return (((bitmap >> i) & 1) != 0);
		}

		public static UInt64 SetBit (UInt64 bitmap, int i)
		{
			return bitmap | (1ul << i);
		}
		
		public static UInt64 ResetBit (UInt64 bitmap, int i)
		{
			return bitmap & (~(1ul << i));
		}
		
		public static UInt64 AssignBit (UInt64 bitmap, int i, bool x)
		{
			if (x) {
				return SetBit (bitmap, i);
			} else {
				return ResetBit (bitmap, i);
			}
		}
		
		public static UInt64 Reverse (UInt64 bitmap, int size)
		{
			bool tmp;
			int pos;
			int mid = size >> 1;
			for (int i = 0; i < mid; i++) {
				tmp = GetBit (bitmap, i);
				pos = size - i - 1;
				bitmap = AssignBit (bitmap, i, GetBit (bitmap, pos));
				bitmap = AssignBit (bitmap, pos, tmp);
			}
			return bitmap;
		}
		
		public static int Log2 (UInt64 u)
		{
			// another option for this function is computing using tables,
			// hopping for registers.
			int log2 = 0;
			while (u > 0) {
				u >>= 1;
				++log2;
			}
			return log2;
		}
	}
}
