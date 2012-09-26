//
//  Copyright 2012  Eric Sadit Tellez Avila
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections;
using System.Collections.Generic;
using natix;

namespace natix.CompactDS
{
	public delegate IList<int> ListIBuilder (IList<int> list, int maxvalue);

	public class ListIBuilders
	{
		public ListIBuilders ()
		{
		}

		public static ListIBuilder GetListIFS ()
		{
			return delegate (IList<int> L, int maxvalue) {
				var numbits = ListIFS.GetNumBits(maxvalue);
				if (numbits == 8) {
					var p = new ListIFS8();
					foreach (var u in L) {
						p.Add(u);
					}
					return p;
				} else if (numbits == 4) {
					var p = new ListIFS4();
					foreach (var u in L) {
						p.Add(u);
					}
					return p;

				} else {
					var p = new ListIFS(numbits);
					foreach (var u in L) {
						p.Add(u);
					}
					return p;
				}
			};
		}

		public static ListIBuilder GetListIDiffs (short bsize,
		                                          BitmapFromBitStream marks_builder = null,
		                                          IIEncoder32 encoder = null)
		{
			return delegate (IList<int> L, int maxvalue) {
				var p = new ListIDiffs();
				p.Build(L, bsize, marks_builder, encoder); 
				return p;
			};
		}

		public static ListIBuilder GetListIRS64 (BitmapFromList64 bitmap_builder)
		{
			return delegate (IList<int> L, int maxvalue) {
				var p = new ListIRS64();
				p.Build(L, maxvalue, bitmap_builder);
				return p;
			};
		}

		public static ListIBuilder GetListEqRL ()
		{
			return delegate (IList<int> L, int maxvalue) {
				var p = new ListEqRL();
				p.Build(L, maxvalue);
				return p;
			};
		}

		public static ListIBuilder GetListRL ()
		{
			return delegate (IList<int> L, int maxvalue) {
				var p = new ListRL();
				p.Build(L, maxvalue);
				return p;
			};
		}

		public static ListIBuilder GetArray ()
		{
			return delegate (IList<int> L, int maxvalue) {
				int[] p = new int[L.Count];
				for (int i = 0; i < p.Length; ++i) {
					p[i] = L[i];
				}
				return p;
			};
		}

	}
}

