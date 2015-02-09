//
//  Copyright 2012  Eric Sadit Tellez Avila
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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public abstract class PivotsAbstract : BasicIndex, IndexSingle
	{
		public List<double>[] DIST;
		public MetricDB PIVS;
		
		public PivotsAbstract ()
		{
		}

		public void Build (MetricDB db, MetricDB pivs, int NUMBER_TASKS=-1)
		{
			this.DB = db;
			this.PIVS = pivs;
			int num_pivs = pivs.Count;
			this.DIST = new List<double>[num_pivs];
			Action<int> one_pivot = delegate (int pivID) {
				var n = this.DB.Count;
				var L = new List<double>();
				this.DIST[pivID] = L;
				for (int docID = 0; docID < n; ++docID) {
					var d = this.DB.Dist (this.PIVS[pivID], this.DB [docID]);
					L.Add (d);
				}
			};

//			ParallelOptions ops = new ParallelOptions ();
//			ops.MaxDegreeOfParallelism = NUMBER_TASKS;
			LongParallel.For (0, num_pivs, one_pivot, NUMBER_TASKS);
		}
		
		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.PIVS = SpaceGenericIO.SmartLoad(Input, false);
			this.DIST = new List<double>[this.PIVS.Count];
			for (int i = 0; i < this.PIVS.Count; ++i) {
				this.DIST[i] = new List<double>(this.DB.Count);
				PrimitiveIO<double>.LoadVector(Input, this.DB.Count, this.DIST[i]);
			}
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.PIVS);
			for (int i = 0; i < this.PIVS.Count; ++i) {
				PrimitiveIO<double>.SaveVector(Output, this.DIST[i]);
			}
		}
		
		public override IResult SearchKNN (object q, int K, IResult res)
		{		
			var m = this.PIVS.Count;
			var n = this.DB.Count;
			// var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
			var dqp_vec = new double[ m ];
			for (int pivID = 0; pivID < m; ++pivID) {
				dqp_vec[pivID] = this.DB.Dist(q, this.PIVS[pivID]);
			}
			this.internal_numdists += m;
			// todo: randomize
			for (int docID = 0; docID < n; ++docID) {
				bool check_object = true;
				for (int pivID = 0; pivID < m; ++pivID) {
					double dqp = dqp_vec[pivID];
					var dpu = this.DIST[pivID][docID];
					if (Math.Abs (dqp - dpu) > res.CoveringRadius) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					res.Push(docID, this.DB.Dist(q, this.DB[docID]));
				}
			}
			return res;
		}

		
		public object CreateQueryContext (object q)
		{
			var m = this.PIVS.Count;
			var L = new double[ m ];
			for (int pivID = 0; pivID < m; ++pivID) {
				++this.internal_numdists;
				L[pivID] = this.DB.Dist(q, this.PIVS[pivID]);
			}
			return L;
		}
		
		public bool MustReviewItem (object q, int item, double radius, object ctx)
		{
			var pivs = ctx as Double[];
			var m = this.PIVS.Count;
			for (int pivID = 0; pivID < m; ++pivID) {
				var P = this.DIST[pivID];
				if (Math.Abs (P[item] - pivs[pivID]) > radius) {
					return false;
				}
			}
			return true;
		}
		
		public override IResult SearchRange (object q, double radius)
		{
			var res = new Result(this.DB.Count);
			var n = this.DB.Count;
			var m = this.PIVS.Count;
			var dqp_vec = new double[ m ];
			for (int pivID = 0; pivID < m; ++pivID) {
				dqp_vec[pivID] = this.DB.Dist(q, this.PIVS[pivID]);
			}
			this.internal_numdists += m;
			for (int docID = 0; docID < n; ++docID) {
				bool check_object = true;
				for (int pivID = 0; pivID < m; ++pivID) {
					double dqp = dqp_vec[pivID];
					var dpu = this.DIST[pivID][docID];
					if (Math.Abs (dqp - dpu) > radius) {
						check_object = false;
						break;
					}
				}
				if (check_object) {
					var d = this.DB.Dist(this.DB[docID], q);
					if (d <= radius) {
						res.Push(docID, d);
					}
				}
			}
			return res;
		}		
	}
}

