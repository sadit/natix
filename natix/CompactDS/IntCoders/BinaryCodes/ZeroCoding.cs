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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/ZeroCoding.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class ZeroCoding : IIEncoder32
	{
		
		IIEncoder32 Coder;
		int Smaller;
		
		public ZeroCoding ()
		{
		}
		
		public ZeroCoding (IIEncoder32 coder, int smaller)
		{
			this.Smaller = smaller;
			this.Coder = coder;
		}
			
		public void Encode (IBitStream Buffer, int u)
		{
			this.Coder.Encode (Buffer, u + this.Smaller);
		}
		
		public int Decode (IBitStream Buffer, BitStreamCtx ctx)
		{
			return this.Coder.Decode (Buffer, ctx) - this.Smaller;
		}
		
		public void Save (BinaryWriter Output)
		{
			IEncoder32GenericIO.Save (Output, this.Coder);
			Output.Write ((int)this.Smaller);
		}

		public void Load (BinaryReader Input)
		{
			this.Coder = IEncoder32GenericIO.Load (Input);
			this.Smaller = Input.ReadInt32 ();
		}
	}
}
