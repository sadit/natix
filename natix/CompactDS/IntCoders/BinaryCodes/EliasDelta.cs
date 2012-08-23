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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/EliasDelta.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class EliasDelta : IIEncoder32
	{
		static EliasGamma32 gammacoder = new EliasGamma32();

		public void Save (BinaryWriter Output)
		{
		}

		public void Load (BinaryReader Input)
		{
		}
		
		public EliasDelta ()
		{
		}
		
		public void Encode (IBitStream Buffer, int u)
		{
			if (u < 1) {
				throw new ArgumentOutOfRangeException (String.Format ("Invalid range for elias delta coding, u: {0}", u));
			}
			var log2 = BitAccess.Log2 (u);
			gammacoder.Encode (Buffer, log2);
			Buffer.Write (u, log2-1);
		}
		
		public int Decode (IBitStream Buffer, BitStreamCtx ctx)
		{
			int len_code = gammacoder.Decode (Buffer, ctx);
			len_code--;
			if (len_code == 0) {
				return 1;
			} else {
				int output = (int)Buffer.Read (len_code, ctx);
				// Console.WriteLine ("Decode> count: {0}, output: {1}", count, output);
				return (1 << len_code) + output;
			}
		}
	}
}

