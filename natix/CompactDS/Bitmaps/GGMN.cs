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
//   Original filename: natix/CompactDS/Bitmaps/GGMN.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class GGMN : RankSelectBase
	{
		/// <summary>
		/// The bitmap to index rank, select, access.
		/// *** Description:
		/// Gonzalez, Grabowski, M\"akinen and Navarro. WEA2005.
		/// *** Time:
		/// Depending on parameters.
		/// *** Storage:
		/// n + o(n)
		/// </summary>
		protected IList<uint> BitBlocks;
		/// <summary>
		/// Absolute values
		/// </summary>
		protected IList<uint> Abs;
		/// <summary>
		/// How many uint items exists per absolute register.
		/// </summary>
		protected short B;
		/// <summary>
		///  The length of the bitmap.
		/// </summary>
		int N;
		
		public IList<uint> GetBitBlocks ()
		{
			return this.BitBlocks;
		}

		public void SetBitBlocks (IList<uint> bit_blocks)
		{
			this.BitBlocks = bit_blocks;
		}
		
		public override void AssertEquality (IRankSelect obj)
		{
			var other = obj as GGMN;
			if (this.N != other.N) {
				throw new ArgumentException (String.Format ("GNBitmap.N inequality"));
			}
			if (this.B != other.B) {
				throw new ArgumentException (String.Format ("GNBitmap.B inequality"));
			}
			Assertions.AssertIList<uint> (this.BitBlocks, other.BitBlocks, "GNBitmap.Bitmap");
			Assertions.AssertIList<uint> (this.Abs, other.Abs, "GNBitmap.Abs");
		}
		
		public void Save (BinaryWriter bw, bool save_bitmap)
		{
			bw.Write ((int)this.N);
			bw.Write ((short)this.B);
			bw.Write ((int)this.Abs.Count);
			PrimitiveIO<uint>.WriteVector (bw, this.Abs);
			if (save_bitmap) {
				bw.Write ((int)this.BitBlocks.Count);
				PrimitiveIO<uint>.WriteVector (bw, this.BitBlocks);
			}
		}
		
		public override void Save (BinaryWriter bw)
		{
			this.Save (bw, true);
		}

		public void Load (BinaryReader br, bool load_bitmap)
		{
			this.N = br.ReadInt32 ();
			this.B = br.ReadInt16 ();
			int len = br.ReadInt32 ();
			//Console.WriteLine ("xxxx Loading N: {0}, AbsBlockSize: {1}, len: {2}", this.N, this.AbsBlockSize, len);
			this.Abs = new uint[len];
			PrimitiveIO<uint>.ReadFromFile (br, len, this.Abs);
			if (load_bitmap) {
				len = br.ReadInt32 ();
				this.BitBlocks = new uint[len];
				PrimitiveIO<uint>.ReadFromFile (br, len, this.BitBlocks);
			}
		}
		
		public override void Load (BinaryReader br)
		{
			this.Load (br, true);
		}
		
		public override int Count {
			get {
				return (int)this.N;
			}
		}
		
		public override bool this[int i] {
			get {
				return BitAccess.GetBit (this.BitBlocks, i);
			}
		}
		
		public GGMN ()
		{
		}
		
		public void Build (IBitStream bitmap, short B)
		{
			this.BuildBackend (bitmap.GetIList32(), (int)bitmap.CountBits, B);
		}
		
		public void Build (IList<int> orderedList, short B)
		{
			int n = 0;
			if (orderedList.Count > 0) {
				n = orderedList[orderedList.Count - 1] + 1;
			}
			this.Build (orderedList, n, B);
		}
		
		public void Build (IList<int> orderedList, int N, short B)
		{
			BitStream32 b = new BitStream32 ();
			PlainSortedList s = new PlainSortedList ();
			s.Build (orderedList, (int)N);
			for (int i = 0; i < N; i++) {
				b.Write (s[i]);
			}
			this.Build (b, B);
		}
		
		public void BuildBackend (IList<uint> bitblocks, int N, short BlockSize)
		{
			this.BitBlocks = bitblocks;
			this.B = BlockSize;
			this.N = N;
			this.Abs = new uint[this.BitBlocks.Count / this.B];
			uint abs = 0;
			for (int index = 0, absindex = 0; absindex < this.Abs.Count; absindex++) {
				int count = Math.Min ((int)this.B, (bitblocks.Count - index));
				int rank = BitAccess.Rank1(this.BitBlocks, index, count, -1);
				abs += (uint)rank;
				this.Abs[absindex] = abs;
				index += count;
			}
//			Console.WriteLine("xxxxxxxxxxxxxxxxxxx GNBitmap, AbsBlockSize: {0}", this.AbsBlockSize);
//			for (int i = 0; i < this.Abs.Count; i++) {
//				Console.Write("{0}, ", this.Abs[i]);
//			}
//			Console.WriteLine("<end>");
		}
		
		public override int Rank1 (int pos)
		{
			if (pos < 0) {
				return 0;
			}
			int posindex = pos >> 5;
			int absindex = posindex / this.B - 1;
			if (absindex < 0) {
				return BitAccess.Rank1 (this.BitBlocks, 0, posindex, pos - (posindex << 5));
			} else {
				int abs = (int)this.Abs [absindex];
				int startindex = (absindex + 1) * this.B;
				//try {
					return abs + BitAccess.Rank1 (this.BitBlocks, startindex, posindex - startindex, pos - (posindex << 5));
//				} catch (Exception e) {
//					Console.WriteLine ("xxxxxxxx> pos: {0}, posindex: {1}, absindex: {2}", pos, posindex, absindex);
//					Console.WriteLine ("xxxxxxxx> Abs.Count: {0}, n: {1}", this.Abs.Count, this.Count);
//					Console.WriteLine ("xxxxxxxx> abs-rank: {0}, seq-start-index: {1}", abs, startindex);
//					Console.WriteLine (e.StackTrace);
//					throw e;
//				}


			}
		}
		
		public override int Select1 (int rank)
		{
			if (rank <= 0) {
				return -1;
			}
			// Console.WriteLine ("xxxxxxx> Abs.Count: {0}, rank: {1}, absblocksize: {2}", Abs.Count, rank, this.AbsBlockSize);
			int absindex = -1;
			if (this.Abs.Count > 0) {
				absindex = GenericSearch.FindFirst<uint> ((uint)rank, this.Abs);
			}
			if (absindex >= 0 && this.Abs[absindex] == rank) {
				absindex--;
			}
			if (absindex < 0) {
				return BitAccess.Select1 (this.BitBlocks, 0, this.B, rank);
			} else {
				// Console.WriteLine ("GNBitmap Abs.Count: {0}, absindex: {1}", this.Abs.Count, absindex);
				int startindex = (absindex + 1) * this.B;
				//return ((startindex)<<5) +
				//	BitAccess.Select1 (this.BitBlocks, startindex, this.AbsBlockSize, rank - (int)this.Abs[absindex]);
				int rel = 0;
				if (this.BitBlocks.Count != startindex) {
					rel = BitAccess.Select1 (this.BitBlocks, startindex, this.B, rank - (int)this.Abs[absindex]);
				}
				return (startindex << 5) + rel;
			}
		}		
	}
}
