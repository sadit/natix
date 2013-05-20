////
////  Copyright 2013  Eric Sadit Tellez Avila
////
////    Licensed under the Apache License, Version 2.0 (the "License");
////    you may not use this file except in compliance with the License.
////    You may obtain a copy of the License at
////
////        http://www.apache.org/licenses/LICENSE-2.0
////
////    Unless required by applicable law or agreed to in writing, software
////    distributed under the License is distributed on an "AS IS" BASIS,
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
////    See the License for the specific language governing permissions and
////    limitations under the License.
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
//    public class PolyIndexLC_ApproxGraph : PolyIndexLC
//    {
//        public int RepeatTimes;
//        public int FailTimes;
//
//        public PolyIndexLC_ApproxGraph () : this(5, 5)
//        {
//        }
//
//        public PolyIndexLC_ApproxGraph (int repeat, int failtimes) : base()
//        {
//            this.RepeatTimes = repeat;
//            this.FailTimes = failtimes;
//        }
//
//        public override IResult SearchRange (object q, double radius)
//        {
//            var R = new ResultRange(radius);
//            return this.SearchKNN(q, this.DB.Count, R);
//        }
//
//        public override IResult SearchKNN (object q, int K, IResult res_global)
//        {
//            var lambda = this.LC_LIST.Count;
//            Random rand = new Random ();
//            var n = this.DB.Count;
//            int centerID = -1;         
//            var C = new BitStream32 ();
//            var M = new BitStream32[lambda];
//            C.Write (false, n);
//            for (int _lambda = 0; _lambda < lambda; ++_lambda) {
//                M [_lambda] = new BitStream32 ();
//                M [_lambda].Write (false, this.LC_LIST [_lambda].CENTERS.Count);
//            }
//            int repeat_search_step = this.RepeatTimes;
//            int num_failures = this.FailTimes;
//            for (int _try = 0; _try < repeat_search_step; ++_try) {
//                var res_inner = new Result(res_global.K, res_global.Ceiling);
//                // Console.WriteLine("try: {0} / {1}", _try, repeat_search_step);
//                var i = -1;
//                var starting = true;
//                while (true) {
//                    i = (i + 1) % lambda;
//                    var lc = this.LC_LIST [i];
//                    var _M = M [i];
//                    if (res_inner.Count == 0 || starting) {
//                        centerID = (int)(rand.NextDouble () * lc.CENTERS.Count);
//                        if (num_failures > 0 && _M[centerID]) {
//                            --num_failures;
//                            continue;
//                        }
//                        starting = false;
//                    } else {
//                        centerID = lc.SEQ.Access (res_inner.First.docid);
//                        if (centerID == lc.CENTERS.Count) {
//                            // the nearest is in the set of centers, skipping this index
//                            continue;
//                        }
//                    }
//                    if (_M [centerID]) {
//                        break;
//                    }
//                    _M[centerID] = true;
//                    var rs = lc.SEQ.Unravel (centerID);
//                    var count1 = rs.Count1;
//                    var prev_radius = res_inner.CoveringRadius;
//                    for (int rank = 1; rank <= count1; ++rank) {
//                        var pos = rs.Select1 (rank);
//                        if (! C [pos]) {
//                            C [pos] = true;
//                            var d = this.DB.Dist (this.DB [pos], q);
//                            res_inner.Push (pos, d);
//                            res_global.Push(pos, d);
//                        }
//                    }
//                }
//            }
//            return res_global;
//        }
//    }
//}