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
		public IList<int> PIVLIST;
		public int[] PIVS;
		public float[] DIST;

		public PivotGroup ()
		{
		}

		public void Load(BinaryReader Input)
		{
			this.PIVLIST = ListIGenericIO.Load (Input);
			var len = Input.ReadInt32 ();
			this.PIVS = (int[])PrimitiveIO<int>.ReadFromFile(Input, len, null);
			this.DIST = (float[])PrimitiveIO<float>.ReadFromFile(Input, len, null);
		}

		public void Save(BinaryWriter Output)
		{
			ListIGenericIO.Save(Output, this.PIVLIST);
			Output.Write(this.PIVS.Length);
			PrimitiveIO<int>.WriteVector(Output, this.PIVS);
			PrimitiveIO<float>.WriteVector(Output, this.DIST);
		}

		public void Build (MetricDB DB, double alpha_stddev, int min_bs, int seed)
		{
			DynamicSequential idxDynamic;
			idxDynamic = new DynamicSequential (seed);
			idxDynamic.Build (DB);
			this.PIVLIST = new List<int>();
			this.PIVS = new int[DB.Count];
			this.DIST = new float[DB.Count];
			// PivotGroup g = new PivotGroup(DB.Count);
			//Console.WriteLine ("Number of objects: {0}",idxDynamic.DOCS.Count);
			int I = 0;
			while(idxDynamic.DOCS.Count > 0){
				var pidx = idxDynamic.GetRandom();
				object piv = DB[pidx];
				idxDynamic.Remove(pidx);
				this.PIVLIST.Add(pidx);
				this.DIST[pidx] = 0;
				this.PIVS[pidx] = pidx;
				double mean, stddev;
				IResult near, far;
				idxDynamic.SearchExtremesRange(piv, alpha_stddev, min_bs, out near, out far, out mean, out stddev);
				foreach (var pair in near){
					this.PIVS[pair.docid] = pidx; 
					this.DIST[pair.docid] = (float)pair.dist;
				}
				foreach (var pair in far){
					this.PIVS[pair.docid] = pidx;
					this.DIST[pair.docid] = (float)-pair.dist;
				}
				if (I % 10 == 0) {
					Console.WriteLine("--- I {0}> remains: {1}, alpha_stddev: {2}, mean: {3}, stddev: {4}, pivot: {5}",
					                  I, idxDynamic.DOCS.Count, alpha_stddev, mean, stddev, pidx);
					double near_first, near_last, far_first, far_last;
					if (near.Count == 0) {
						near_first = -1;
						near_last = -1;
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
					Console.WriteLine("--- +++ first-near: {0}, last-near: {1}, first-far: {2}, last-far: {3}, near-count: {4}, far-count: {5}",
					                  near_first, near_last, far_first, far_last, near.Count, far.Count);
					Console.WriteLine("--- +++ normalized first-near: {0}, last-near: {1}, first-far: {2}, last-far: {3}, mean: {4}, stddev: {5}",
					                  near_first/far_last, near_last/far_last, far_first/far_last, far_last/far_last, mean/far_last, stddev/far_last);
					//}
				}
				++I;
				idxDynamic.Remove(near);
				idxDynamic.Remove(far);
				//Console.WriteLine("Number of objects after: {0}",idxDynamic.DOCS.Count);
			}
			Console.WriteLine("Number of pivots per group: {0}", this.PIVLIST.Count);
		}
	}
}

