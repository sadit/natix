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
using natix.SortingSearching;
using natix.CompactDS;

namespace natix.Sets
{
	public class SvSHashIntersection : SimpleHashIntersection
	{
		public SvSHashIntersection () : base()
		{
		}

		public override HashSet<int> Intersection (IList<IRankSelect> lists)
		{
			Sorting.Sort<IRankSelect>(lists, (x, y) => x.Count1.CompareTo(y.Count1));
			return base.Intersection(lists);
		}
	}
}

