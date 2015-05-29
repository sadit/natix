//
//  Copyright 2013-2015  Eric Sadit Tellez Avila <eric.tellez@infotec.com.mx>
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


namespace natix.SimilaritySearch
{
    public class SAT_ApproxSearch : SAT
    {

        public SAT_ApproxSearch () : base()
        {
        }

        public void Build (SAT sat)
        {
            this.root = sat.root;
            this.DB = sat.DB;
        }

        public override IResult SearchKNN (object q, int K, IResult res)
		{
			return this._SearchKNN (q, K, res);
		}

		public IResult _SearchKNN (object q, int K, IResult res)
        {
            if (this.root == null) {
                return res;
            }
            var dist_root = this.DB.Dist(q, this.DB[this.root.objID]);
            res.Push(this.root.objID, dist_root);

			if (this.root.Children.Count > 0) {
				this.ApproxSearchKNNNode (this.root, q, res);
			}
            return res;
        }

		protected void ApproxSearchKNNNode (Node node, object q, IResult res)
        {
            // res.Push (node.objID, dist);
			var D = new double[node.Children.Count];
			var closer_child = node.Children[0];
			var closer_dist = this.DB.Dist(q, this.DB[closer_child.objID]);
			res.Push(closer_child.objID, closer_dist);
			D[0] = closer_dist;
			for (int i = 1; i < D.Length; ++i) {
				var child = node.Children[i];
				D[i] = this.DB.Dist(q, this.DB[child.objID]);
				res.Push(child.objID, D[i]);
				if (D[i] < closer_dist) {
					closer_dist = D[i];
					closer_child = child;
				}
			}
			if (closer_child.Children.Count > 0) {
				this.ApproxSearchKNNNode (closer_child, q, res);
			}
         
        }
    }
}

