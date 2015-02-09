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
// 
// Francisco Santoyo 
// - Adaptation of the KNN searching algorithm of LAESA
// Eric Sadit
// - Parallel building
// - SearchRange SearchKNN optimizations

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class EPTable : BasicIndex, IndexSingle
	{
		public EPList[] rows;

		public EPTable ()
		{
		}

		
		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var num_groups = Input.ReadInt32 ();
			this.InitRows (num_groups);
			//CompositeIO<PivotGroup>.LoadVector (Input, num_groups, this.GROUPS);
			foreach (var g in this.rows) {
				g.Load(Input);
			}
		}
		
		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			Output.Write ((int)this.rows.Length);
			// CompositeIO<PivotGroup>.SaveVector (Output, this.GROUPS);
			foreach (var g in this.rows) {
				g.Save(Output);
			}
		}

		protected virtual void InitRows (int len)
		{
			this.rows = new EPList[len];
			for (int i = 0; i < len; ++i) {
				this.rows[i] = new EPList();
			}
		}

		public void Build (EPTable pgi, int num_groups)
		{
			this.DB = pgi.DB;
			if (num_groups <= 0) {
				num_groups = pgi.rows.Length;
			}
			this.InitRows (num_groups);
			for (int i = 0; i < num_groups; ++i) {
				this.rows[i] = pgi.rows[i];
			}
		}


		public virtual void Build (MetricDB db, int num_groups, Func<MetricDB,Random,EPList> new_eplist = null, int num_build_processors = -1)
		{
			if (new_eplist== null) {
				new_eplist = (MetricDB _db, Random rand) => new EPListRandomPivots(_db, 1000, rand);
			}
			// num_build_processors = 1;
			this.DB = db;
			this.InitRows (num_groups);
			var seed = RandomSets.GetRandomInt ();
			ParallelOptions ops = new ParallelOptions ();
			ops.MaxDegreeOfParallelism = num_build_processors;
			// Parallel.For (0, num_groups, ops, (i) => this.GROUPS[i] = this.GetGroup(percentil));
			int I = 0;
			var build_one_group = new Action<int> (delegate(int i) {
				Console.WriteLine ("*** Begin of processing eplist ({0}/{1}) ***", i, num_groups);
				this.rows[i] = new_eplist(db, new Random(i+seed));
				Console.WriteLine ("Advance {0}/{1} ({2}, db={3}, timestamp={4})",
				                   I, num_groups, this.rows[i], db.Name, DateTime.Now);
				I++;
			});
			Console.WriteLine ("==== creation with {0} threads", num_build_processors);
			if (num_build_processors == 1 || num_build_processors == 0) {
				for (int i = 0; i < num_groups; ++i) {
					Console.WriteLine ("*** Begin of processing eplist ({0}/{1}) ***", i, num_groups);
					build_one_group (i);
					Console.WriteLine ("*** End of processing eplist ({0}/{1}) ***", i, num_groups);
				}
			} else {
				LongParallel.For (0, num_groups, build_one_group, num_build_processors);
			}
		}

		public object CreateQueryContext(object q)
		{
			var D_rows = new double[this.rows.Length][];
			for (int rowID = 0; rowID < this.rows.Length; ++rowID) {
				var pivs = this.rows[rowID].Pivs;
				var row = D_rows[rowID] = new double[pivs.Length];
				for (int pivID = 0; pivID < pivs.Length; ++pivID) {
					row[pivID] = this.DB.Dist(q, this.DB[pivs[pivID].objID]);
				}
				this.internal_numdists += pivs.Length;
			}
			return D_rows;
		}

		public bool MustReviewItem(object q, int objID, double radius, object query_context)
		{
			var D_rows = (double[][])query_context;
			for (int rowID = 0; rowID < this.rows.Length; ++rowID) {
				var g = this.rows[rowID];
				var item = g.Items[objID];
				var dqp = D_rows[rowID][item.ObjID];
				if (Math.Abs(item.Dist - dqp) > radius) {
					return false;
				}
			}
			return true;
		}
		                               
		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var n = this.DB.Count;
			var D_rows = (double[][])this.CreateQueryContext (q);
			for (int objID = 0; objID < n; ++objID) {
				bool review = true;
				var radius = res.CoveringRadius;
				for (int rowID = 0; rowID < this.rows.Length; ++rowID) {
					var g = this.rows[rowID];
					var item = g.Items[objID];
					var dqp = D_rows[rowID][item.ObjID];
					if (Math.Abs(item.Dist - dqp) > radius) {
						review = false;
						break;
					}
				}
				if (review) {
					var d = this.DB.Dist(q, this.DB[objID]);
					res.Push (objID, d);
				}
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
			var res = new ResultRange (radius, this.DB.Count);
			return this.SearchKNN(q, this.DB.Count, res);
		}
	}
}

