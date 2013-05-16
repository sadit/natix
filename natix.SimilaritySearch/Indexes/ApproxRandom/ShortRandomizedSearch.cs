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

using System;
using System.Collections.Generic;
using natix;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	public class ShortRandomizedSearch : BasicIndex
	{
		Random rand;

		public ShortRandomizedSearch () : base()
		{
		}

		public void Build (MetricDB db)
		{
			this.DB = db;
			this.rand = new Random ();
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var n = this.DB.Count;
			for (int i = 0; i < K; ++i) {
				var objID = this.rand.Next (0, n);
				var d = this.DB.Dist (q, this.DB [objID]);
				res.Push (objID, d);
			}
			return res;
		}
	}
}

