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
        public Dictionary<int, IList<int>> TABLE;       

		public override void Save (BinaryWriter Output)
        {
            base.Save (Output);
            Output.Write (this.K);
            IndexGenericIO.Save (Output, this.R);
            var num_keys = this.TABLE.Count;
            Output.Write ((int)num_keys);
            foreach (var p in this.TABLE) {
                Output.Write (p.Key);
                ListIGenericIO.Save(Output, p.Value);
            }
		}

		public override void Load (BinaryReader Input)
        {
            base.Load (Input);
            this.K = Input.ReadInt32 ();
            this.R = IndexGenericIO.Load (Input);
            var num_keys = Input.ReadInt32 ();
            this.TABLE = new Dictionary<int, IList<int>> (num_keys);
            for (int i = 0; i < num_keys; ++i) {
                var key = Input.ReadInt32();
                var value = ListIGenericIO.Load(Input);
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

		public void Build (MetricDB db)
		{
            this.DB = db;
			int n = db.Count;
            // valid values to be used as parameters
            // numrefs <= 255
            // K <= 4
            var numrefs = 16;
            this.K = 4;
            var refs = new SampleSpace("", db, numrefs);
            var seq = new Sequential();
            seq.Build(refs);
            this.R = seq;
			int[] G = new int[n];
			int I = 0;
			Action<int> compute = delegate (int i) {
				if (I % 1000 == 0) {
					Console.WriteLine ("computing knrlsh {0}/{1} (adv. {2:0.00}%, curr. time: {3})", I, n, I*100.0/n, DateTime.Now);
				}
				++I;
				var u = this.DB[i];
				var useq = this.GetHashKnr(u);
                G[i] = useq;
			};
			Parallel.For(0, n, compute);
            this.TABLE = new Dictionary<int, IList<int>> ();
            for (int docID = 0; docID < n; ++docID) {
                var hash = G[docID];
                IList<int> L;
                if (!this.TABLE.TryGetValue(hash, out L)) {
                    L = new List<int>();
                    this.TABLE.Add(hash, L);
                }
                L.Add (docID);
            }
		}

		public int GetHashKnr (object q)
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

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var hash = this.GetHashKnr (q);
            IList<int> L;
            if (!this.TABLE.TryGetValue(hash, out L)) {
                return res;
            }
            foreach (var docID in L) {
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