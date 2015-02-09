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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix;
using natix.CompactDS;

namespace natix.Sets
{
	public class SimpleHashIntersection : INumericIntersection
	{
		public SimpleHashIntersection ()
		{
		}

		public virtual ICollection<int> Intersection (IList<IList<int>> lists)
		{
			HashSet<int> A = new HashSet<int> ();
			HashSet<int> B = new HashSet<int> ();

			var L = lists[0];
			var count1 = L.Count;
			for (int i = 0; i < count1; ++i) {
				A.Add( L[i] );
			}
			for(int s = 1; s < lists.Count; ++s) {
				L = lists[s];
				count1 = L.Count;
				for (int i = 0; i < count1; ++i) {
					var pos = L[i];
					if (A.Contains (pos)) {
						B.Add (pos);
					}
				}
				var C = A;
				A = B;
				B = C;
				B.Clear();
			}
			return A;
		}
	}
}
