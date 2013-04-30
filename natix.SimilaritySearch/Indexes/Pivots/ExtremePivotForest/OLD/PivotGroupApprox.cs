//
//  Copyright 2013  Eric Sadit Téllez Avila
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
	public class PivotGroupApprox : PivotGroup
	{
		double ApproxFactor = 1;

		public PivotGroupApprox () : base()
		{
		}

		public void Build (PivotGroup group, double approx_factor)
		{
			this.Pivs = group.Pivs;
			this.Items = group.Items;
			this.ApproxFactor = approx_factor;
		}

		public override int SearchKNN (MetricDB db, object q, int K, IResult res, short[] A, short current_rank_A)
		{
			int abs_pos = 0;
			int count_dist = 0;
			foreach (var piv in this.Pivs) {
				var pivOBJ = db [piv.objID];
				var dqp = db.Dist (q, pivOBJ);
				res.Push (piv.objID, dqp);
				++count_dist;
				// checking near ball radius
				if (dqp <= piv.last_near + res.CoveringRadius * this.ApproxFactor) {
					for (int j = 0; j < piv.num_near; ++j, ++abs_pos) {
						var item = this.Items [abs_pos];
						// checking covering pivot
						if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
							++A [item.objID];
						}
					}
				} else {
					abs_pos += piv.num_near;
				}
				// checking external radius
				if (dqp + res.CoveringRadius * this.ApproxFactor >= piv.first_far) {
					for (int j = 0; j < piv.num_far; ++j, ++abs_pos) {
						var item = this.Items [abs_pos];
						// checking covering pivot
						if (Math.Abs (item.dist - dqp) <= res.CoveringRadius) {
							++A [item.objID];
						}
					}
				} else {
					abs_pos += piv.num_far;
				}
				if (dqp + res.CoveringRadius*this.ApproxFactor <= piv.last_near || piv.first_far <= dqp - res.CoveringRadius*this.ApproxFactor) {
					break;
				}
			}
			return count_dist;
		}
	}
}

