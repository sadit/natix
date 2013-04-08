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
        #region STRUCTS
        public struct Pivot : ILoadSave
        {
			public int objID;
            public double stddev;
            public double mean;
            public double last_near;
            public double first_far;
			public int num_near;
			public int num_far;

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
        }

        #endregion

		public Pivot[] _Pivs;
        public ItemPair[] _Items;

		public PivotGroup ()
		{
		}

		public void Load(BinaryReader Input)
		{
			int len;
			len = Input.ReadInt32 ();
			this._Pivs = CompositeIO<Pivot>.LoadVector (Input, len, null) as Pivot[];
			len = Input.ReadInt32 ();
            this._Items = CompositeIO<ItemPair>.LoadVector(Input, len, null) as ItemPair[];
		}

		public void Save (BinaryWriter Output)
        {
			Output.Write (this._Pivs.Length);
			CompositeIO<Pivot>.SaveVector (Output, this._Pivs);
            Output.Write (this._Items.Length);
            CompositeIO<ItemPair>.SaveVector (Output, this._Items);
		}

        protected virtual void SearchExtremes (DynamicSequential idx, List<ItemPair> items, object piv, double alpha_stddev, int min_bs, out IResult near, out IResult far, out DynamicSequential.Stats stats)
        {
            throw new NotSupportedException();
        }

        public virtual void Build (MetricDB DB, double alpha_stddev, int min_bs, int seed)
        {
            var idxDynamic = new DynamicSequentialOrdered ();
            idxDynamic.Build (DB, RandomSets.GetRandomPermutation(DB.Count, new Random(seed)));
            this._Items = new ItemPair[DB.Count];
			var pivs = new List<Pivot> (1024);
			var items = new List<ItemPair> (DB.Count);
            int I = 0;
            var extreme_items = new List<ItemPair>(idxDynamic.Count);
            while (idxDynamic.Count > 0) {
                var pidx = idxDynamic.GetAnyItem();
                object piv = DB[pidx];
                idxDynamic.Remove(pidx);
                this._Items[pidx] = new ItemPair(pidx, 0);
                IResult near, far;
                DynamicSequential.Stats stats;
                this.SearchExtremes(idxDynamic, extreme_items, piv, alpha_stddev, min_bs, out near, out far, out stats);
                foreach (var pair in near) {
					items.Add( new ItemPair (pair.docid, pair.dist) );
                }
				foreach (var pair in far) {
					items.Add( new ItemPair (pair.docid, pair.dist) );
				}
				var piv_data = new Pivot(pidx, stats.mean, stats.stddev, 0, double.MaxValue, near.Count, far.Count);
                if (near.Count > 0) piv_data.last_near = near.Last.dist;
                if (far.Count > 0) piv_data.first_far = far.First.dist;
                pivs.Add(piv_data);
                if (I % 10 == 0) {
                    Console.WriteLine ("");
                    Console.WriteLine (this.ToString());
                    Console.WriteLine("-- I {0}> remains: {1}, alpha_stddev: {2}, mean: {3}, stddev: {4}, pivot: {5}",
                                      I, idxDynamic.Count, alpha_stddev, stats.mean, stats.stddev, pidx);
                    double near_first, near_last, far_first, far_last;
                    if (near.Count > 0) {
                        near_first = near.First.dist;
                        near_last = near.Last.dist;
                        Console.WriteLine("-- (ABSVAL)  first-near: {0}, last-near: {1}, near-count: {2}",
                                          near_first, near_last, near.Count);
                        Console.WriteLine("-- (NORMVAL) first-near: {0}, last-near: {1}",
                                          near_first / stats.max, near_last / stats.max);
                        Console.WriteLine("-- (SIGMAS)  first-near: {0}, last-near: {1}",
                                          near_first / stats.stddev, near_last / stats.stddev);
                        
                    }
                    if (far.Count > 0) {
                        far_first = far.First.dist;
                        far_last = far.Last.dist;
                        Console.WriteLine("++ (ABSVAL)  first-far: {0}, last-far: {1}, far-count: {2}",
                                          far_first, far_last, far.Count);
                        Console.WriteLine("++ (NORMVAL) first-far: {0}, last-far: {1}",
                                          far_first / stats.max, far_last / stats.max);
                        Console.WriteLine("++ (SIGMAS)  first-far: {0}, last-far: {1}",
                                          far_first / stats.stddev, far_last / stats.stddev);
                    }
                }
                ++I;
                idxDynamic.Remove(near);
                idxDynamic.Remove(far);
                //Console.WriteLine("Number of objects after: {0}",idxDynamic.DOCS.Count);
            }
            Console.WriteLine("Number of pivots per group: {0}", I);
			this._Pivs = pivs.ToArray ();
			this._Items = items.ToArray ();
        }
	}
}

