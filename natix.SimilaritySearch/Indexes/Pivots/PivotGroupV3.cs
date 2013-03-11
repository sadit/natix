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
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class PivotGroupV3 : PivotGroup
	{
		public PivotGroupV3 () : base()
		{
		}

        protected override void SearchExtremes (DynamicSequential idx, List<DynamicSequential.Item> items, object piv, double alpha_stddev, int min_bs, out IResult near, out IResult far, out DynamicSequential.Stats stats)
        {
            items.Clear();
            idx.ComputeDistances (piv, items, out stats);
            var radius = stats.min; // stats.stddev * alpha_stddev;
            near = new Result (idx.Count);
            far = new Result (idx.Count);
            idx.DropCloseToMean (stats.mean - radius, stats.mean + radius, near, far, items);
            if (near.Count == 0 && far.Count == 0 & min_bs > 0) {
                idx.AppendKExtremes(min_bs, near, far, items);
            }
        }
	}
}

