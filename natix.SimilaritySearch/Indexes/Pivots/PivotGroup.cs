//
//  Copyright 2012  Francisco Santoyo
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
// Eric S. Tellez
// - Load and Save methods
// - Everything was modified to compute slices using radius instead of the percentiles
// - Argument minimum bucket size

using System;
using System.IO;
using natix.CompactDS;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class PivotGroup : ILoadSave
	{
        public struct Pivot : ILoadSave
        {
            public float stddev;
            public float mean;
            public float cov_near;
            public float cov_far;

            public Pivot(float stddev, float mean, float cov_near, float cov_far)
            {
                this.stddev = stddev;
                this.mean = mean;
                this.cov_near = cov_near;
                this.cov_far = cov_far;
            }

            public void Load(BinaryReader Input)
            {
                this.stddev = Input.ReadSingle();
                this.mean = Input.ReadSingle();
                this.cov_near = Input.ReadSingle();
                this.cov_far = Input.ReadSingle();
            }
            
            public void Save (BinaryWriter Output)
            {
                Output.Write (this.stddev);
                Output.Write (this.mean);
                Output.Write (this.cov_near);
                Output.Write (this.cov_far);
            }
        }

        public struct Item : ILoadSave
        {
            public int pivID;
            public float dist;

            public Item (int pivID, float dist)
            {
                this.pivID = pivID;
                this.dist = dist;
            }

            public void Load(BinaryReader Input)
            {
                this.pivID = Input.ReadInt32 ();
                this.dist = Input.ReadSingle();
            }

            public void Save (BinaryWriter Output)
            {
                Output.Write (this.pivID);
                Output.Write (this.dist);
            }
        }

        public Item[] Items;
        public Dictionary<int,Pivot> Pivs;

		public PivotGroup ()
		{
		}

		public void Load(BinaryReader Input)
		{
            var len = Input.ReadInt32 ();
            this.Items = CompositeIO<Item>.LoadVector(Input, len, null) as Item[];
            len = Input.ReadInt32 ();
            this.Pivs = new Dictionary<int, Pivot>(len);
            for (int i = 0; i < len; ++i) {
                var u = default(Pivot);
                u.Load (Input);
                var pivID = Input.ReadInt32 ();
                this.Pivs.Add(pivID, u);
            }
		}

		public void Save (BinaryWriter Output)
        {
            Output.Write (this.Items.Length);
            CompositeIO<Item>.SaveVector (Output, this.Items);
            Output.Write (this.Pivs.Count);
            foreach (var p in this.Pivs) {
                Output.Write(p.Key);
                p.Value.Save(Output);
            }
		}

		public void Build (MetricDB DB, double alpha_stddev, int min_bs, int seed)
		{
			DynamicSequential idxDynamic;
			idxDynamic = new DynamicSequential (seed);
			idxDynamic.Build (DB);
            this.Items = new Item[DB.Count];
            this.Pivs = new Dictionary<int, Pivot>();
			int I = 0;

			while(idxDynamic.DOCS.Count > 0){
				var pidx = idxDynamic.GetRandom();
				object piv = DB[pidx];
				idxDynamic.Remove(pidx);
                this.Items[pidx] = new Item(pidx, 0);
				double mean, stddev;
				IResult near, far;
				idxDynamic.SearchExtremesRange(piv, alpha_stddev, min_bs, out near, out far, out mean, out stddev);
				foreach (var pair in near) {
                    this.Items[pair.docid] = new Item (pidx, (float) pair.dist);
				}
				foreach (var pair in far) {
                    this.Items[pair.docid] = new Item (pidx, (float)-pair.dist);
				}
                var piv_data = new Pivot((float)mean, (float)stddev, 0, float.MaxValue);
                if (near.Count > 0) piv_data.cov_near = (float)near.Last.dist;
                if (far.Count > 0) piv_data.cov_far = (float)far.Last.dist;
                this.Pivs.Add (pidx, piv_data);
				if (I % 10 == 0) {
					Console.WriteLine("-- I {0}> remains: {1}, alpha_stddev: {2}, mean: {3}, stddev: {4}, pivot: {5}",
					                  I, idxDynamic.DOCS.Count, alpha_stddev, mean, stddev, pidx);
					double near_first, near_last, far_first, far_last;
					if (near.Count == 0) {
						near_first = near_last = -1;
					} else {
						near_first = near.First.dist;
						near_last = near.Last.dist;
					}
					if (far.Count == 0) {
						far_last = far_first = -1;
					} else {
						far_first = -far.Last.dist;
						far_last = -far.First.dist;
					}
					Console.WriteLine("--    first-near: {0}, last-near: {1}, first-far: {2}, last-far: {3}, near-count: {4}, far-count: {5}",
					                  near_first, near_last, far_first, far_last, near.Count, far.Count);
					Console.WriteLine("      normalized first-near: {0}, last-near: {1}, first-far: {2}, last-far: {3}, mean: {4}, stddev: {5}",
					                  near_first/far_last, near_last/far_last, far_first/far_last, far_last/far_last, mean/far_last, stddev/far_last);
                    Console.WriteLine("      mean_to_last_near: {0} sigmas, mean_to_first_far: {1} sigmas, first: {2} sigmas", (mean - near_last)/stddev, -(mean-far_first)/stddev, near_first / stddev);
					//}
				}
				++I;
				idxDynamic.Remove(near);
				idxDynamic.Remove(far);
				//Console.WriteLine("Number of objects after: {0}",idxDynamic.DOCS.Count);
			}
			Console.WriteLine("Number of pivots per group: {0}", I);
		}
	}
}

