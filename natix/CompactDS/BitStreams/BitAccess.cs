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
	public partial class BitAccess
	{

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
