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
	public class KnrLSH : BasicIndex
	{
		public int K;
		public Index R;
        public Dictionary<int, List<int>> TABLE;       

		public override string ToString ()
		{
			return string.Format ("[KnrLSH K: {0}, num_refs: {1}, num_symbols: {2}, db: {3}]",
			                      this.K, this.R.DB.Count,
			                      this.TABLE == null ? "undefined" : this.TABLE.Count.ToString(), this.DB.Name);
		}

		public override void Save (BinaryWriter Output)
        {
            base.Save (Output);
            Output.Write (this.K);
            IndexGenericIO.Save (Output, this.R);
            var num_keys = this.TABLE.Count;
            Output.Write ((int)num_keys);
            foreach (var p in this.TABLE) {
                Output.Write (p.Key);
				Output.Write (p.Value.Count);
				PrimitiveIO<int>.SaveVector(Output, p.Value);
            }
		}

		public override void Load (BinaryReader Input)
        {
            base.Load (Input);
            this.K = Input.ReadInt32 ();
            this.R = IndexGenericIO.Load (Input);
            var num_keys = Input.ReadInt32 ();
            this.TABLE = new Dictionary<int, List<int>> (num_keys);
            for (int i = 0; i < num_keys; ++i) {
                var key = Input.ReadInt32();
				var len = Input.ReadInt32 ();
				var value = new List<int>(len);
				PrimitiveIO<int>.LoadVector(Input, len, value);
                this.TABLE.Add (key, value);
            }
		}

		public KnrLSH () : base()
		{
		}

		public KnrLSH (KnrLSH other) : base()
		{
			this.DB = other.DB;
			this.K = other.K;
			this.R = other.R;
			this.TABLE = other.TABLE;
		}

		public void Build (MetricDB db, int K, int num_refs, Random rand)
		{
            this.DB = db;
			int n = db.Count;
            // valid values to be used as parameters
            // numrefs <= 255
            // K <= 4
			if (K > 4) {
				throw new ArgumentOutOfRangeException (String.Format("K should be between 1 to 4, K={0}", K));
			}
			if (num_refs > 255) {
				throw new ArgumentOutOfRangeException (String.Format("num_refs should be between 1 to 255, num_refs={0}", num_refs));
			}
			this.K = K;
            var refs = new SampleSpace("", db, num_refs);
            var seq = new Sequential();
            seq.Build(refs);
            this.R = seq;
			int[] G = new int[n];
			for (int objID = 0; objID < n; ++objID) {
				var u = this.DB[objID];
				var useq = this.GetHashKnr(u);
				G[objID] = useq;
				if (objID % 10000 == 0) {
					Console.WriteLine ("computing knrlsh {0}/{1} (adv. {2:0.00}%, db: {3}, K: {4}, curr. time: {5})", objID, n, objID*100.0/n, this.DB.Name, this.K, DateTime.Now);
				}
			}
            this.TABLE = new Dictionary<int, List<int>> ();
            for (int objID = 0; objID < n; ++objID) {
                var hash = G[objID];
                List<int> L;
                if (!this.TABLE.TryGetValue(hash, out L)) {
                    L = new List<int>();
                    this.TABLE.Add(hash, L);
                }
                L.Add (objID);
            }
		}

		public int EncodeKnr(List<int> near)
		{
			int hash = 0;
			int i = 0;
			foreach (var refID in near) {
				hash |= refID << (i << 3);
				++i;
			}
			return hash;
		}

		public virtual int GetHashKnr (object q)
		{
            this.internal_numdists-=this.R.Cost.Internal;
			var near = this.R.SearchKNN(q, this.K);
            this.internal_numdists+=this.R.Cost.Internal;
			// var qseq = new  int[this.K];
            var hash = 0;
            // var numbits = ListIFS.GetNumBits(this.R.DB.Count);
            int i = 0;
            foreach (var p in near) {
                // var shift = numbits * i;
                hash |= p.docid << (i << 3);
                ++i;
            }
            return hash;
		}

		public virtual HashSet<int> GetNear(object q)
		{
			var hash = this.GetHashKnr (q);
			List<int> L;
			if (this.TABLE.TryGetValue(hash, out L)) {
				return new HashSet<int>(L);
			} else {
				return new HashSet<int>();
			}
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
            foreach (var docID in this.GetNear(q)) {
                double d = this.DB.Dist (q, this.DB [docID]);
                res.Push (docID, d);
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
            return this.SearchKNN(q, this.DB.Count, new ResultRange(radius));
		}
	}
}