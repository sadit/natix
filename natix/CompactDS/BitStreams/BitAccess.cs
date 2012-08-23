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
//   Original filename: natix/CompactDS/BitStreams/BitAccess.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// Bit access methods
	/// </summary>
	public class BitAccess
	{
		/// <summary>
		/// Gets the (i+1)th bit.
		/// </summary>
		public static bool GetBit (IList<UInt32> bitmap, long i)
		{
			uint b = bitmap [(int)(i >> 5)];
			int shift_right = (int)(i & 31);
			return (((b >> shift_right) & 1) != 0);
		}
		
		/// <summary>
		/// Enables the (i+1)th bit.
		/// </summary>
		public static void SetBit (IList<UInt32> bitmap, long i)
		{
			int quotient = (int)(i >> 5);
			uint b = bitmap [quotient];
			b |= 1u << ((int)(i & 31));
			bitmap [quotient] = b;
		}
		
		/// <summary>
		/// Resets the (i+1)th bit.
		/// </summary>
		public static void ResetBit (IList<UInt32> bitmap, long i)
		{
			int quotient = (int)(i >> 5);
			uint b = bitmap [quotient];
			b &= ~(1u << ((int)(i & 31)));
			bitmap [quotient] = b;
		}
		
		/// <summary>
		/// Select of the (rank) th the specified bitmap, rank and rankout.
		/// </summary>
		public static int Select1 (uint bitmap, int rank, out int rankout)
		{
			int popcount = 0;
			int pos = 0;
			while (pos < 32) {
				popcount = Bits.PopCount8 [bitmap & 255];
				if (popcount >= rank) {
					break;
				}
				rank -= popcount;
				bitmap >>= 8;
				pos += 8;
			}
			for (int i = 0; i < 8; ++i,++pos) {
				if ((bitmap & 1) == 1) {
					--rank;
					if (rank == 0) {
						break;
					}
				}
				bitmap >>= 1;
			}
			rankout = rank;
			return pos;			
		}
		
		/// <summary>
		/// Select1 of rank
		/// </summary>
		public static int Select1 (uint bitmap, int rank)
		{
			int rankout;
			int pos = Select1 (bitmap, rank, out rankout);
			if (rankout != 0) {
				throw new ArgumentOutOfRangeException ("BitAccesss uint Select1 for an undefined value");
			}
			return pos;

		}
		
		/// <summary>
		/// Select1 of rank, the bit sequences is bitmap. The search starts in start_bit, reviewing atmost max_count_bits.
		/// </summary>
		public static int Select1BitArgs (IList<uint> bitmap, int start_bit, int max_count_bits, int rank)
		{
			// Console.WriteLine ("************** AAA start_bit: {0}, max_count_bits: {1}, rank: {2}", start_bit, max_count_bits, rank);
			if (rank < 1) {
				return -1;
			}
			int start = start_bit >> 5;
			// int start_remainder = start_bit - (start << 5);
			int start_remainder = start_bit & 31;
			int rel_rank;
			int pos = Select1 (bitmap [start] >> start_remainder, rank, out rel_rank);
			if (rel_rank == 0) {
				// Console.WriteLine ("*************** A");
				return start_bit + pos;
			}
			// Console.WriteLine ("************** B rel_rank: {0} ", rel_rank);
			int at_most_count = (max_count_bits - start_remainder) >> 5;
			start++;
			return (start << 5) + Select1 (bitmap, start, at_most_count + 1, rel_rank);
		}
		
		/// <summary>
		/// Select1 of rank in bitmap. The boundaries are start and start+atmostcount
		/// </summary>
		public static int Select1 (IList<uint> bitmap, int start, int atmostcount, int rank)
		{
			//Console.WriteLine ("--BitAccess.Select1 start: {0}, atmostcount: {1}, rank: {2}, array-size: {3}",
			//	start, atmostcount, rank, bitmap.Count);
			if (rank == 0) {
				return -1;
			}
			int r = rank;
			// int s = start;
			int pos = 0;
			//Console.WriteLine ("BitAccess.Select1 bitmap.Count: {0}, start: {1}, atmostcount: {2}, rank: {3}",
			//	bitmap.Count, start, atmostcount, rank);
			for (atmostcount += start; start <= atmostcount; ++start) {
				uint b = bitmap [start];
				int popcount = Rank1 (b, 31);
				if (popcount >= rank) {
					/*Console.WriteLine ("XXXXX-> pos: {0}, b: {1}, popcount: {2}, rank: {3}, bits: {4}",
						pos, b, popcount, rank, BinaryHammingSpace.ToAsciiString(b));
						*/
					return pos + Select1 (b, rank);
				}
				rank -= popcount;
				pos += 32;
			}
			/*for (; s < count; s++) {
				Console.WriteLine("s: {0}, bits: {1}", s, BinaryHammingSpace.ToAsciiString(bitmap[s]));
			}*/
			throw new ArgumentOutOfRangeException (String.Format (
					"BitAccess.Select1 rank: {0}. There are {1} items outside of the bounds", r, rank));
		}
		
		/// <summary>
		/// Returns Rank1(bitpos) at bitmap
		/// </summary>
		public static int Rank1 (uint bitmap, int bitpos)
		{
			if (bitpos < 0) {
				return 0;
			}
			bitpos++;
			int popcount = 0;
			while (bitpos > 8) {
				popcount += Bits.PopCount8 [bitmap & 255];
				bitpos -= 8;
				bitmap >>= 8;
			}
			popcount += Bits.PopCount8 [bitmap & ((1 << bitpos) - 1)];
			return popcount;
		}

		/// <summary>
		/// Returns Rank1(bitpos) at bitmap using boundaries start, start+count
		/// </summary>
		
		public static int Rank1 (IList<UInt32> bitmap, int start, int count, int bitpos)
		{
			// Console.WriteLine ("--BitAccess.Rank1 start: {0}, count: {1}, bitexcess: {2}", start, count, bitpos);
			int popcount = 0;
			uint u;
			for (count += start; start < count; start++) {
				u = bitmap[start];
				popcount += Bits.PopCount8[u & 255];
				popcount += Bits.PopCount8[(u >> 8) & 255];
				popcount += Bits.PopCount8[(u >> 16) & 255];
				popcount += Bits.PopCount8[(u >> 24) & 255];
			}
			if (count < bitmap.Count && bitpos >= 0) {
				popcount += Rank1 (bitmap[count], bitpos);
			}
			return popcount;
		}
		
		// Overloads using bytes
		
		/// <summary>
		/// Gets the (i+1)th bit. Overload using bytes.
		/// </summary>
		public static bool GetBit (IList<byte> bitmap, int i)
		{
			return (((bitmap [i >> 3] >> (i & 7)) & 1) != 0);
		}

		/// <summary>
		/// Enalbes the (i+1)th bit.
		/// </summary>
		public static void SetBit (IList<byte> bitmap, int i)
		{
			byte b = bitmap [i >> 3];
			b |= (byte)(1 << (i & 7));
			bitmap [i >> 3] = b;
		}
		
		/// <summary>
		/// Resets the (i+1)th bit.  Overload using bytes.
		/// </summary>
		public static void ResetBit (IList<byte> bitmap, int i)
		{
			byte b = bitmap[i >> 3];
			b &= (byte)(~(1 << (i & 7)));
			bitmap[i >> 3] = b;
		}
		
		/// <summary>
		/// Gets the (i+1)th bit. Overload using int as bitmap
		/// </summary>
		public static bool GetBit (int bitmap, int i)
		{
			return (((bitmap >> i) & 1) != 0);
		}

		/// <summary>
		/// Enables the (i+1)th bit. Overload using int as bitmap
		/// </summary>
		
		public static int SetBit (int bitmap, int i)
		{
			return bitmap | (1 << i);
		}
		
		/// <summary>
		/// Reset the (i+1)th bit. Overload using int as bitmap
		/// </summary>

		public static int ResetBit (int bitmap, int i)
		{
			return bitmap & (~(1 << i));
		}
		
		/// <summary>
		/// Assign the (i+1)th bit to x. Overload using int as bitmap
		/// </summary>

		public static int AssignBit (int bitmap, int i, bool x)
		{
			if (x) {
				return SetBit (bitmap, i);
			} else {
				return ResetBit (bitmap, i);
			}
		}
		
		/// <summary>
		/// Reverse the first size bits inside the bitmap
		/// </summary>
		public static int Reverse (int bitmap, int size)
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
		
		/// <summary>
		/// floor(Log2) of the specified u.
		/// </summary>
		public static int Log2 (int u)
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

		public static int Log2 (long u)
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

		/// <summary>
		/// Prints u as a bit string
		/// </summary>
		public static string ToAsciiString (uint u)
		{
			StringWriter w = new StringWriter ();
			for (int i = 0; i < 32; i++) {
				if ((u & 1) == 1) {
					w.Write ("1");
				} else {
					w.Write ("0");
				}
				u >>= 1;
			}
			return w.ToString ();
		}
		
		/// <summary>
		/// Prints u as a bit string
		/// </summary>
		public static string ToAsciiString (ulong u)
		{
			StringWriter w = new StringWriter ();
			for (int i = 0; i < 64; i++) {
				if ((u & 1) == 1) {
					w.Write ("1");
				} else {
					w.Write ("0");
				}
				u >>= 1;
			}
			return w.ToString ();
		}

	}
}
