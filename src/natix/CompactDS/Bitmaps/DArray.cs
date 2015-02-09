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
	public class DArray : Bitmap
	{
		GGMN BaseIndex;
		int[] PosAbs;
		GGMN IsLargeBlock;
		int[] SavedPos;
		int B;
		
		public DArray () : base()
		{
		}
		
		public override int Count {
			get {
				 return this.BaseIndex.Count;
			}
		}

		public override bool Access(int i)
		{
			return this.BaseIndex.Access(i);
		}
		
		public override void AssertEquality (Bitmap obj)
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
		
		public void Build (BitStream32 bstream, short Brank, int Bselect)
		{
			this.BuildBackend (bstream.Buffer.ToArray(), Brank, Bselect, (int)bstream.CountBits);
		}
		
		public void BuildBackend (uint[] bitblocks, short Brank, int Bselect, int N)
		{
			this.BaseIndex = new GGMN ();
			this.BaseIndex.BuildBackend (bitblocks, N, Brank);
			this.B = Bselect;
			int M = (int)this.BaseIndex.Rank1 (N - 1);
			// Console.WriteLine("XXXX N: {0}, M: {1}", N, M);
			this.PosAbs = new int[(int)Math.Ceiling (M * 1.0 / this.B)];
			int m = 0;
			int i = 0;
			int large_limit = 32 * this.B;
			BitStream32 mark_bits = new BitStream32 (this.PosAbs.Length >> 5);
			int num_saved_slots = 0;
			while (m < M) {
				int start_pos = (int)this.BaseIndex.Select1 (m + 1);
				int Bmin = Math.Min (this.B, M - m);
				int end_pos = (int)this.BaseIndex.Select1 (m + Bmin);
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
			int R = (int)this.IsLargeBlock.Rank1 (this.IsLargeBlock.Count - 1);
			this.SavedPos = new int[num_saved_slots];
			for (int k = 0; k < R; k++) {
				int index = (int)this.IsLargeBlock.Select1 (k + 1);
				int rank_base = this.B * index;
				int index_base = this.B * k;
				int maxB = Math.Min (this.B, M - rank_base);
				for (int rank_rel = 0; rank_rel < maxB; rank_rel++) {
					var pos = (int)this.BaseIndex.Select1( rank_rel + rank_base + 1);
					this.SavedPos[rank_rel + index_base] = pos;
				}
			}
		}
		
		public override int Select1 (int rank)
		{
			if (rank < 1) {
				return -1;
			}
			var index = (rank - 1)/ this.B;
			var start_bit = this.PosAbs[ index ];
			var rank_diff = (rank - 1) - index * this.B;
			if (this.IsLargeBlock.Access(index)) {
				index = (this.IsLargeBlock.Rank1(index) - 1) * this.B + rank_diff ;
				return this.SavedPos[ index ]; 
			} else {
				if (rank_diff == 0) {
					return start_bit;
				}
				var max_count_bits = this.BaseIndex.Count - start_bit;
				return BitAccess.Select1BitArgs( this.BaseIndex.GetBitBlocks(), start_bit+1, max_count_bits, rank_diff);
			}
		}

		public override int Rank1 (int p)
		{
			return this.BaseIndex.Rank1 (p);
		}

		public override void Save (BinaryWriter bw)
		{
			this.Save (bw, true);
		}
		
		public void Save (BinaryWriter bw, bool save_bitmap)
		{
			bw.Write ((int)this.B);
			bw.Write ((int)this.PosAbs.Length);
			bw.Write ((int)this.SavedPos.Length);
			PrimitiveIO<int>.SaveVector (bw, this.PosAbs);
			PrimitiveIO<int>.SaveVector (bw, this.SavedPos);
			this.IsLargeBlock.Save (bw);
			this.BaseIndex.Save (bw, save_bitmap);
		}
		
		public override void Load (BinaryReader br)
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
			PrimitiveIO<int>.LoadVector (br, posabslen, this.PosAbs);
			PrimitiveIO<int>.LoadVector (br, expposlen, this.SavedPos);
			this.IsLargeBlock = new GGMN ();
			this.IsLargeBlock.Load (br);
			this.BaseIndex = new GGMN ();
			this.BaseIndex.Load (br, load_bitmap);
		}
		
		public uint[] GetBitBlocks ()
		{
			return this.BaseIndex.GetBitBlocks ();
		}
		
		public void SetBitBlocks (uint[] bitblocks)
		{
			this.BaseIndex.SetBitBlocks (bitblocks);
		}
	}
}

