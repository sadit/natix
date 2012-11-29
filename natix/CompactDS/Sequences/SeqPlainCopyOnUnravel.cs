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
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class SeqPlainCopyUnravel : SeqPlain
	{
		public SeqPlainCopyUnravel (): base()
		{
		}

		public override IRankSelect Unravel (int sym)
        {
            var L = new List<int> ();
            var n = this.SEQ.Count;
            for (int i = 0; i < n; ++i) {
                if (this.SEQ[i] == sym) {
                    L.Add (i);
                }
            }
            var slist = new PlainSortedList();
            slist.Build(L, n);
            return slist;
		}
	}
}

