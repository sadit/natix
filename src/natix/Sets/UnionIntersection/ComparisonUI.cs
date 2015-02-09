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
//   Original filename: natix/Sets/UnionIntersection/ComparisonUI.cs
// 
using System;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.Sets
{
	public class ComparisonUI : IUnionIntersection
	{
		IIntersection<int> ialg;
		
		public ComparisonUI (IIntersection<int> alg)
		{
			this.ialg = alg;
		}
		
		public IList<int> ComputeUI (IList<IList<Bitmap>> sets)
		{
			var L = new IList<int>[sets.Count];
			int i = 0;
			foreach (var alist in sets) {
				L [i] = this.Union (alist);
				i++;
			}
			var u = this.ialg.Intersection (L);
			var uL = u as IList<int>;
			if (uL != null) {
				return uL;
			}
			return new List<int> (u);
		}
		
		IList<int> Union (IList<Bitmap> disjoint_sets)
		{
			HashSet<int > S = new HashSet<int> ();
			foreach (var rs in disjoint_sets) {
				var count1 = rs.Count1;
				for (int i = 1; i < count1; ++i) {
					//foreach (var item in list) {
					var item = rs.Select1 (i);
					S.Add (item);
				}
			}
			var L = new int[S.Count];
			{
				int i = 0;
				foreach (var item in S) {
					L [i] = item;
					i++;
				}
			}
			Array.Sort (L);
			return L;
		}
	}
}
