//
//   Copyright 2013 Eric Sadit Tellez <donsadit@gmail.com>
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

// Based on the implementation of 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{


	public class BitParallelKnr8LLCS 
	{
		//int[] q;
		byte[] B;

		public BitParallelKnr8LLCS (int[] q, int shift, int sigma) : base()
		{
			//this.q = q;
			this.B = new byte[sigma];
			for (int i = 0; i < q.Length; ++i) {
				this.B [q [i] + shift] = (byte) (1 << i);
			}
		}


		public int llcs (int[] p)
		{
			int V = ~0;
			int tB;
			int cB;
			for (int i = 0; i < p.Length; ++i) {
				cB = this.B [p [i]];
				tB = (V & cB);
				V = ((V + tB) | (V - tB));
			}
			V = ~V;
			int _llncs = 0;
			while (V != 0) {
				V &= V - 1;
				++_llncs;
			}
			return _llncs;
//			void simpleOurs(mask *B, unsigned char *p2, int n)^M
//			{
//				register mask V, tB;^M
//				register unsigned char *pPtr;^M
//				register unsigned char *pEnd;^M
//				int i = 0;^M
//				pEnd = &p2[n];^M
//				V = ~((mask) 0);^M
//				pPtr = &p2[0];^M
//				while(pPtr < pEnd)^M
//				{^M
//					tB = (V & B[*pPtr++]);^M
//					V = ((V + tB) | (V - tB));^M
//				}^M
//				i = 0;^M
//				V = ~V;^M
//				while (V)^M
//				{ V &= V-1;^M
//					i++;^M
//				}^M
//				printf("LLCS: %i\n", i);^M
//				return
//			}
		}

		/// <summary>

		/// </summary>
		public static int llcs_diggested_pattern (byte[] Bp)
		{
			int V = ~0;
			int tB;
			int cB;
			for (int i = 0; i < Bp.Length; ++i) {
				cB = Bp [i];
				tB = (V & cB);
				V = ((V + tB) | (V - tB));
			}
			V = ~V;
			int _llncs = 0;
			while (V != 0) {
				V &= V - 1;
				++_llncs;
			}
			return _llncs;
		}

		public static float llcs_diggested_pattern_with_intersection (byte[] Bp)
		{
			int V = ~0;
			int tB;
			int cB;
			int intersection = 0;
			for (int i = 0; i < Bp.Length; ++i) {
				cB = Bp [i];
				tB = (V & cB);
				V = ((V + tB) | (V - tB));
				if (cB != 0) {
					++intersection;
				}
			}
			V = ~V;
			int _llncs = 0;
			while (V != 0) {
				V &= V - 1;
				++_llncs;
			}
			return _llncs / ((float) Bp.Length) + intersection;
		}
	}
}
