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
//using System.Threading;
//using System.Threading.Tasks;
//
//namespace natix.SimilaritySearch
//{
//	public abstract class PolyIndexLC_Partial : BasicIndex, IndexSingle
//	{
//		public IList<LC_RNN> LC_LIST;
//		// public IUnionIntersection UI_ALG;
//
//		public PolyIndexLC_Partial ()
//		{
//		}
//
//        protected virtual Action BuildOneClosure (IList<LC_RNN> output, int i, MetricDB db, int numcenters, Random rand, SequenceBuilder seq_builder)
//        {
//            var action = new Action(delegate () {
//                var lc = new LC ();
//                //var lc = new LC_RNN();
//                lc.Build (db, numcenters, rand, seq_builder);
//                output[i] = lc;
//            });
//            return action;
//        }
//
//		public virtual void Build (IList<LC_RNN> indexlist, int max_instances = 0, SequenceBuilder seq_builder = null)
//        {
//            this.LC_LIST = new List<LC_RNN> ();
//            if (max_instances <= 0) {
//                max_instances = indexlist.Count;
//            }
//			for (int i = 0; i < max_instances; ++i) {
//                var lc = indexlist[i];
//                if (seq_builder == null) {
//                    this.LC_LIST.Add(lc);
//                } else {
//                    var s = new LC_RNN();
//                    s.Build(lc, seq_builder);
//                    this.LC_LIST.Add(s);
//                }
//			}
//		}
//
//		public override MetricDB DB {
//			get {
//				return this.LC_LIST[0].DB;
//			}
//			set {
//			}
//		}
//
//		public override void Save (BinaryWriter Output)
//		{
//			Output.Write((int) this.LC_LIST.Count);
//			for (int i = 0; i < this.LC_LIST.Count; ++i) {
//				IndexGenericIO.Save(Output, this.LC_LIST[i]);
//			}
//		}
//
//		public override void Load (BinaryReader Input)
//		{
//			var count = Input.ReadInt32 ();
//			this.LC_LIST = new LC_RNN [count];
//			for (int i = 0; i < count; ++i) {
//				this.LC_LIST[i] = (LC_RNN)IndexGenericIO.Load(Input);
//			}
//			// this.UI_ALG = new FastUIArray8 (this.DB.Count);
//		}
//
//        protected virtual IResult PartialSearchRange (object q, double radius, IResult R, int max, Dictionary<int,double> cache, Action<int> on_intersection)
//		{
//            var queue_list = new List<IRankSelect>(64);
//            for (int i = 0; i < max; ++i) {
//                //foreach (var I in this.LC_LIST) {
//                var lc = this.LC_LIST[i];
//				lc.PartialSearchRange (q, radius, R, cache, queue_list);
//			}
//            // var C = this.UI_ALG.ComputeUI (M);
//            byte[] A = new byte[ this.DB.Count ];
//            // var max = this.LC_LIST.Count;
//            foreach (var rs in queue_list) {
//                var count1 = rs.Count1;
//                for (int i = 1; i <= count1; ++i) {
//                    var item = rs.Select1 (i);
//                    A [item]++;
//                    if (A [item] == max) {
//                        on_intersection(item);
////                        var dist = this.DB.Dist (q, this.DB [item]);
////                        if (dist <= radius) {
////                            R.Push (item, dist);
////                        }
//                    }
//                }
//			}
//			return R;
//		}
//
//        protected virtual IResult PartialSearchKNN (object q, int K, IResult R, int max, Dictionary<int,double> cache, Action<int> on_intersection)
//        {
//            var queue_dist = new List<double>();
//            var queue_list = new List<IRankSelect>();
//            for (int i = 0; i < max; ++i) {
//                var lc = this.LC_LIST[i];
//                lc.PartialSearchKNN (q, K, R, cache, queue_dist, queue_list);
//            }
//            byte[] A = new byte[ this.DB.Count ];
//            // int max = this.LC_LIST.Count;
//            Sorting.Sort<double, IRankSelect>(queue_dist, queue_list);
//            for (int x = 0; x < queue_dist.Count; ++x) {
//                var rs = queue_list[x];
//                var dcq_cov = queue_dist[x];
//                var count1 = rs.Count1;
//                /*if (dcq_cov > R.CoveringRadius) {
//                    break;
//                }*/
//                for (int i = 1; i <= count1; ++i) {
//                    var item = rs.Select1 (i);
//                    A [item]++;
//                    if (A [item] == max) {
//                        on_intersection(item);
////                        var dist = this.DB.Dist (q, this.DB [item]);
////                        R.Push (item, dist);
//                    }
//                }
//            }
//            return R;           
//        }
//
//        public virtual object CreateQueryContext (object q)
//        {
//            var ctx = new ArrayList ();
//            foreach (var lc in this.LC_LIST) {
//                ctx.Add (lc.CreateQueryContext(q));
//            }
//            return ctx;
//        }
//       
//        public virtual bool MustReviewItem (object q, int item, double radius, object ctx)
//        {
//            return this.MustReviewItem(0, q, item, radius, ctx);
//        }
//
//        public virtual bool MustReviewItem (int start_index_lambda, object q, int item, double radius, object ctx)
//        {
//            var ctx_list = (ctx as ArrayList);
//            for (int i = start_index_lambda; i < this.LC_LIST.Count; ++i) {
//                var lc = this.LC_LIST[i];
//                var ctx_lc = ctx_list[i];
//                var review = lc.MustReviewItem(q, item, radius, ctx_lc);
//                if (!review) {
//                    return false;
//                }
//            }
//            return true;
//        }
//
//	}
//}
