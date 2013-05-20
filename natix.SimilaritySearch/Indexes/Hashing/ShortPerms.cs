// 
//  Copyright 2013  sadit
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
	public class ShortPerms : BasicIndex
	{
		public MetricDB refs;
        public Dictionary<long, List<int>> TABLE;

		public override string ToString ()
		{
			return string.Format ("[SmallPerms num_refs: {0}, db: {1}]",
			                      this.refs.Count, this.DB.Name);
		}

		public override void Save (BinaryWriter Output)
        {
            base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.refs);
            var num_keys = this.TABLE.Count;
            Output.Write ((int)num_keys);
            foreach (var p in this.TABLE) {
                Output.Write ((long)p.Key);
				Output.Write (p.Value.Count);
				PrimitiveIO<int>.WriteVector(Output, p.Value);
            }
		}

		public override void Load (BinaryReader Input)
        {
            base.Load (Input);
			this.refs = SpaceGenericIO.SmartLoad (Input, false);
			var num_keys = Input.ReadInt32 ();
            this.TABLE = new Dictionary<long, List<int>> (num_keys);
            for (int i = 0; i < num_keys; ++i) {
                var key = Input.ReadInt64();
				var len = Input.ReadInt32 ();
				var value = new List<int>(len);
				PrimitiveIO<int>.ReadFromFile(Input, len, value);
                this.TABLE.Add (key, value);
            }
		}

		public ShortPerms () : base()
		{
		}

		public void Build (MetricDB db, int num_refs, Random rand)
		{
			if (num_refs > 16) {
				throw new ArgumentOutOfRangeException(String.Format ("num_refs should be smaller than 16, num_refs: {0}", num_refs));
			}
            this.DB = db;
			int n = db.Count;
            this.refs = new SampleSpace("", db, num_refs, rand);
			var G = new long[n];
			for (int objID = 0; objID < n; ++objID) {
				var u = this.DB[objID];
				var useq = this.GetHash(u);
				G[objID] = useq;
			}
            this.TABLE = new Dictionary<long, List<int>> ();
            for (int objID = 0; objID < n; ++objID) {
                var hash = G[objID];
                List<int> L;
                if (!this.TABLE.TryGetValue(hash, out L)) {
                    L = new List<int>();
                    this.TABLE.Add(hash, L);
                }
                L.Add (objID);
            }
			int m = 0;
			foreach ( var p in this.TABLE ) {
				m += p.Value.Count;
				Console.WriteLine ("@@@@> key: {0}, count: {1}", p.Key, p.Value.Count);
			}
			Console.WriteLine ("===== @@@ hashes: {0}, n: {1}, m: {2}", this.TABLE.Count, n, m);
		}

		public virtual long GetHash (object q)
		{
			var near = new List<ItemPair> ();
			this.internal_numdists += this.refs.Count;
			for (int refID = 0; refID < this.refs.Count; ++refID) {
				var d = this.DB.Dist(q, this.DB[refID]);
				near.Add (new ItemPair(refID, d));
			}
			near.Sort ();
            long hash = 0;
            int i = 0;
            foreach (var p in near) {
				long objID = p.objID;
                hash |= objID << (i << 2);
                ++i;
            }
            return hash;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var hash = this.GetHash (q);
            List<int> L;
            if (!this.TABLE.TryGetValue(hash, out L)) {
                return res;
            }
            foreach (var docID in L) {
                double d = this.DB.Dist (q, this.DB [docID]);
                res.Push (docID, d);
			}
			return res;
		}
	}
}