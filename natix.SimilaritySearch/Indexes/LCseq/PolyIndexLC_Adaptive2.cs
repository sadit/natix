////
////   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
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
////   Original filename: natix/SimilaritySearch/Indexes/PolyIndexLC.cs
//// 
//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using NDesk.Options;
//using natix.Sets;
//using natix.CompactDS;
//using natix.SortingSearching;
//
//namespace natix.SimilaritySearch
//{
//	public class PolyIndexLC_Adaptive2 : PolyIndexLC
//	{
//		public PolyIndexLC_Adaptive2 () : base()
//        {
//        }
//
//        public override IResult SearchRange (object q, double radius)
//        {
//            IResult R = new ResultRange(radius, this.DB.Count);
//            this.SearchKNN(q, this.DB.Count, R);
//            return R;
//        }
//
//        public override IResult SearchKNN (object q, int K, IResult R)
//        {
//            var cache = new Dictionary<int,double> (this.LC_LIST [0].CENTERS.Count);
//            var queue_list = new List<IRankSelect> (64);
//            //var queue_dist = new List<double>(64);
//
//            int last_index = 0;
//            int lambda = this.LC_LIST.Count;
//            /*var A = new HashSet<int> ();
//            var B = new HashSet<int> ();
//            */
//            var C = new byte[this.DB.Count];
//            float C_prob = 1;
//            var C_prev_counter = this.DB.Count;
//            for (last_index = 0; last_index < lambda; ++last_index) {
//                // B.Clear ();
//                queue_list.Clear ();
//                //queue_dist.Clear ();
//                var IDX = this.LC_LIST [last_index];
//                //IDX.PartialSearchKNN (q, K, R, cache, queue_dist, queue_list);
//                IDX.PartialSearchKNN (q, K, R, cache, null, queue_list);
//                int C_curr_counter = 0;
//                for (int c = 0; c < queue_list.Count; ++c) {
//                    var rs = queue_list[c];
//                    var count1 = rs.Count1;
//                    for (int i = 1; i <= count1; ++i) {
//                        var item = rs.Select1 (i);
//                        /*if (last_index == 0 || A.Contains (item)) {
//                            B.Add (item);
//                        }*/
//                        if (C[item] == last_index) {
//                            ++C[item];
//                            ++C_curr_counter;
//                        }
//                    }
//                }
//                float prob = ((float)C_curr_counter) / ((float)C_prev_counter);
//                // if prob <= C_prob (current <= previous), the indexes are keeping the               
//                // discarding power
//                // if prob > C_prob (current > preview), the discarding power has been
//                // reduced, we can stop the loop
//                // the constant 1.01 is a relaxing factor
//                //if (prob > C_prob*1.01) {
//                if (C_prev_counter == 0 || prob >= 0.9) {
//                    // if (C_curr_counter*1.20 >= C_prev_counter) {
//                    ++last_index;
//                    break;
//                }
//                C_prob = prob;
//                C_prev_counter = C_curr_counter;
//            }
//            Console.WriteLine ("XXXX last_index: {0}, prev_counter: {1}, review-prob: {2}", last_index, C_prev_counter, C_prob);
//            // var ctx = this.CreateQueryContext(q);
//            // var rad = R.CoveringRadius;
//            for (int item = 0; item < C.Length; ++item) {
//                // Console.WriteLine ("XXX checking {0}", item);
//                if (C[item] != last_index) {
//                    continue;
//                }
//                //if (this.MustReviewItem(C, last_index, q, item, rad, ctx)) {
//                    var dist = this.DB.Dist (q, this.DB [item]);
//                    //if (dist <= rad) {
//                        R.Push (item, dist);
//                    //}
//                //}
//            }
//            return R;
//        }
// 	}
//}
