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
	public class MKnrLSH : BasicIndex
	{
        public KnrLSH[] A;

		public override void Save (BinaryWriter Output)
        {
            Output.Write (A.Length);
            foreach (var a in A) {
                a.Save(Output);
            }
		}

		public override void Load (BinaryReader Input)
        {
            var len = Input.ReadInt32 ();
            this.A = new KnrLSH[len];
            for (int i = 0; i < len; ++i) {
                var a = new KnrLSH();
                a.Load(Input);
                this.A[i] = a;
            }
		}

		public MKnrLSH () : base()
		{
		}

		public void Build (MetricDB db, int num_instances)
        {
            this.A = new KnrLSH[num_instances];
            this.DB = db;
            for (int i = 0; i < num_instances; ++i) {
                var a = new KnrLSH();
                a.Build(db);
                this.A[i] = a;
            }
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
            var L = new HashSet<int>();
            foreach (var a in this.A) {
                var h = a.GetHashKnr(q);
                IList<int> M;
                if (a.TABLE.TryGetValue(h, out M)) {
                    foreach (var docID in M) {
                        L.Add(docID);
                    }
                }
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