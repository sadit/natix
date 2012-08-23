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
//   Original filename: natix/CompactDS/IntCoders/SearchingCodesBinary/UltimateSearchCoding.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	public class UltimateSearchCoding : IIEncoder32
	{
		
		public UltimateSearchCoding ()
		{
		}
		
		public void Save (BinaryWriter Output)
		{
		}

		public void Load (BinaryReader Input)
		{
		}
				
		public void Encode (IBitStream stream, int u)
		{
			int min = 0;
			int galloping = 1;
			int check;
			while (true) {
				check = (1 << galloping) - 1;
				if (u > check) {
					stream.Write (true);
					min = check + 1;
					++galloping;
				} else {
					stream.Write (false);
					if (galloping == 1) {
						stream.Write(u == 1);
						return;
					} else {
						u -= min;
						min = 0;
						galloping = 1;
					}
				}
			}
		}
		
		public int Decode (IBitStream stream, BitStreamCtx ctx)
		{
			int min = 0;
			int galloping = 1;
			int check;
			int u = 0;
			while (true) {
				check = (1 << galloping) - 1;
				if (stream.Read (ctx)) {
					min = check + 1;
					++galloping;
				} else {
					if (galloping == 1) {
						if (stream.Read(ctx)) {
							u++;
						}
						return u;
					} else {
						u += min;
						min = 0;
						galloping = 1;
					}
				}
			}
		}
	}
}

