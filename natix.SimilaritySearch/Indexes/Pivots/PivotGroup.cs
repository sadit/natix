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
            public double stddev;
            public double mean;
            public double last_near;
            public double first_far;

            public Pivot(double stddev, double mean, double cov_near, double cov_far)
            {
                this.stddev = stddev;
                this.mean = mean;
                this.last_near = cov_near;
                this.first_far = cov_far;
            }

            public void Load(BinaryReader Input)
            {
                this.stddev = Input.ReadDouble();
                this.mean = Input.ReadDouble();
                this.last_near = Input.ReadDouble();
                this.first_far = Input.ReadDouble();
            }
            
            public void Save (BinaryWriter Output)
            {
                Output.Write (this.stddev);
                Output.Write (this.mean);
                Output.Write (this.last_near);
                Output.Write (this.first_far);
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
        #endregion

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
                var pivID = Input.ReadInt32 ();
                var u = default(Pivot);
                u.Load (Input);
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

        protected virtual void SearchExtremes (DynamicSequential idx, List<DynamicSequential.Item> items, object piv, double alpha_stddev, int min_bs, out IResult near, out IResult far, out DynamicSequential.Stats stats)
        {
            throw new NotSupportedException();
        }

        public virtual void Build (MetricDB DB, double alpha_stddev, int min_bs, int seed)
        {
            var idxDynamic = new DynamicSequentialRandom (seed);
            idxDynamic.Build (DB);
            this.Items = new Item[DB.Count];
            this.Pivs = new Dictionary<int, Pivot>();
            int I = 0;
            var items = new List<DynamicSequential.Item>(idxDynamic.Count);
            while(idxDynamic.DOCS.Count > 0){
                var pidx = idxDynamic.GetAnyItem();
                object piv = DB[pidx];
                idxDynamic.Remove(pidx);
                this.Items[pidx] = new Item(pidx, 0);
                IResult near, far;
                DynamicSequential.Stats stats;
                this.SearchExtremes(idxDynamic, items, piv, alpha_stddev, min_bs, out near, out far, out stats);
                foreach (var pair in near) {
                    this.Items[pair.docid] = new Item (pidx, (float) pair.dist);
                }
                foreach (var pair in far) {
                    this.Items[pair.docid] = new Item (pidx, (float) pair.dist);
                }
                var piv_data = new Pivot(stats.mean, stats.stddev, 0, float.MaxValue);
                if (near.Count > 0) piv_data.last_near = (float)near.Last.dist;
                if (far.Count > 0) piv_data.first_far = (float)far.First.dist;
                this.Pivs.Add (pidx, piv_data);
                if (I % 10 == 0) {
                    Console.WriteLine ("");
                    Console.WriteLine (this.ToString());
                    Console.WriteLine("-- I {0}> remains: {1}, alpha_stddev: {2}, mean: {3}, stddev: {4}, pivot: {5}",
                                      I, idxDynamic.DOCS.Count, alpha_stddev, stats.mean, stats.stddev, pidx);
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
        }
	}
}

