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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/BinaryCoding.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// Uses a small fixed number of bits to represent numbers. Truncated binary.
	/// </summary>
	public class BinaryCoding : IIEncoder32
	{
		byte numbits;
		
		public int NumBits
		{
			get {
				return this.numbits;
			}
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((byte)this.numbits);
		}
		
		public void Load (BinaryReader Input)
		{
			this.numbits = Input.ReadByte ();
		}
		
		public BinaryCoding () : this(32)
		{
		}
		
		public BinaryCoding (int numbits)
		{
			this.numbits = (byte)numbits;
		}
			
		public void Encode (BitStream32 Buffer, int u)
		{
			if (u < 0) {
				var message = String.Format ("Negative numbers are not valid for BinaryCoding, u: {0}", u);
				throw new ArgumentOutOfRangeException (message);
			}
			int mask = 1 << this.NumBits;
			if (u >= mask) {
				var message = String.Format ("Number too large for {0} bits BinaryCoding, {1} >= {2}",
					this.NumBits, u, mask);
				throw new ArgumentOutOfRangeException (message);
			}
			--mask;
			u &= mask;
			Buffer.Write(u, this.NumBits);
		}
		
		public int Decode (BitStream32 Buffer, BitStreamCtx ctx)
		{
			int number = (int)Buffer.Read (this.NumBits, ctx);
			return number;
		}
		
		public int ArrayGet (BitStream32 Buffer, int pos)
		{
			
			long p = pos;
			p *= this.NumBits;
			var ctx = new BitStreamCtx (p);
			int number = (int)Buffer.Read (this.NumBits, ctx);
			return number;
		}
		
		public void ArraySet (BitStream32 Buffer, int pos, int val)
		{
			long p = pos;
			p *= this.NumBits;
			Buffer.WriteAt ((uint)val, this.NumBits, p);
		}
		
		public void ArrayAdd (BitStream32 Buffer, int val)
		{
			Buffer.Write (val, this.NumBits); 
		}
	}
}
