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
//	public class PolyIndexLC_General : PolyIndexLC
//	{
//		public PolyIndexLC_Ada () : base()
//        {
//        }
//
//        public override IResult SearchRange (object q, double radius)
//        {
//            IResult R = this.DB.CreateResult (this.DB.Count, false);
//            var cache = new Dictionary<int,double> (this.LC_LIST[0].CENTERS.Count);
//            var queue_list = new List<IRankSelect>(64);
//            byte[] A = new byte[ this.DB.Count ];
//            int max = 0;
//            foreach (var I in this.LC_LIST) {
//                ++max;
//                queue_list.Clear ();
//                I.PartialSearchRange (q, radius, R, cache, queue_list);
//                foreach (var rs in queue_list) {
//                    var count1 = rs.Count1;
//                    for (int i = 1; i <= count1; ++i) {
//                        var item = rs.Select1 (i);
//                        A [item]++;
//                        if (A [item] == max) {
//                            var dist = this.DB.Dist (q, this.DB [item]);
//                            if (dist <= radius) {
//                                R.Push (item, dist);
//                            }
//                        }
//                    }
//                }
//            }
//            // var C = this.UI_ALG.ComputeUI (M);
//            return R;
//        }
//
//		public override IResult SearchKNN (object q, int K, IResult R)
//        {
//            var cache = new Dictionary<int,double> ();
//            var queue_dist = new List<double>();
//            var queue_list = new List<IRankSelect>();
//            int max = 0;
//            this.LC_LIST[max].PartialSearchKNN_Adaptive(q, K, R, cache, queue_dist, queue_list);
//            for (max = 1; max < this.LC_LIST.Count; ++max) {
//                double initial_dist = R.Last.dist;
//                this.LC_LIST[max].PartialSearchKNN_Adaptive(q, K, R, cache, queue_dist, queue_list);
//                double improving_ratio = - (R.Last.dist - initial_dist);
//                // Console.WriteLine ("XXX improving_ratio {0}, lambda: {1}, max-lambda: {2}", improving_ratio, max, this.LC_LIST.Count);
//                if (improving_ratio <= this.MinimumImprovingRatio) {
//                    break;
//                }
//            }
//
//            Sorting.Sort <double,IRankSelect> (queue_dist, queue_list);
//            byte[] A = new byte[ this.DB.Count ];
//            for (int i = 0; i < queue_dist.Count; ++i) {
//                var dcq_cov = queue_dist[i];
//                // Console.WriteLine ("dcq_cov: {0}, total: {1}, i: {2}, radius: {3}", dcq_cov, queue_dist.Count, i, R.CoveringRadius);
//                if (dcq_cov > R.CoveringRadius) {
//                    break;
//                }
//                var rs = queue_list[i];
//                var count1 = rs.Count1;
//				for (int rank = 1; rank <= count1; ++rank) {
//					var item = rs.Select1 (rank);
//					A [item]++;
//					if (A [item] == max) {
//						var dist = this.DB.Dist (q, this.DB [item]);
//						R.Push (item, dist);
//					}
//				}
//			}
//			return R;
//		}
//	}
//}
