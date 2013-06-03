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
//   Original filename: natix/Sets/UnionIntersection/FastUIArray8.cs
// 
using System;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.Sets
{
	public class FastUIArray8 : IUnionIntersection
	{
		int N;
		public FastUIArray8 (int N)
		{
			this.N = N;
		}
		
		public IList<int> ComputeUI (IList<IList<Bitmap>> sets)
		{
			byte[] A = new byte[this.N];
			var lambda = sets.Count;
			var L = new List<int> ();
			foreach (var list in sets) {
				foreach (var rs in list) {
					// foreach (var item in alist) {
					var count1 = rs.Count1;
					for (int i = 1; i <= count1; ++i) {
						var item = rs.Select1 (i);
						A [item]++;
						if (A [item] == lambda) {
							L.Add (item);
						}
					}
				}
			}
			return L;
		}
	}
}

