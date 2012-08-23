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
//   Original filename: natix/CompactDS/IntCoders/OctetCodes/OctetSearchCode.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class OctetSearchCode : IOctetCoder
	{
		byte numoctets;
		
		public OctetSearchCode (long N)
		{
			this.numoctets = (byte)Math.Ceiling (Math.Log (N+1, 256));
			// Console.WriteLine ("== OctetSearchCode N: {0}, Log256: {1}", N, this.numoctets);
		}
		
		public byte NumOctets {
			get {
				return this.numoctets;
			}
		}
		public long Decode (OctetReader reader)
		{
			long d = 0;
			for (int i = (this.numoctets-1) * 8; i >= 0; i-=8) {
				long u = reader.Read ();
				d |= u << i;
			}
			return d;
		}
		
		public void Encode (long d, IList<byte> output)
		{
			for (int i = (this.numoctets-1) * 8; i >= 0; i-=8) {
				long u = d;
				u >>= i;
				output.Add ((byte)(u & 255));
			}
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write (this.numoctets);
		}
		
		public void Load (BinaryReader Input)
		{
			this.numoctets = Input.ReadByte ();
		}
	}
}

