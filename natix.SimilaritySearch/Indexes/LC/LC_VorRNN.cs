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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
    public class LC_VorRNN : LC_RNN
    {
        public LC_VorRNN () : base()
        {            
        }

        /// <summary>
        /// Search the specified q with radius qrad.
        /// </summary>
        public override IResult SearchRange (object q, double qrad)
        {
            var sp = this.DB;
            var R = sp.CreateResult (int.MaxValue, false);
            int num_centers = this.CENTERS.Count;
            var D = new double[num_centers];
            var d_closer = double.MaxValue;
            for (int centerID = 0; centerID < num_centers; centerID++) {
                var dcq = sp.Dist (this.DB [this.CENTERS [centerID]], q);
                D[centerID] = dcq;
                d_closer = Math.Min (dcq, d_closer);
            }
            for (int centerID = 0; centerID < num_centers; centerID++) {
                var dcq = D[centerID];
                if (dcq <= qrad) {
                    R.Push (this.CENTERS [centerID], dcq);
                }
                if (dcq <= d_closer + 2 * qrad && dcq <= qrad + this.COV [centerID]) {
                    var rs = this.SEQ.Unravel (centerID);
                    var count1 = rs.Count1;
                    for (int i = 1; i <= count1; i++) {
                        var u = rs.Select1 (i);
                        var r = sp.Dist (q, sp [u]);
                        if (r <= qrad) {
                            R.Push (u, r);
                        }
                    }
                }
            }
            return R;
        }
        
        /// <summary>
        /// KNN search.
        /// </summary>
        public override IResult SearchKNN (object q, int K, IResult R)
        {
            var sp = this.DB;
            int num_centers = this.CENTERS.Count;
            var C = this.DB.CreateResult (num_centers, false);
            for (int centerID = 0; centerID < num_centers; centerID++) {
                var dcq = sp.Dist (this.DB [this.CENTERS [centerID]], q);
                ++this.internal_numdists;
                R.Push (this.CENTERS [centerID], dcq);
                if (dcq <= R.CoveringRadius + this.COV [centerID]) {
                    C.Push (centerID, dcq);
                }
            }
            var closer = C.First;
            foreach (ResultPair pair in C) {
                var dcq = pair.dist;
                var center = pair.docid;
                if (dcq <= closer.dist + 2 * R.CoveringRadius &&
                    dcq <= R.CoveringRadius + this.COV [center]) {
                    var rs = this.SEQ.Unravel (center);
                    var count1 = rs.Count1;
                    for (int i = 1; i <= count1; i++) {
                        var u = rs.Select1 (i);
                        var r = sp.Dist (q, sp [u]);
                        //if (r <= qr) { // already handled by R.Push
                        R.Push (u, r);
                    }
                }
            }
            return R;
        }
        
        public override void PartialSearchKNN (object q, int K, IResult R, IDictionary<int,double> cache, List<double> queue_dist, List<IRankSelect> queue_list)
        {
            var sp = this.DB;
            int num_centers = this.CENTERS.Count;
            var d_closer = double.MaxValue;
            for (int centerID = 0; centerID < num_centers; centerID++) {
                var center_objID = this.CENTERS[centerID];
                double dcq;
                if (!cache.TryGetValue(center_objID, out dcq)) {
                    dcq = sp.Dist (this.DB [center_objID], q);
                    ++this.internal_numdists;
                    cache[center_objID] = dcq;
                    R.Push (center_objID, dcq);
                }
                d_closer = Math.Min (dcq, d_closer);
            }
            for (int centerID = 0; centerID < num_centers; centerID++) {
                var center_objID = this.CENTERS[centerID];
                double dcq = cache[ center_objID ];
                var cov = this.COV[centerID];
                if (dcq <= d_closer + 2 * R.CoveringRadius && dcq <= R.CoveringRadius + cov) {
                    // if (dcq_cov <= R.CoveringRadius) {
                    var list = this.SEQ.Unravel(centerID);
                    if (queue_dist != null) {
                        queue_dist.Add(dcq - cov);
                    }
                    queue_list.Add(list);
                }
            }
        }

        /// <summary>
        /// Partial radius search
        /// </summary>
        public override void PartialSearchRange (object q, double qrad, IResult R, IDictionary<int,double> cache, List<IRankSelect> queue_list)
        {
            var sp = this.DB;
            int num_centers = this.CENTERS.Count;
            var d_closer = double.MaxValue;
            for (int centerID = 0; centerID < num_centers; centerID++) {
                var center_objID = this.CENTERS[centerID];
                double dcq;
                if (!cache.TryGetValue(center_objID, out dcq)) {
                    dcq = sp.Dist (this.DB [center_objID], q);
                    ++this.internal_numdists;
                    cache[center_objID] = dcq;
                    if (dcq <= qrad) {
                        R.Push (center_objID, dcq);
                    }
                }
                d_closer = Math.Min (dcq, d_closer);
            }
            for (int centerID = 0; centerID < num_centers; centerID++) {
                var center_objID = this.CENTERS[centerID];
                double dcq = cache[ center_objID ];
                var cov = this.COV[centerID];
                if (dcq <= d_closer + 2 * qrad && dcq <= qrad + cov) {
                    // if (dcq_cov <= R.CoveringRadius) {
                    var list = this.SEQ.Unravel(centerID);
                    queue_list.Add(list);
                }
            }

        }
    }
}

