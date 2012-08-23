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
	public class BinQ8JaccardSpace : BinQ8HammingSpace
	{
		public override double Dist (object a, object b)
		{
			this.numdist++;
			return DistMinJaccard((IList<byte>) a, (IList<byte>) b, this.symlen);
		}

		public static double DistMinJaccard(IList<byte> a, IList<byte> b, int symlen)
		{
			double min = double.MaxValue;
			if (a.Count < b.Count) {
				IList<byte> w = a;
				a = b;
				b = w;
			}
			int bL = b.Count;
			int aL = a.Count - bL;
			for (int askip = 0; askip <= aL; askip += symlen) {
				float I = 0;
				float U = 0;

				for (int bskip = 0, abskip = askip; bskip < bL; bskip++,abskip++) {
					I += Bits.PopCount8[a[abskip] & b[bskip]];
					U += Bits.PopCount8[a[abskip] | b[bskip]];
				}
				float d = 1 - I / U;
				if (min > d) {
					min = d;
				}
			}
			return min;
		}
	}
}
