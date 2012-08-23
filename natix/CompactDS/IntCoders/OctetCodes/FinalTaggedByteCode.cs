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
//   Original filename: natix/CompactDS/IntCoders/OctetCodes/FinalTaggedByteCode.cs
// 
using System;
using System.IO;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// Byte code. Similar to the Brisaboa et al. SPIRE 2003 (?)
	/// </summary>
	public class FinalTaggedByteCode : IOctetCoder
	{
		public FinalTaggedByteCode ()
		{
		}
		
		public void Encode (long d, IList<byte> Output)
		{
			// byte lg = (byte)Math.Ceiling (Math.Log (d + 1, 128));
			byte lg = (byte)Math.Ceiling (Math.Log (d + 1, 128));
			if (lg == 0) {
				lg++;
			}
			for (int i = 0; i < lg; i++) {
				byte m = (byte)(d & 127);
				if (i + 1 == lg) {
					m |= 128;
				}
				d >>= 7;
				Output.Add (m);
			}
		}
		
		public long Decode (OctetReader reader)
		{
			long d = 0;
			int p = 0;
			while (true) {
				long m = reader.Read ();
				d |= (m & 127) << p;
				if ((m & 128) == 128) {
					break;
				}
				p += 7;
			}
			return d;
		}

		public void Save (BinaryWriter Output)
		{
		}
		
		public void Load (BinaryReader Input)
		{
		}

	}
}

