//
//  Copyright 2013  Eric Sadit Tellez Avila
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

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class PivotGroupIndexApprox : PivotGroupIndex
	{
		public PivotGroupIndexApprox () : base()
		{
		}
       
		public void Build (PivotGroupIndex pgi, int num_groups, double approx_factor)
		{
			this.DB = pgi.DB;
			if (num_groups <= 0) {
				num_groups = pgi.GROUPS.Length;
			}
			this.GROUPS = new PivotGroupApprox[num_groups];
			for (int i = 0; i < num_groups; ++i) {
				var g = new PivotGroupApprox();
				g.Build(pgi.GROUPS[i], approx_factor);
				this.GROUPS[i] = g;
			}
		}
	}
}

