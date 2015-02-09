// 
//  Copyright 2013  Eric Sadit Tellez Avila
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
using System.Collections.Generic;
using natix.CompactDS;
using natix.Sets;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
    public class ManyKnr : BasicIndex
	{
		public KnrSeqSearch[] Indexes;
		public int MAXCAND;

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);	    
			Output.Write(this.MAXCAND);
			Output.Write(this.Indexes.Length);
			CompositeIO<KnrSeqSearch>.SaveVector(Output, this.Indexes);
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.MAXCAND = Input.ReadInt32();
			var len = Input.ReadInt32();
			this.Indexes = new KnrSeqSearch[len];
			CompositeIO<KnrSeqSearch>.LoadVector(Input, len, this.Indexes);
		}

		public ManyKnr () : base()
		{

		}
	
		public ManyKnr (ManyKnr parent, int num_indexes)
		{
			this.DB = parent.DB;
			this.MAXCAND = parent.MAXCAND;
			this.Indexes = new KnrSeqSearch[num_indexes];
			for (int i = 0; i < num_indexes; ++i) {
				this.Indexes [i] = parent.Indexes [i];
			}
		}

		public void Build (MetricDB db, int num_indexes, int num_refs_per_instance, int k, int MAXCAND)
		{
			var seed = RandomSets.GetRandomInt ();
			this.DB = db;
			this.MAXCAND = MAXCAND;
			this.Indexes = new KnrSeqSearch[num_indexes];
			for (int i = 0; i < num_indexes; ++i) {
				this.Indexes [i] = new KnrSeqSearch();
				this.Indexes [i].Build (db, new Random(seed+i), num_refs_per_instance, k, MAXCAND); 
			}
		}

		public void BuildDoublingReferences (MetricDB db, int num_indexes, int smaller_num_refs, int k, int MAXCAND)
		{
			var seed = RandomSets.GetRandomInt ();
			this.DB = db;
			this.MAXCAND = MAXCAND;
			this.Indexes = new KnrSeqSearch[num_indexes];
			for (int i = 0; i < num_indexes; ++i) {
				this.Indexes [i] = new KnrSeqSearch();
				this.Indexes [i].Build (db, new Random(seed+i), smaller_num_refs << i, k, MAXCAND); 
			}
		}

		public void CastIndexes(Func<KnrSeqSearch,KnrSeqSearch> castfun)
		{
			for (int i = 0; i < this.Indexes.Length; ++i) {
				this.Indexes[i] = castfun(this.Indexes[i]);
			}
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			//int maxcand = 1+Math.Abs (this.MAXCAND) / this.Indexes.Length;
			//Console.WriteLine ("MAXCAND: {0}, numindexes: {1}, maxcand-static: {2}", maxcand, this.Indexes.Length, this.MAXCAND);
			var map = new Dictionary<int,double> ();
			foreach (var knrindex in this.Indexes) {
				var seq = knrindex.GetKnr (q);
				foreach (var c in knrindex.SearchKNN (seq, q, null, -this.MAXCAND)) {
					double dist;
					if (map.TryGetValue (c.ObjID, out dist)) {
						map [c.ObjID] = Math.Min (dist, c.Dist);
					} else {
						map.Add (c.ObjID, dist);
					}
				}
			}
			var cand = new ResultTies (this.MAXCAND);
			foreach (var p in map) {
				cand.Push (p.Key, p.Value);
			}
			foreach (var p in cand) {
				double d = this.DB.Dist (q, this.DB [p.ObjID]);
				res.Push (p.ObjID, d);
			}
			return res;
		}
	}
}