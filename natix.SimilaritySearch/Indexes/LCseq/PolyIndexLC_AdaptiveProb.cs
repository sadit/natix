////
////   Copyright 2013 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
////
////   Licensed under the Apache License, Version 2.0 (the "License");
////   you may not use this file except in compliance with the License.
////   You may obtain a copy of the License at
////
////       http://www.apache.org/licenses/LICENSE-2.0
////
////   Unless required by applicable law or agreed to in writing, software
////   distributed under the License is distributed on an "AS IS" BASIS,
////   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
////   See the License for the specific language governing permissions and
////   limitations under the License.
////
//
//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using natix.Sets;
//using natix.CompactDS;
//using natix.SortingSearching;
//
//namespace natix.SimilaritySearch
//{
//	public class PolyIndexLC_AdaptiveProb : PolyIndexLC
//	{
//        int _internalD = 0;
//
//		public PolyIndexLC_AdaptiveProb () : base()
//        {
//        }
//
//        public override SearchCost Cost {
//            get {
//                var cost = base.Cost;
//                cost.Internal += _internalD;
//                return cost;
//            }
//        }
//
//        public override IResult SearchRange (object q, double radius)
//        {
//            IResult R = this.DB.CreateResult (this.DB.Count, false);
//            Action<int> on_intersection = delegate(int item) {
//                var dist = this.DB.Dist (q, this.DB [item]);
//                if (dist <= radius) {
//                    R.Push (item, dist);
//                }
//            };
//            var cache = new Dictionary<int, double>(this.LC_LIST[0].CENTERS.Count);
//            var num_indexes = this.GetNumIndexesSearchRange(q, R, radius, cache);
//            if (num_indexes <= 0) {
//                // num_indexes = this.LC_LIST.Count;
//                var count = this.DB.Count;
//                for (int objID = 0; objID < count; ++objID) {
//                    if (!cache.ContainsKey(objID)) {
//                        var d = this.DB.Dist(this.DB[objID], q);
//                        R.Push(objID, d);
//                    }
//                }
//                return R;
//            }
//            return this.PartialSearchRange (q, radius, R, num_indexes, cache, on_intersection);
//        }
//
//        public override IResult SearchKNN (object q, int K, IResult R)
//        {
//            Action<int> on_intersection = delegate(int item) {
//                var dist = this.DB.Dist (q, this.DB [item]);
//                R.Push (item, dist);
//            };
//            var cache = new Dictionary<int, double> (this.LC_LIST [0].CENTERS.Count);
//            var num_indexes = this.GetNumIndexesSearchKNN (q, R, cache);
//            if (num_indexes <= 0) {
//                // num_indexes = this.LC_LIST.Count;
//                var count = this.DB.Count;
//                for (int objID = 0; objID < count; ++objID) {
//                    if (!cache.ContainsKey(objID)) {
//                        var d = this.DB.Dist(this.DB[objID], q);
//                        R.Push(objID, d);
//                    }
//                }
//                return R;
//            }
//            Console.WriteLine ("XXXXXXXXXXXXXXXXX USING INDEXES: {0}/{1}", num_indexes, this.LC_LIST.Count);
//            return this.PartialSearchKNN (q, K, R, num_indexes, cache, on_intersection);
//        }
//             
//        protected int GetNumIndexesSearchRange (object q, IResult R, double radius, Dictionary<int, double> cache)
//        {
//            var lc = this.LC_LIST [0];
//            var review = 0;
//            var num_centers = lc.CENTERS.Count;
//            for (int centerID = 0; centerID < num_centers; ++centerID) {
//                var centerOBJ = lc.CENTERS[centerID];
//                var dcq = lc.DB.Dist (lc.DB[centerOBJ], q);
//                ++this._internalD;
//                cache[centerOBJ] = dcq;
//                if (dcq <= radius) {
//                    R.Push(centerOBJ, dcq);
//                    if (dcq <= radius + lc.COV[centerID]) {
//                        ++review;
//                    }
//                }
//            }
//            var prob_review = ((double)review) / num_centers;
//            if (prob_review == 1) {
//                return 0;
//            }
//            // n G_u = m k + n P_u^k
//            // now, we need to minimize k
//            var x = - this.DB.Count * Math.Log (prob_review);
//            var k = (int)(Math.Log (num_centers, prob_review) - Math.Log (x, prob_review)); 
//            // return k;
//            // TODO: If k - this.LC_LIST.Count is very large then we expect a very expensive search, then we must perform a sequential search
//            return Math.Min (this.LC_LIST.Count, k);
//        }
//
//        protected int GetNumIndexesSearchKNN (object q, IResult R, Dictionary<int, double> cache)
//        {
//            var lc = this.LC_LIST [0];
//            var review = 0;
//            var num_centers = lc.CENTERS.Count;
//            for (int centerID = 0; centerID < num_centers; ++centerID) {
//                var centerOBJ = lc.CENTERS [centerID];
//                var dcq = lc.DB.Dist (lc.DB [centerOBJ], q);
//                ++this._internalD;
//                cache [centerOBJ] = dcq;
//                R.Push (centerOBJ, dcq);
//                if (dcq <= R.CoveringRadius + lc.COV [centerID]) {
//                    ++review;
//                }
//            }
//            var prob_review = ((double)review) / (num_centers);
//            if (prob_review == 1) {
//                return 0;
//            }
//            // n G_u = m k + n P_u^k
//            // now, we need to minimize k
//            var x = - this.DB.Count * Math.Log (prob_review);
//            var k = (int)(Math.Log (num_centers, prob_review) - Math.Log (x, prob_review)); 
//            return Math.Min (this.LC_LIST.Count, k);
//        }
// 	}
//}
