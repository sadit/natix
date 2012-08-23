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
//   Original filename: natix/CompactDS/BitStreams/BitStream32.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class BitStream32 : IBitStream
	{
		// long Offset;
		long N;
		IList<uint> Buffer;
		
		public void AssertEquality (object obj)
		{
			var B = obj as BitStream32;
			/*if (B.Offset != this.Offset) {
				var w = String.Format ("BitStream32 Inequality in Offset. B.Offset: {0}, this.Offset: {1}", B.Offset, this.Offset);
				throw new ArgumentException (w);
			
			}*/
			if (B.N != this.N) {
				throw new ArgumentException ("BitStream32 Inequality in N");
			}
			Assertions.AssertIList<uint>(this.Buffer, B.Buffer, "BitStream32.Buffer");
		}
		
		public IList<UInt32> GetIList32 ()
		{
			return this.Buffer;
		}
		
		public IList<UInt64> GetIList64 ()
		{
			return new ListGen<UInt64> (delegate(int i) {
				i <<= 1;
				UInt64 item = this.Buffer[i];
				UInt64 tmp = 0;
				if (i+1 < this.Buffer.Count) {
					tmp = this.Buffer[i+1];
				}
				item |= tmp << 32;
				return item;
			}, (int)Math.Ceiling(this.Buffer.Count / 2.0));
		}

//		public long CurrentOffset {
//			get {
//				return this.Offset;
//			}
//		}
		
		public BitStream32 ()
		{
			this.Buffer = new List<UInt32> ();
			// this.Offset = this.N = 0;
			this.N = 0;
		}
		
		public BitStream32 (int buffersizeuint)
		{
			// this.Offset = this.N = 0;
			this.N = 0;
			this.Buffer = new List<UInt32> (buffersizeuint);
		}
		
		public BitStream32 (IList<uint> buffer)
		{
			this.Buffer = buffer;
			//this.Offset = this.N = (uint)(buffer.Count << 5);
			this.N = (uint)(buffer.Count << 5);
		}
		
		public BitStream32 (IList<byte> buffer)
		{
			//this.Offset = this.N = (uint)(buffer.Count << 3);
			this.N = (uint)(buffer.Count << 3);
			this.Buffer = new List<UInt32>((int)Math.Ceiling(buffer.Count * 1.0 / 4));
			for (int i = 0, m = 0; i < this.Buffer.Count; m++) {
				uint u = 0;
				for (int j = 0; j < 4 && i < this.Buffer.Count; i++, j++) {
					u |= ((uint)buffer[i + j]) << (8 * j);
				}
				this.Buffer[m] = u;
			}
		}
		
		public BitStream32 (BitStream32 bstream)
		{
			this.Buffer = bstream.Buffer;
			// this.Offset = bstream.Offset;
			this.N = bstream.N;
		}
		
		/// <summary>
		/// Number of bits
		/// </summary>
		
		public long CountBits {
			get { return this.N; }
		}

		/// <summary>
		/// Number of bytes
		/// </summary>
		/*public int CountBytesX {
			get { return this.Buffer.Count << 2; }
		}*/
		
		/// <summary>
		/// Number of uint32 items
		/// </summary>
		public int Count32 {
			get { return this.Buffer.Count; }
		}
		public int Count64 {
			get { return this.Buffer.Count >> 1; }
		}
		public void Clear ()
		{
			this.Buffer.Clear ();
			// this.Offset = this.N = 0;
			this.N = 0;
		}
		
		/// <summary>
		/// Returns the specified uint (position i), please notice that bitcounter is not checked
		/// </summary>
		public uint Get32 (int i)
		{
			return this.Buffer[i];
		}

		/// <summary>
		/// Set the complete uint (val) at the specified position (i), please notice that bitcounter is not modified
		/// </summary>
		public void Set32 (int i, UInt32 val)
		{
			this.Buffer[i] = val;
		}

		public bool this[long i] {
			get { return BitAccess.GetBit (this.Buffer, i); }
			set {
				if (value) {
					BitAccess.SetBit (this.Buffer, i);
				} else {
					BitAccess.ResetBit (this.Buffer, i);
				}
			}
		}

		/*public void TrimExcess ()
		{
			this.Buffer.TrimExcess ();
		}*/

		public override string ToString ()
		{
			StringWriter w = new StringWriter ();
			w.Write ("LSB to MSB: ");
			for (long i = 0; i < this.CountBits; i++) {
				if (this [i]) {
					w.Write ('1');
				} else {
					w.Write ('0');
				}
				// w.Write (BinaryHammingSpace.ToAsciiString (this.Buffer[i]));
			}
			return w.ToString ();
		}

		//*** IO LIKE METHODS ***
		public bool Read (BitStreamCtx ctx)
		{
			return this[ctx.Offset++];
		}

		public void Write (bool x)
		{
			if ((this.N >> 5) >= this.Buffer.Count) {
				this.Buffer.Add (0);
			}
			this[this.N] = x;
			++this.N;
		}

		public void Write (bool x, int times)
		{
			uint u = 0;
			if (x) {
				u = ~u;
			}
			while (times > 32) {
				this.Write (u, 32);
				times -= 32;
			}
			if (times > 0) {
				this.Write (u, times);
			}
		}

		public void Write (ulong x, int numbits)
		{
			this.Write ((uint)x, numbits);
		}

		public void Write (long x, int numbits)
		{
			this.Write ((uint)x, numbits);
		}
	
		public void Write (int x, int numbits)
		{
			this.Write ((uint)x, numbits);
		}		
		
		public UInt64 Read (int numbits, BitStreamCtx ctx)
		{
			int offset_r = (int)(ctx.Offset & 31);
			int offset_q = (int)(ctx.Offset >> 5);
			uint u;
			u = this.Buffer [offset_q];

			// cases:
			// 1) numbits is contained in q-th item
			// 2) numbits is in two items, q and q+1 th items
			// r is the remainder in the this.Offset bit-stream
			// q is the integer number containing the this.Offset bit

			// Console.WriteLine("u: {0}, offset_q: {1}, offset_r: {2}, mask: {3}, numbits: {4}",
			//		u, offset_q, offset_r, mask, numbits);
			
			UInt64 output;
			if (numbits + offset_r <= 32) {
				// case 1
				uint mask = (uint)((1ul << (numbits)) - 1); 
				output = (u >> offset_r) & mask;
			} else {
				// case 2
				u >>= offset_r;
				int d1 = 32 - offset_r;
				// int d2 = numbits + offset_r - 32;
				int d2 = numbits - d1;
				uint v = this.Buffer [offset_q + 1] & ((1u << d2) - 1);
				v <<= d1;
				output = u | v;
			}
			ctx.Offset += numbits;
			return output;
		}
		
		public void Write (uint x, int numbits)
		{
			if (numbits < 1) {
				return;
			}
			int offset_r = (int)(this.N & 31);
			int offset_q = (int)(this.N >> 5);
			if (this.Buffer.Count == offset_q) {
				this.Buffer.Add (0);
			}
			// same cases than Read, cases:
			// 1) numbits is contained in q-th item
			// 2) numbits is in two items, q and q+1 th items
			// r is the remainder in the this.Offset bit-stream
			// q is the integer number containing the this.Offset bit
			uint mask = uint.MaxValue;
			if (numbits < 32) {
				mask = (1u << numbits) - 1;
			}
			x &= mask;
			if (numbits + offset_r <= 32) {
				// case 1
				this.Buffer [offset_q] |= x << offset_r;
			} else {
				// case 2
				// cleaning the remainding bits of x if necessary
				this.Buffer [offset_q] |= x << offset_r;
				x >>= 32 - offset_r;
				this.Buffer.Add (x);
			}
			this.N += numbits;
		}

		public void WriteAt (uint x, int numbits, long pos)
		{
			if (numbits < 1) {
				return;
			}
			if (pos + numbits > this.N) {
				throw new IndexOutOfRangeException ();
			}
			int offset_r = (int)(pos & 31);
			int offset_q = (int)(pos >> 5);
			// same cases than Read, cases:
			// 1) numbits is contained in q-th item
			// 2) numbits is in two items, q and q+1 th items
			// r is the remainder in the this.Offset bit-stream
			// q is the integer number containing the this.Offset bit
			uint mask = uint.MaxValue;
			if (numbits < 32) {
				mask = (1u << numbits) - 1;
			}
			x &= mask;
			if (numbits + offset_r <= 32) {
				// case 1
				var c = this.Buffer [offset_q];
				c &= ~(mask << offset_r);
				c |= x << offset_r;
				this.Buffer [offset_q] = c;
			} else {
				// case 2
				// cleaning the remainding bits of x if necessary
				var c = this.Buffer [offset_q];
				c &= ~(mask << offset_r);
				c |= x << offset_r;
				this.Buffer [offset_q] = c;
				// remainder
				var numbits_written = 32 - offset_r;
				x >>= numbits_written;
				mask >>= numbits_written;
				c = this.Buffer [offset_q + 1];
				c &= ~mask;
				c |= x;
				this.Buffer [offset_q + 1] = c;
			}
		}
		
//		public void Seek (long offset)
//		{
//			if (offset > this.CountBits || offset < 0) {
//				throw new ArgumentOutOfRangeException ("Can't seek outside bounds");
//			}
//			this.Offset = offset;
//		}
				
		public int ReadZeros (BitStreamCtx ctx)
		{
			int c = 0;
			while (ctx.Offset < this.N) {
				int numbits = (int)Math.Min (32, this.N - ctx.Offset);
				uint u = 0;
				u = (uint)this.Read (numbits, ctx);
				if (u == 0x0) {
					c += numbits;
					continue;
				}
				int pos = BitAccess.Select1 (u, 1);
				ctx.Offset -= numbits - pos;
				c += pos;
				break;
			}
			return c;
		}		
		
		public int ReadOnes (BitStreamCtx ctx)
		{
			int c = 0;
			while (ctx.Offset < this.N) {
				int numbits = (int)Math.Min (32, this.N - ctx.Offset);
				uint u = (uint)this.Read (numbits, ctx);
				//Console.WriteLine ("--- u: {0}, numbits: {1}", u, numbits);
				if (numbits == 32) {
					if (u == 0xFFFFFFFF) {
						c += numbits;
						continue;
					}
				} else {
					if ((u + 1) == (1u << numbits)) {
						c += numbits;
						continue;
					}
				}
				//Console.WriteLine ("+++ u: {0}, numbits: {1}", u, numbits);
				int pos = BitAccess.Select1 (~u, 1);
				ctx.Offset -= numbits - pos;
				c += pos;
				break;
			}
			return c;
		}
	
		public void Save (BinaryWriter w)
		{
			w.Write ((long)this.N);
			PrimitiveIO<uint>.WriteVector (w, this.Buffer);
		}

		public void Load (BinaryReader r)
		{
			// this.Clear ();
			this.N = r.ReadInt64 ();
			int numitems = (int)Math.Ceiling (this.N / 32.0);
			// Console.WriteLine ("BitStream32.Load N: {0}, num_items: {1}", this.N, numitems);
			this.Buffer = new uint[numitems];
			// this.Seek (0);
			PrimitiveIO<uint>.ReadFromFile (r, numitems, this.Buffer);
		}
	}
}
