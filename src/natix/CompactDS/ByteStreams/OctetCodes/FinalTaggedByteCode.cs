//
//   Copyright 2012,2013,2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
// Sep. 2014, complete rewrite
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
		
		public void Encode (long d, OctetStream Output)
		{
			long m;
			while (true) {
				m = d & 127;
				d >>= 7;
				if (d == 0) {
					Output.Add ((byte)(m | 128));
					break;
				} else {
					Output.Add ((byte) m);
				}
			}
		}
		
		public long Decode (OctetStream reader, OctetStream.Context ctx)
		{
			long d = 0;
			int p = 0;
			while (true) {
				long m = reader.Read (ctx);
				d |= (m & 127) << p;
				if ((m & 128) != 0) {
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

