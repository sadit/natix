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
//   Original filename: natix/CompactDS/BitStreams/Bits.cs
// 
using System;

namespace natix.CompactDS
{
	public class Bits
	{
		/// <summary>
		/// Table for counting enabled bits (for nibbles)
		/// </summary>
		// 0 = 0000, 1 = 0001, 2 = 0010, 3 = 0011, 4 = 0100, 5 = 0101, 6 = 0110, 7 = 0111
		// 8 = 1000, 9 = 1001, A = 1010, B = 1011, C = 1100, D = 1101, E = 1110, F = 1111
		public static byte[] PopCount4 = new byte[16] {
			0,1,1,2,1,2,2,3,
			1,2,2,3,2,3,3,4
		};
		
		/// <summary>
		/// Table for counting enabled bits (byte size)
		/// </summary>
		public static byte[] PopCount8;
		
		/// <summary>
		/// Count the number of enabled bits in an UInt16
		/// </summary>
		//public static byte[] HammingTable16;
		
		static Bits ()
		{
			PopCount8 = new byte[256];
			for (uint i = 0; i < 256; i++) {
				PopCount8 [i] = (byte)(PopCount4 [i & 0x0F] + PopCount4 [i >> 4]);
			}
			/*HammingTable16 = new byte[UInt16.MaxValue+1];
			for (uint i = 0; i <= UInt16.MaxValue; i++) {
				HammingTable16[i] = (byte)(HammingTable8[i & 0x00FF] + HammingTable8[ i >> 8]);
			}*/
		}

		
		public Bits ()
		{
		}
	}
}

