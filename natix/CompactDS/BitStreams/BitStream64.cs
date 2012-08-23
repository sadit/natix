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
//   Original filename: natix/CompactDS/BitStreams/BitStream64.cs
// 
//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//
//namespace natix.CompactDS
//{
//	public class BitStream64 : IBitStream
//	{
//		long Offset;
//		long Size;
//		public IList<UInt64> Buffer;
//		
//		public void AssertEquality (object obj)
//		{
//			var B = obj as BitStream64;
//			if (B.Offset != this.Offset) {
//				throw new ArgumentException ("BitStreamUInt64 Inequality in Offset");
//			
//			}
//			if (B.Size != this.Size) {
//				throw new ArgumentException ("BitStreamUInt64 Inequality in Size");
//			}
//			Assertions.AssertIList<UInt64>(this.Buffer, B.Buffer, "BitStream64.Buffer");
//		}
//		
//		public long CurrentOffset {
//			get {
//				return this.Offset;
//			}
//		}
//		
//		public BitStream64 ()
//		{
//			this.Buffer = new List<UInt64> ();
//			this.Offset = this.Size;
//		}
//		
//		public BitStream64 (int buffersizeuint)
//		{
//			this.Offset = this.Size = 0;
//			this.Buffer = new List<UInt64> (buffersizeuint);
//		}
//		
//		public BitStream64 (IList<UInt64> buffer)
//		{
//			this.Buffer = buffer;
//			this.Offset = this.Size = buffer.Count << 6;
//		}
//		
//		public BitStream64 (IList<byte> buffer)
//		{
//			this.Offset = this.Size = buffer.Count << 3;
//			this.Buffer = new List<UInt64>((int)Math.Ceiling(buffer.Count * 1.0 / 8));
//			for (int i = 0, m = 0; i < this.Buffer.Count; m++) {
//				UInt64 u = 0;
//				for (int j = 0; j < 8 && i < this.Buffer.Count; i++, j++) {
//					u |= ((UInt64)buffer[i + j]) << (8 * j);
//				}
//				this.Buffer[m] = u;
//			}
//		}
//		
//		public BitStream64 (BitStream64 bstream)
//		{
//			this.Buffer = bstream.Buffer;
//			this.Offset = bstream.Offset;
//			this.Size = bstream.Size;
//		}
//		
//		/// <summary>
//		/// Number of bits
//		/// </summary>
//		
//		public long CountBits {
//			get { return this.Size; }
//		}
//
//		/// <summary>
//		/// Number of bytes
//		/// </summary>
//		/*public int CountBytesX {
//			get { return this.Buffer.Count << 2; }
//		}*/
//		
//		/// <summary>
//		/// Number of uint32 items
//		/// </summary>
//		public int Count64 {
//			get { return this.Buffer.Count; }
//		}
//
//		public void Clear ()
//		{
//			this.Buffer.Clear ();
//			this.Offset = this.Size = 0;
//		}
//		
//		public IList<UInt64> GetIList64 ()
//		{
//			return this.Buffer;
//		}
//		
//		public IList<UInt32> GetIList32 ()
//		{
//			return new ListGen<UInt32> (delegate(int i) {
//				ulong item = this.Buffer[i >> 1];
//				if ((i & 1) == 1) {
//					return (uint)((item >> 32) & 0xffffffff);
//				} else {
//					return (uint)(item & 0xffffffff);
//				}
//			}, (int)Math.Ceiling(this.Buffer.Count * 2.0));
//		}
//
//		/// <summary>
//		/// Returns the specified uint (position i), please notice that bitcounter is not checked
//		/// </summary>
//		public UInt64 Get64 (int i)
//		{
//			return this.Buffer[i];
//		}
//
//		/// <summary>
//		/// Set the complete uint (val) at the specified position (i), please notice that bitcounter is not modified
//		/// </summary>
//		public void Set64 (int i, UInt64 val)
//		{
//			this.Buffer[i] = val;
//		}
//		
//		public bool this[long i] {
//			get { return BitAccess64.GetBit (this.Buffer, i); }
//			set {
//				if (value) {
//					BitAccess64.SetBit (this.Buffer, i);
//				} else {
//					BitAccess64.ResetBit (this.Buffer, i);
//				}
//			}
//		}
//
//		/*public void TrimExcess ()
//		{
//			this.Buffer.TrimExcess ();
//		}*/
//
//		public override string ToString ()
//		{
//			StringWriter w = new StringWriter ();
//			w.Write ("LSB to MSB: ");
//			for (int i = 0; i < this.CountBits; i++) {
//				if (this[i]) {
//					w.Write ('1');
//				} else {
//					w.Write ('0');
//				}
//				// w.Write (BinaryHammingSpace.ToAsciiString (this.Buffer[i]));
//			}
//			return w.ToString ();
//		}
//
//		//*** IO LIKE METHODS ***
//		public bool Read ()
//		{
//			return this[this.Offset++];
//		}
//		
//		public void Write (bool x)
//		{
//			lock (this.Buffer) {
//				if ((this.Size >> 6) >= this.Buffer.Count) {
//					this.Buffer.Add (0);
//				}
//				this[this.Size] = x;
//				++this.Size;
//				this.Offset = this.Size;
//			}
//		}
//
//		public void Write (bool x, int times)
//		{
//			UInt64 u = 0;
//			if (x) {
//				u = ~u;
//			}
//			while (times > 64) {
//				this.Write (u, 64);
//				times -= 64;
//			}
//			if (times > 0) {
//				this.Write (u, times);
//			}
//		}
//		
//		public void Write (Int64 x, int numbits)
//		{
//			this.Write ((UInt64)x, numbits);
//		}
//
//		public void Write (Int32 x, int numbits)
//		{
//			this.Write ((UInt64)x, numbits);
//		}
//
//		public void Write (UInt32 x, int numbits)
//		{
//			this.Write ((UInt64)x, numbits);
//		}
//
//		public UInt64 Read (int numbits)
//		{
//			int offset_r = (int)(this.Offset & 63);
//			int offset_q = (int)(this.Offset >> 6);
//			UInt64 u = this.Buffer[offset_q];
//			// cases:
//			// 1) numbits is contained in q-th item
//			// 2) numbits is in two items, q and q+1 th items
//			// r is the remainder in the this.Offset bit-stream
//			// q is the integer number containing the this.Offset bit
//
//			// Console.WriteLine("u: {0}, offset_q: {1}, offset_r: {2}, mask: {3}, numbits: {4}",
//			//		u, offset_q, offset_r, mask, numbits);
//			UInt64 output;
//			if (numbits + offset_r <= 64) {
//				// case 1
//				UInt64 mask = UInt64.MaxValue;
//				if (numbits < 64) {
//					mask = (1ul << (numbits))-1; 
//				}
//				output = (u >> offset_r) & mask;
//			} else {
//				// case 2
//				u >>= offset_r;
//				int d1 = 64 - offset_r;
//				// int d2 = numbits + offset_r - 32;
//				int d2 = numbits - d1;
//				UInt64 v = this.Buffer[offset_q + 1] & ((1ul << d2)-1);
//				v <<= d1;
//				output = u | v;
//			}
//			this.Offset += numbits;
//			return output;
//		}
//		
//		public void Write (UInt64 x, int numbits)
//		{
//			int offset_r = (int)(this.Offset & 63);
//			int offset_q = (int)(this.Offset >> 6);
//			if (this.Buffer.Count == offset_q) {
//				this.Buffer.Add (0);
//			}
//
//			// same cases than Read, cases:
//			// 1) numbits is contained in q-th item
//			// 2) numbits is in two items, q and q+1 th items
//			// r is the remainder in the this.Offset bit-stream
//			// q is the integer number containing the this.Offset bit
//			UInt64 mask = UInt64.MaxValue;
//			if (numbits < 64) {
//				mask = (1ul << (numbits )) - 1;
//			}
//			x &= mask;
//			if (numbits + offset_r <= 64) {
//				// case 1
//				this.Buffer[offset_q] |= x << offset_r;
//			} else {
//				// case 2
//				// cleaning the remainding bits of x if necessary
//				this.Buffer[offset_q] |= x << offset_r;
//				x >>= 64 - offset_r;
//				this.Buffer.Add(x);
//			}
//			this.Offset += numbits;
//			this.Size = this.Offset;
//		}
//		
//		public void Seek (long offset)
//		{
//			if (offset > this.CountBits || offset < 0) {
//				throw new ArgumentOutOfRangeException ("Can't seek outside bounds");
//			}
//			this.Offset = offset;
//		}
//		
//		public int ReadZeros ()
//		{
//			int c = 0;
//			while (!this.Read ()) {
//				c++;
//			}
//			this.Offset--;
//			return c;
//		}
//		
//		public int ReadOnes ()
//		{
//			int c = 0;
//			while (this.Read ()) {
//				c++;
//			}
//			this.Offset--;
//			return c;
//		}
//		
//		public void Save (BinaryWriter w)
//		{
//			w.Write ((long)this.Size);
//			PrimitiveIO<UInt64>.WriteVector (w, this.Buffer);
//		}
//
//		public void Load (BinaryReader r)
//		{
//			this.Clear ();
//			long numbits = r.ReadInt64();
//			this.Size = numbits;
//			int numitems = (int)Math.Ceiling( numbits * 1.0 / 64 );
//			PrimitiveIO<UInt64>.ReadFromFile (r, numitems, this.Buffer);
//		}
//	}
//}
