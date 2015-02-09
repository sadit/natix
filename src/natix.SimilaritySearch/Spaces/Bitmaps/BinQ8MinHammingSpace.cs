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
//   Original filename: natix/SimilaritySearch/Spaces/BinaryHammingSpace.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Jaccard for bit strings
	/// </summary>
	public class BinQ8MinHammingSpace : BinQ8HammingSpace
	{
		public override double Dist (object a, object b)
		{
			this.numdist++;
			return DistMinHamming((byte[]) a, (byte[]) b, this.symlen);
		}

		public static double DistMinHamming (byte[] a, byte[] b, int symlen)
		{
			int min = int.MaxValue;
			if (a.Length < b.Length) {
				var w = a;
				a = b;
				b = w;
			}
			int bL = b.Length;
			int aL = a.Length - bL;
			int d;
			//Console.WriteLine ("aL: {0} bL: {1}, symlen: {2}", aL, bL, this.symlen);
			for (int askip = 0; askip <= aL; askip += symlen) {
				d = 0;
				for (int bskip = 0, abskip = askip; bskip < bL; bskip++,abskip++) {
					// Console.WriteLine ("a:{0}, b:{1}, A: {2}, B: {3}", askip, bskip, a[askip], b[bskip]);
					d += Bits.PopCount8[a[abskip] ^ b[bskip]];
				}
				if (min > d) {
					min = d;
				}
			}
			return min;
		}

	}
}
