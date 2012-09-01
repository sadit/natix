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
	public class SimpleHashIntersection
	{
		public SimpleHashIntersection ()
		{
		}

		public virtual HashSet<int> Intersection (IList<IRankSelect> lists)
		{
			HashSet<int> A = new HashSet<int> ();
			HashSet<int> B = new HashSet<int> ();

			var rs = lists[0];
			var count1 = rs.Count1;
			for (int i = 1; i <= count1; ++i) {
				A.Add( rs.Select1(i) );
			}
			for(int s = 1; s < lists.Count; ++s) {
				rs = lists[s];
				count1 = rs.Count1;
				for (int i = 1; i <= count1; ++i) {
					var pos = rs.Select1 (i);
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

