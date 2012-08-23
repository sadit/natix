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
//   Original filename: natix/Sets/UnionIntersection/FastUIHashTable.cs
// 
using System;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.Sets
{
	public class FastUIHashTable : IUnionIntersection
	{
		public FastUIHashTable ()
		{
		}
		
		public IList<int> ComputeUI (IList<IList<IRankSelect>> sets)
		{
			var A = new Dictionary<int, int> ();
			var lambda = sets.Count;
			var L = new List<int> ();
			foreach (var list in sets) {
				foreach (var rs in list) {
					// foreach (var item in alist) {
					var count1 = rs.Count1;
					for (int i = 1; i <= count1; ++i) {
						var item = rs.Select1 (i);
						int counter;
						if (A.TryGetValue (item, out counter)) {
							counter++;
						} else {
							counter = 1;
						}
						A [item] = counter;
						if (counter == lambda) {
							L.Add (item);
						}
					}
				}
			}
			return L;
		}
	}
}

