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
//using natix.CompactDS;
//using natix.SortingSearching;
//
//namespace natix.SimilaritySearch
//{
//    public class LC_KVorRNN : LC_RNN
//    {
//		public int KNearCenters = 7;
//		public float[][] CCDIST;
//        public LC_KVorRNN () : base()
//        {            
//        }
//
//		protected void FillCCD()
//		{
//			var m = this.CENTERS.Count;
//			this.CCDIST = new float[m][];
//			for (int centerID = 0; centerID < m; ++centerID) {
//				if (centerID % 100 == 0) {
//					Console.WriteLine("=== computing all vs all center's distances {0}/{1}", centerID+1, m);
//				}
//				var curr = new float[centerID];
//				this.CCDIST[centerID] = curr;
//				var objA = this.DB[this.CENTERS[centerID]];
//				for (int i = 0; i < centerID; ++i) {
//					var objB = this.DB[this.CENTERS[i]];
//					curr[i] = (float)(this.DB.Dist(objA, objB) / 2);
//				}
//			}
//		}
//
//		public override void Save (BinaryWriter Output)
//		{
//			base.Save (Output);
//			var m = this.CENTERS.Count;
//			for (int centerID = 0; centerID < m; ++centerID) {
//				PrimitiveIO<float>.WriteVector(Output, this.CCDIST[centerID]);
//			}
//		}
//
//		public override void Load (BinaryReader Input)
//		{
//			base.Load (Input);
//			var m = this.CENTERS.Count;
//			this.CCDIST = new float[m][];
//			for (int centerID = 0; centerID < m; ++centerID) {
//				var curr = new float[centerID];
//				this.CCDIST[centerID] = curr;
//				PrimitiveIO<float>.ReadFromFile(Input, centerID, curr);
//			}
//		}
//
//		float GetDistanceCC_2(int a, int b) 
//		{
//			if (a == b) {
//				return 0;
//			}
//			if (a < b) {
//				return this.CCDIST[b][a];
//			} else {
//				return this.CCDIST[a][b];
//			}
//		}
//
//		public override void Build (LC_RNN lc, SequenceBuilder seq_builder)
//		{
//			base.Build (lc, seq_builder);
//			this.FillCCD ();
//		}
//
//		public override void Build (MetricDB db, int num_centers, Random rand, SequenceBuilder seq_builder = null)
//		{
//			base.Build (db, num_centers, rand, seq_builder);
//			this.FillCCD ();
//		}
//
//        /// <summary>
//        /// Search the specified q with radius qrad.
//        /// </summary>
//        public override IResult SearchRange (object q, double qrad)
//        {
//			var res = new ResultRange (qrad, this.DB.Count);
//			this.SearchKNN (q, this.DB.Count, res);
//			return res;
//        }
//        
//        /// <summary>
//        /// KNN search.
//        /// </summary>
//        public override IResult SearchKNN (object q, int K, IResult R)
//        {
//            var sp = this.DB;
//            int num_centers = this.CENTERS.Count;
//            var near_centers = this.DB.CreateResult (this.KNearCenters, false);
//			var D = new double[num_centers];
//            for (int centerID = 0; centerID < num_centers; centerID++) {
//                var dcq = sp.Dist (this.DB [this.CENTERS [centerID]], q);
//				D[centerID] = dcq;
//                R.Push (this.CENTERS [centerID], dcq);
//                if (dcq <= R.CoveringRadius + this.COV [centerID]) {
//					near_centers.Push(centerID, dcq);
//                }
//            }
//			this.internal_numdists += num_centers;
//			for (int centerID = 0; centerID < num_centers; ++centerID) {
//				bool review = true;
//				var dcq = D[centerID];
//				foreach (var closer in near_centers) {
//					if (dcq < closer.dist) {
//						break;
//					}
//					//if (dcq> closer.dist + 2 * R.CoveringRadius) {
//					var ccdist = this.GetDistanceCC_2(closer.docid, centerID);
//					//if (closer.dist + R.CoveringRadius < ccdist || ccdist < dcq - R.CoveringRadius ) {
//					if (closer.dist + R.CoveringRadius < dcq - R.CoveringRadius ) {
//					//if (closer.dist + R.CoveringRadius * 2 < dcq ) {
//						review = false;
//						break;
//					}
//					// break;
//                }
//				if (review && dcq <= R.CoveringRadius + this.COV [centerID]) {
//					var rs = this.SEQ.Unravel (centerID);
//					var count1 = rs.Count1;
//					for (int i = 1; i <= count1; i++) {
//						var u = rs.Select1 (i);
//						var r = sp.Dist (q, sp [u]);
//						//if (r <= qr) { // already handled by R.Push
//						R.Push (u, r);
//					}
//				}
//            }
//            return R;
//        }        
//    }
//}
//
