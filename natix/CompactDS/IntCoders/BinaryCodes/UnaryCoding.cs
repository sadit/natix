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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/UnaryCoding.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class UnaryCoding : IIEncoder32
	{
	
		public UnaryCoding ()
		{
		}
	
		public void Encode (IBitStream Buffer, int u)
		{
			if (u < 0) {
				throw new ArgumentOutOfRangeException (String.Format ("Invalid range for UnaryCoding, u: {0}", u));
			}
			Buffer.Write (false, u);
			Buffer.Write (true);
			//Buffer.Write (true, u);
			//Buffer.Write (false);

		}
		
		public int Decode (IBitStream Buffer, BitStreamCtx ctx)
		{
			int u = Buffer.ReadZeros (ctx);
			//int u = Buffer.ReadOnes ();
			Buffer.Read (ctx);
			return u;
		}
		
		public void Save (BinaryWriter Output)
		{
		}

		public void Load (BinaryReader Input)
		{
		}

	}
}
