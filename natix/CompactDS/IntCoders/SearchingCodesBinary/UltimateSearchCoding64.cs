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
//   Original filename: natix/CompactDS/IntCoders/SearchingCodesBinary/UltimateSearchCoding64.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	public class UltimateSearchCoding64 : IIEncoder64
	{
		
		public UltimateSearchCoding64 ()
		{
		}
		
		public void Save (BinaryWriter Output)
		{
		}

		public void Load (BinaryReader Input)
		{
		}
				
		public void Encode (IBitStream stream, long u)
		{
			long min = 0;
			long check;
			int galloping = 1;
			while (true) {
				check = (1L << galloping) - 1L;
				if (u > check) {
					stream.Write (true);
					min = check + 1L;
					++galloping;
				} else {
					stream.Write (false);
					if (galloping == 1) {
						stream.Write (u == 1L);
						return;
					} else {
						u -= min;
						min = 0L;
						galloping = 1;
					}
				}
			}
		}
		
		public long Decode (IBitStream stream, BitStreamCtx ctx)
		{
			long min = 0;
			long check;
			long u = 0;
			int galloping = 1;
			while (true) {
				check = (1L << galloping) - 1L;
				if (stream.Read (ctx)) {
					min = check + 1L;
					++galloping;
				} else {
					if (galloping == 1) {
						if (stream.Read (ctx)) {
							u++;
						}
						return u;
					} else {
						u += min;
						min = 0L;
						galloping = 1;
					}
				}
			}
		}
	}
}
