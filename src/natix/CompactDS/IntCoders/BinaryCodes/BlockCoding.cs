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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/BlockCoding.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class BlockCoding : IIEncoder32
	{
		IIEncoder32 SkipCoder;
		int Power;
		
		public BlockCoding ()
		{
		}
		
		public BlockCoding (int power, IIEncoder32 skipcoder)
		{
			// power must be an small integer like 3, 4, 5
			this.Power = power;
			this.SkipCoder = skipcoder;
		}

		public virtual void Save (BinaryWriter Output)
		{
			IEncoder32GenericIO.Save (Output, this.SkipCoder);
			Output.Write ((byte)this.Power);
		}

		public virtual void Load (BinaryReader Input)
		{
			this.SkipCoder = IEncoder32GenericIO.Load (Input);
			this.Power = Input.ReadByte ();
		}

		public void Encode (BitStream32 Buffer, int u)
		{
			if (u < 0) {
				throw new ArgumentOutOfRangeException (String.Format ("Invalid range for BlockCoding, u: {0}", u));
			}
			int skip = u >> this.Power;
			this.SkipCoder.Encode (Buffer, skip);
			u &= (1 << this.Power) - 1;
			Buffer.Write(u, this.Power);
		}
		
		public int Decode (BitStream32 Buffer, BitStreamCtx ctx)
		{
			int skip = this.SkipCoder.Decode (Buffer, ctx);
			int output = (int)Buffer.Read (this.Power, ctx);
			return (skip << this.Power) | output;
		}
	}
}
