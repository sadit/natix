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
//   Original filename: natix/CompactDS/Bitmaps/RRR.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class RRR : RankSelectBase
	{
		// we use the GN bitmap strategy
		protected IList<int> Klasses;
		IList<int> AbsRank;
		BitStream32 Offsets;
		IList<int> AbsOffset;
		protected short BlockSize;
		int N;
		// we implement BinCoeffOffTable in multiple lists for simplicity,
		// since it is just a single OffTable per process it is not a problem
		// static short[][] BinCoeff;
		static List<short>[] OffTable;
		static byte[] NumBits;
		
		public class CtxCache : BitStreamCtx
		{
			public int prev_item;
			public CtxCache (int offset) : base(offset)
			{
				this.prev_item = int.MinValue;
			}
		}
	
		static byte GetClass (short b)
		{
			return (byte)(Bits.PopCount8[b & 0xff] + Bits.PopCount8[b >> 8]);
		}
		
		protected int GetOffset (short b, byte klass)
		{
			return OffTable[klass].BinarySearch (b);
		}
		
		static RRR ()
		{
			var BinCoeff = new short[16]
			{ 1, 15, 105, 455, 1365, 3003, 5005, 6435,
				6435, 5005, 3003, 1365, 455, 105, 15, 1 };
			OffTable = new List<short>[BinCoeff.Length];
			NumBits = new byte[BinCoeff.Length];
			for (int i = 0; i < BinCoeff.Length; i++) {
				OffTable[i] = new List<short> (BinCoeff[i]);
				NumBits[i] = (byte)Math.Ceiling (Math.Log (BinCoeff[i], 2));
			}
			int maxvalue = (1 << 15);
			for (int i = 0; i < maxvalue; i++) {
				var klass = GetClass ((short) i);
				OffTable[klass].Add ((short)i);
			}
		}
		
		public RRR () : base()
		{
		}
		
		public override int Count {
			get {
				return this.N;
			}
		}
		
		virtual protected void InitClasses ()
		{
			this.Klasses = new ListIFS (4);
		}
		
		virtual protected void SaveClasses (BinaryWriter Output)
		{
			(this.Klasses as ListIFS).Save (Output);
		}
		
		virtual protected void LoadClasses (BinaryReader Input)
		{
			var c = new ListIFS ();
			c.Load (Input);
			this.Klasses = c;
		}
		
		virtual protected void EncodeClass (int klass)
		{
			this.Klasses.Add (klass);
		}

		virtual protected int DecodeClass (int i, CtxCache ctx)
		{
			var klass = this.Klasses[i];
			return klass;
		}
		
		public void Build (IList<int> orderedList, short blockSize)
		{
			var L = new PlainSortedList ();
			L.Build (orderedList);
			this.Build (L, blockSize);
		}
		
		public void Build (IRankSelect RS, short blockSize)
		{
			// we need to store n bits, this is not the best solution!!
			BitStream32 B = new BitStream32 ();
			for (int i = 0, count = RS.Count; i < count; i++) {
				B.Write (RS.Access(i));
			}
			this.Build (B, blockSize);
		}

		public void Build (IBitStream B, short blockSize)
		{
			this.N = (int)B.CountBits;
			this.BlockSize = (short)blockSize;
			this.InitClasses();
			this.Offsets = new BitStream32 ();
			IList<int> _L = new ListIFS (15, B);
			IList<int> L;
			if ((B.CountBits % 15) == 0) {
				L = _L;
			} else {
				int D = _L.Count;
				int C = 15 * D;
				var ctx = new BitStreamCtx(0);
				ctx.Seek(C);
				int last_block = (int)B.Read(((int)B.CountBits) - C, ctx);
				L = new ListGen<int>(delegate(int a) {
					if (a == D) {
						return last_block;
					} else {
						return _L[a];
					}
				}, D+1);			
			}
			this.AbsRank = new int[(int)Math.Ceiling(((float)L.Count) / this.BlockSize)];
			this.AbsOffset = new int[ this.AbsRank.Count ];
			int I = 0;
			int acc = 0;
			for (int i = 0; i < L.Count; i++) {
				var u = (short)L[i];
				var klass = GetClass(u);
				this.EncodeClass(klass);
				if (i % this.BlockSize == 0) {
					this.AbsRank[I] = acc;
					this.AbsOffset[I] = (int)this.Offsets.CountBits;
					I++;
				}
				var numbits = NumBits[klass];
				if (numbits > 0) {
					int offset = this.GetOffset (u, klass);
					this.Offsets.Write (offset, numbits);
				}
				acc += klass;
			}
		}
		
		public override void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.N);
			Output.Write ((short)this.BlockSize);
			this.SaveClasses (Output);
			this.Offsets.Save (Output);
			Output.Write ((int)this.AbsOffset.Count);
			PrimitiveIO<int>.WriteVector (Output, this.AbsRank);
			PrimitiveIO<int>.WriteVector (Output, this.AbsOffset);
		}
		
		public override void Load (BinaryReader Input)
		{
			this.N = Input.ReadInt32 ();
			this.BlockSize = Input.ReadInt16 ();
			this.LoadClasses (Input);
			this.Offsets = new BitStream32 ();
			this.Offsets.Load (Input);
			var len = Input.ReadInt32 ();
			this.AbsRank = new int[len];
			this.AbsOffset = new int[len];
			PrimitiveIO<int>.ReadFromFile (Input, len, this.AbsRank);
			PrimitiveIO<int>.ReadFromFile (Input, len, this.AbsOffset);
		}

		public override void AssertEquality (IRankSelect _other)
		{
			RRR other = _other as RRR;
			if (other == null) {
				throw new ArgumentNullException ("RRR Other should be a RRR object too");
			}
			if (this.N != other.N) {
				throw new ArgumentNullException ("RRR Inequality on N");
			}
			if (this.BlockSize != other.BlockSize) {
				throw new ArgumentException ("RRR Inequality on BlockSize");
			}
			Assertions.AssertIList<int> (this.Klasses, other.Klasses, "RRR Classes");
			Assertions.AssertIList<int> (this.AbsRank, other.AbsRank, "RRR AbsRank");
			Assertions.AssertIList<int> (this.AbsOffset, other.AbsOffset, "RRR AbsOffset");
		}
		
		protected short ReadBlock (int klass, int offset)
		{
			int numbits = NumBits[klass];
			if (numbits > 0) {
				int klass_offset;
				klass_offset = (int)this.Offsets.Read (numbits, new BitStreamCtx(offset));
				
				return OffTable[klass][klass_offset];
			} else {
				return OffTable[klass][0];
			}
		}
				
		int Rank1AccessBackend (int pos, out bool last_bit)
		{
			// in advance, we use pos as a simple counter of the remaining positions
			int blockIndex = pos / 15 / this.BlockSize;
			int classIndex = blockIndex * this.BlockSize;
			int rank = this.AbsRank[blockIndex];
			pos -= classIndex * 15;
			int offset = this.AbsOffset[blockIndex];
			int klass;
			var ctx = new CtxCache (-1);
			if (pos >= 15) {
				for (; pos >= 15; pos -= 15) {
					klass = this.DecodeClass (classIndex, ctx);
					classIndex++;
					rank += klass;
					int numbits = NumBits[klass];
					offset += numbits;
				}
			}
			klass = this.DecodeClass (classIndex, ctx);
			uint block = (uint)this.ReadBlock (klass, offset);
			last_bit = BitAccess.GetBit ((int)block, pos);
			rank += BitAccess.Rank1 (block, pos);
			return rank;
		}
		
		public override int Rank1 (int pos)
		{
			if (pos < 0) {
				return 0;
			}
			bool last_bit;
			return this.Rank1AccessBackend (pos, out last_bit);
		}
		
		public override bool this[int pos] {
			get {
				bool last_bit;
				this.Rank1AccessBackend (pos, out last_bit);
				return last_bit;
			}
		}
		
		public override int Select1 (int rank)
		{
			if (rank < 1) {
				return -1;
			}
			int blockIndex = GenericSearch.FindFirst<int> (rank, this.AbsRank);
			if (this.AbsRank[blockIndex] == rank) {
				blockIndex--;
			}
			int classIndex = blockIndex * this.BlockSize;
			rank -= this.AbsRank[blockIndex];
			int pos = classIndex * 15;
			int offset = this.AbsOffset[blockIndex];
			int klass;
			var ctx = new CtxCache (-1);
			while (rank > 0) {
				klass = this.DecodeClass (classIndex, ctx);
				if (rank - klass <= 0) {
					break;
				}
				classIndex++;
				pos += 15;
				rank -= klass;
				int numbits = NumBits[klass];
				offset += numbits;
			}
			if (rank > 0) {
				klass = this.DecodeClass (classIndex, ctx);
				uint block = (uint)this.ReadBlock (klass, offset);
				// Console.WriteLine ("rank: {0}, pos: {1}, block: {2}, klass: {3}, offset: {4}",
				//	rank, pos, block, klass, offset);
				pos += BitAccess.Select1 (block, rank);
			}
			return pos;

		}
	}
}
