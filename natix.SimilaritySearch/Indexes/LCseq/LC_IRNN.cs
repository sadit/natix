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
////   Original filename: natix/SimilaritySearch/Indexes/LC_IRNN.cs
//// 
//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using NDesk.Options;
//using natix.CompactDS;
//using natix.SortingSearching;
//
//namespace natix.SimilaritySearch
//{
//	/// <summary>
//	/// LC with a fixed number of centers
//	/// </summary>
//	/// <exception cref='ArgumentNullException'>
//	/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
//	/// </exception>
//	public class LC_IRNN : LC_RNN
//	{
//		Index build_index = null;
//			
//		public LC_IRNN () : base()
//		{	
//		}
//
//        // static int MIN_NUM_CENTERS = 1000;		
//		/// <summary>
//		/// Build the index 
//		/// </summary>
//        public override void Build (MetricDB db, int num_centers, Random rand, SequenceBuilder seq_builder = null)
//        {
//            this.Build(db, num_centers, rand, null, seq_builder);
//        }
//
//        public void Build (MetricDB db, int num_centers, Random rand, Func<MetricDB,int,Index> create_internal_index, SequenceBuilder seq_builder = null)
//        {
//            this.DB = db;
//            this.CENTERS = RandomSets.GetRandomSubSet (num_centers, this.DB.Count, rand);
//            this.COV = new float[num_centers];
//            Sorting.Sort<int> (this.CENTERS);
//            MetricDB sample = new SampleSpace (db.Name + ".sample-centers", db, this.CENTERS);
//            if (create_internal_index == null) {
//                var lcbuild = new LC ();
//                double ratio = num_centers * 1.0 / this.DB.Count;
//                ratio = Math.Max (ratio, 0.01);
//                var child_num_centers = (int)Math.Ceiling (num_centers * ratio);
//                Console.WriteLine ("XXXXXX requested num_centers: {0}, actual num_centers: {1}", num_centers, this.CENTERS.Count);
//                lcbuild.Build (sample, child_num_centers, rand);
//                this.build_index = lcbuild;
//            } else {
//                Console.WriteLine ("XXXXXX construction with a custom index: requested num_centers: {0}, actual num_centers: {1}", num_centers, this.CENTERS.Count);
//                this.build_index = create_internal_index(sample, num_centers);
//            }
//			BitStream32 IsCenter = new BitStream32 ();
//			IsCenter.Write (false, db.Count);
//			var seq = new int[db.Count];
//			for (int i = 0; i < num_centers; i++) {
//				IsCenter [this.CENTERS [i]] = true;
//				seq[this.CENTERS[i]] = this.CENTERS.Count;
//			}
//			this.BuildInternal (IsCenter, seq, seq_builder);
//			this.build_index = null;
//		}
//		
//		/// <summary>
//		/// SearchNN, only used at preprocessing time
//		/// </summary>
//		public override void BuildSearchNN (object u, out int nn_center, out double nn_dist)
//		{
//			var R = this.build_index.SearchKNN (u, 1);
//			var p = R.First;
//			nn_center = p.docid;
//			nn_dist = p.dist;
//		}
//		
//	}
//}
