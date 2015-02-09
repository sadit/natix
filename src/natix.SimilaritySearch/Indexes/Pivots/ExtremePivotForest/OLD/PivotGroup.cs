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
        public class Pivot : ILoadSave
        {
			public int objID;
            public double stddev;
            public double mean;
            public double last_near;
            public double first_far;
			public int num_near;
			public int num_far;

			public Pivot()
			{
			}

			public Pivot(int objID, double stddev, double mean, double cov_near, double cov_far, int num_near, int num_far)
            {
				this.objID = objID;
                this.stddev = stddev;
                this.mean = mean;
                this.last_near = cov_near;
                this.first_far = cov_far;
				this.num_near=num_near;
				this.num_far=num_far;
            }

            public void Load(BinaryReader Input)
            {
				this.objID = Input.ReadInt32 ();
                this.stddev = Input.ReadDouble();
                this.mean = Input.ReadDouble();
                this.last_near = Input.ReadDouble();
                this.first_far = Input.ReadDouble();
				this.num_near= Input.ReadInt32();
				this.num_far=Input.ReadInt32();
            }
            
            public void Save (BinaryWriter Output)
            {
				Output.Write (this.objID);
                Output.Write (this.stddev);
                Output.Write (this.mean);
                Output.Write (this.last_near);
                Output.Write (this.first_far);
				Output.Write (this.num_near);
				Output.Write (this.num_far);
            }

			public override string ToString ()
			{
				return string.Format ("[Pivot objID: {0}, stddev: {1}, mean: {2}, last_near: {3}, first_far: {4}, num_near: {5}, num_far: {6}]",
				                      this.objID, this.stddev, this.mean, this.last_near, this.first_far, this.num_near, this.num_far);
			}
        }


		public Pivot[] Pivs;
        public ItemPair[] Items;

		public PivotGroup ()
		{
		}

		public void Load(BinaryReader Input)
		{
			int len;
			len = Input.ReadInt32 ();
			this.Pivs = CompositeIO<Pivot>.LoadVector (Input, len, null) as Pivot[];
			len = Input.ReadInt32 ();
            this.Items = CompositeIO<ItemPair>.LoadVector(Input, len, null) as ItemPair[];
		}

		public void Save (BinaryWriter Output)
        {
			Output.Write (this.Pivs.Length);
			CompositeIO<Pivot>.SaveVector (Output, this.Pivs);
            Output.Write (this.Items.Length);
            CompositeIO<ItemPair>.SaveVector (Output, this.Items);
		}

        protected virtual void SearchExtremes (DynamicSequential idx, List<ItemPair> items, object piv, double alpha_stddev, int min_bs, out IResult near, out IResult far, out DynamicSequential.Stats stats)
        {
            throw new NotSupportedException();
        }

        public virtual void Build (MetricDB DB, double alpha, int min_bs, int seed, bool do_far)
        {
            var idxDynamic = new DynamicSequentialOrdered ();
            idxDynamic.Build (DB, RandomSets.GetRandomPermutation(DB.Count, new Random(seed)));
            // this.Items = new ItemPair[DB.Count];
			var pivs = new List<Pivot> (32);
			var items = new List<ItemPair> (DB.Count);
            int I = 0;
            var extreme_items = new List<ItemPair>(idxDynamic.Count);
            while (idxDynamic.Count > 0) {
                var pidx = idxDynamic.GetAnyItem();
                object piv = DB[pidx];
                idxDynamic.Remove(pidx);
                // this.Items[pidx] = new ItemPair(pidx, 0);
				DynamicSequential.Stats stats;
				Pivot piv_data;
				double near_first = double.MaxValue;
				double near_last = 0;
				double far_first = double.MaxValue;
				int num_near = 0;
				int num_far = 0;
				{
					IResult near, far;
					this.SearchExtremes(idxDynamic, extreme_items, piv, alpha, min_bs, out near, out far, out stats);
					foreach (var pair in near) {
						near_first = Math.Min (near_first, pair.Dist);
						near_last = Math.Max (near_last, pair.Dist);
						items.Add( new ItemPair { ObjID = pair.ObjID, Dist = pair.Dist} );
					}
					num_near = near.Count;
					idxDynamic.Remove(near);
					if (do_far) {
						foreach (var pair in far) {
							far_first = Math.Min (far_first, pair.Dist);
							items.Add( new ItemPair {ObjID = pair.ObjID, Dist = pair.Dist} );
						}
						num_far = far.Count;
						idxDynamic.Remove(far);
					}
					piv_data = new Pivot(pidx, stats.mean, stats.stddev, near_last, far_first, num_near, num_far);
					pivs.Add(piv_data);
				}
                if (I % 10 == 0) {
                    Console.WriteLine ("");
                    Console.WriteLine (this.ToString());
					Console.WriteLine("-- I {0}> remains: {1}, alpha: {2}, mean: {3}, stddev: {4}, pivot: {5}, min_bs: {6}, db: {7}, do_far: {8}",
                                      I, idxDynamic.Count, alpha, stats.mean, stats.stddev, pidx, min_bs, DB.Name, do_far);
                    if (piv_data.num_near > 0) {
						Console.WriteLine("-- (NORMVAL) first-near: {0}, last-near: {1}, near-count: {2}",
                                          near_first / stats.max, piv_data.last_near / stats.max, piv_data.num_near);
                        
                    }
                    if (piv_data.num_far > 0) {
						Console.WriteLine("++ (NORMVAL) first-far: {0}, far-count: {1}",
                                          piv_data.first_far / stats.max, piv_data.num_far);
                    }
                }
                ++I;

                //Console.WriteLine("Number of objects after: {0}",idxDynamic.DOCS.Count);
            }
            Console.WriteLine("Number of pivots per group: {0}", I);
			this.Pivs = pivs.ToArray ();
			this.Items = items.ToArray ();
        }

		public virtual int SearchKNN (MetricDB db, object q, int K, IResult res, short[] A, short current_rank_A)
		{
			int abs_pos = 0;
			int count_dist = 0;
			foreach (var piv in this.Pivs) {
				var pivOBJ = db [piv.objID];
				var dqp = db.Dist (q, pivOBJ);
				res.Push (piv.objID, dqp);
				++count_dist;
				// checking near ball radius
				if (dqp <= piv.last_near + res.CoveringRadius) {
					for (int j = 0; j < piv.num_near; ++j, ++abs_pos) {
						var item = this.Items [abs_pos];
						// checking covering pivot
						if (A[item.ObjID] == current_rank_A && Math.Abs (item.Dist - dqp) <= res.CoveringRadius) {
							++A [item.ObjID];
						}
					}
				} else {
					abs_pos += piv.num_near;
				}
				// checking external radius
				if (dqp + res.CoveringRadius >= piv.first_far) {
					for (int j = 0; j < piv.num_far; ++j, ++abs_pos) {
						var item = this.Items [abs_pos];
						// checking covering pivot
						if (A[item.ObjID] == current_rank_A && Math.Abs (item.Dist - dqp) <= res.CoveringRadius) {
							++A [item.ObjID];
						}
					}
				} else {
					abs_pos += piv.num_far;
				}
				if (dqp + res.CoveringRadius <= piv.last_near || piv.first_far <= dqp - res.CoveringRadius) {
					break;
				}
			}
			return count_dist;
		}
	}
}

