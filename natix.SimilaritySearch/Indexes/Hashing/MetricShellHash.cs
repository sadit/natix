// 
//  Copyright 2012  sadit
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
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class MetricShellHash : BasicIndex
	{
		public MetricDB R;
		public List<int[]> S;
		public int MAXCAND = int.MaxValue;
		public List<List<int>> InvIndex = new List<List<int>>();
		public MetricShellHash () : base()
		{
		}

		public void CreateInvIndex()
		{
			this.InvIndex = new List<List<int>> ();
			for (int refID = 0; refID < this.R.Count; ++refID) {
				this.InvIndex.Add (new List<int>());
			}
			for (int objID = 0; objID < this.DB.Count; ++objID) {
				var seq = this.S [objID];
				foreach (var c in seq) {
					this.InvIndex [c].Add (objID);
				}
			}
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.R = SpaceGenericIO.Load (Input, false);
			var len = Input.ReadInt32 ();
			this.S = new List<int[]> (len);
			for (int objID = 0; objID < len; ++objID) {
				var size = Input.ReadInt16 ();
				var seq = new int[size];
				PrimitiveIO<int>.LoadVector (Input, size, seq);
				this.S.Add (seq);
			}
			this.CreateInvIndex ();
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.Save(Output, this.R, false);
			Output.Write ((int) this.S.Count);
			foreach (var seq in this.S) {
				Output.Write ((short) seq.Length);
				PrimitiveIO<int>.SaveVector (Output, seq);
			}
		}
		public void Build (MetricDB db, int num_refs, Random rand)
		{
			this.Build (db, new SampleSpace ("", db, num_refs, rand));
		}

		public void Build (MetricDB db, MetricDB sample)
		{
			this.DB = db;
			this.R = sample;
			var n = this.DB.Count;
			this.S = new List<int[]> (n);
			for (int i = 0; i < n; ++i) {
				this.S.Add(null);
			}
			int count = 0;
			var compute_one = new Action<int>(delegate(int objID) {
				var u = this.GetMetricShell(this.DB[objID]);
				this.S[objID] = u;
				++count;
				if (count % 1000 == 0) {
					Console.WriteLine ("==== {0} {1}/{2} db: {3}, k: {4}", this, count, this.DB.Count, this.DB.Name, u.Length);
				}
			});
			ParallelOptions ops = new ParallelOptions();
			ops.MaxDegreeOfParallelism = -1;
			Parallel.ForEach(new ListGen<int>((int i) => i, this.DB.Count), ops, compute_one);
			this.CreateInvIndex ();
		}


		public int[] GetMetricShell (object q)
		{
			var seq = new List<int> ();
			var idx = new DynamicSequentialOrdered ();
			// optimize the following:
			idx.Build (this.R, RandomSets.GetIdentity (this.R.Count));
			List<ItemPair> cache = new List<ItemPair>(this.R.Count);
			// Console.WriteLine ("START GetMetricShell");
			while (idx.Count > 0) {
				cache.Clear();
				DynamicSequential.Stats stats;
				int min_objID, max_objID;
				idx.ComputeDistances(q, cache, out stats, out min_objID, out max_objID);
				for (int i = 0; i < cache.Count; ++i) {
					var obj_min = this.DB [min_objID];
					var obj_cur = this.DB [cache[i].objID];
					if (cache[i].dist >= this.DB.Dist(obj_min, obj_cur)) {
						idx.Remove (cache[i].objID);
					}
				}
				//Console.WriteLine ("min: {0}, min_dist: {1}, refs_size: {2}", min_objID, stats.min, idx.Count);
				seq.Add (min_objID);
			}
			return seq.ToArray ();
		}

		public virtual Result GetNear(int[] seq)
		{
			var cand = new Result (this.MAXCAND);
			var n = this.DB.Count;
			for (int objID = 0; objID < n; ++objID) {
				/*var d = Jaccard (seq, this.S [objID]);
				if (d < 1) {
					cand.Push (objID, d);
				}*/
				var d = StringSpace<int>.PrefixLength(seq, this.S[objID]);
				// var d = StringSpace<int>.LCS(seq, this.S[objID]);
				cand.Push (objID, d);
			}
			return cand;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var qseq = this.GetMetricShell(q);
			var cand = this.GetNear (qseq);
			Console.WriteLine ("=== num_candidates: {0}", cand.Count);
			foreach (var pair in cand) {
				double d = this.DB.Dist (q, this.DB [pair.docid]);
				res.Push (pair.docid, d);
			}
			// 	Console.WriteLine (cand);
			return res;
		}

		double Jaccard (int[] qseq, int[] useq)
		{
			var h = new HashSet<int> (qseq);
			h.IntersectWith (useq);
			double j = h.Count;
			j /= (qseq.Length + useq.Length);
			return 1.0 - j;
		}
	
		string AsString(IList<int> array)
		{
			var s = new System.Text.StringBuilder ();
			s.Append ("[");
			for (int i = 0; i < array.Count; ++i) {
				if (i + 1 == array.Count) {
					s.Append (array[i].ToString());
				} else {
					s.Append (array[i].ToString() + ", ");
				}
			}
			s.Append("]");
			return s.ToString ();
		}
	}
}