//
//  Copyright 2013, 2014     Eric S. Tellez <eric.tellez@infotec.com.mx>
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
// 
// 2014, September. NextPivot is now threadsafe

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using natix;

namespace natix.SimilaritySearch
{

	public class PivotSelectorRandom : PivotSelector
	{
		int[] permutation;
		int curr = 0;
		
		public PivotSelectorRandom(int n, Random rand = null)
		{
			this.permutation = RandomSets.GetRandomPermutation (n, rand);
		}
		
		public override int NextPivot ()
		{
			int piv;
			lock (this) {
				piv = this.permutation [this.curr];
				++this.curr;
			}
			return piv;
		}
	}
}