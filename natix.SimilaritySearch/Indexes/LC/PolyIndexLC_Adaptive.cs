//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Original filename: natix/SimilaritySearch/Indexes/PolyIndexLC.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;
using natix.Sets;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class PolyIndexLC_Adaptive : PolyIndexLC
	{
        public double MinimumImprovement = 1.05;

		public PolyIndexLC_Adaptive () : base()
        {
        }
        
        public override SearchCost Cost {
            get {
                return new SearchCost(this.DB.NumberDistances, this.internal_numdists);
            }
        }

        public override IResult SearchRange (object q, double radius)
        {
            var R = new ResultRange(radius, this.DB.Count);
            this.SearchKNN(q, R.K, R);
            return R;
        }

        public override IResult SearchKNN (object q, int K, IResult R)
        {
            var Q = new QueryContext(this, q, R);
            Q.SearchKNN();
            return R;
        }

        protected class QueryContext
        {
            PolyIndexLC_Adaptive pmi;
            BitStream32 CANDS;
            BitStream32[] CENTERS_VISITED;
            Dictionary<int,double> dcq_cache;
            object q;
            IResult res;

            public QueryContext(PolyIndexLC_Adaptive pmi, object q, IResult res)
            {
                this.pmi = pmi;
                this.q = q;
                this.res = res;
                this.CANDS = new BitStream32 ();
                var n = pmi.DB.Count;
                //Console.WriteLine ("XXXXXX Begin CANDS");
                CANDS.Write (true, n);
                //Console.WriteLine ("XXXXXX END CANDS");
                this.dcq_cache = new Dictionary<int, double>();
                this.CENTERS_VISITED = new BitStream32[pmi.LC_LIST.Count];
                //Console.WriteLine ("XXXXXX Begin VISITED");
                for (int i = 0; i < pmi.LC_LIST.Count; ++i) {
                    this.CENTERS_VISITED[i] = new BitStream32();
                    this.CENTERS_VISITED[i].Write(false, pmi.LC_LIST[i].CENTERS.Count);
                }
                //Console.WriteLine ("XXXXXX END VISITED");
            }
         
            public void SearchKNN ()
            {
                var n = this.pmi.DB.Count;
                for (int start_item = 0; start_item < n; ++start_item) {
                    //Console.WriteLine ("XXXX searching {0}, outside", start_item);
                    if (this.CANDS [start_item]) { // it is candidate
                        //Console.WriteLine ("XXXX searching {0}", start_item);
                        this.SearchKNN(0, start_item);
                        this.ReviewObject(start_item);
                    }
                }
            }

            public void ReviewObject (int start_item)
            {
                if (this.CANDS[start_item]) {
                    //Console.WriteLine ("XXXX reviewing {0}", start_item);
                    var d = this.pmi.DB.Dist (this.pmi.DB [start_item], this.q);
                    this.res.Push (start_item, d);
                    this.CANDS[start_item] = false; // mark as reviewed and discarded
                }
            }

            protected double Get_dcq(int centerID_objID)
            {
                double dcq;
                if (!this.dcq_cache.TryGetValue (centerID_objID, out dcq)) {
                    dcq = this.pmi.DB.Dist (this.pmi.DB [centerID_objID], q);
                    ++this.pmi.internal_numdists;
                    if (this.CANDS [centerID_objID]) {
                        res.Push (centerID_objID, dcq);
                        this.CANDS [centerID_objID] = false;
                    }
                }
                return dcq;
            }

            public void SearchKNN (int start_lambda, int start_item)
            {
                /*if (stack_size > 128) {
                    throw new Exception (String.Format ("stack_size: {0}", stack_size));                    
                }*/
                var lc_list = this.pmi.LC_LIST;
                var lc = lc_list [start_lambda];
                var _start_lambda_pp = start_lambda + 1;
                var lambda = lc_list.Count;
                int centerID;
                int centerID_objID;
                float cov;
                lc.GetContainerCenter (start_item, out centerID, out centerID_objID, out cov);
                if (centerID_objID < 0) {
                    // centerID is a center on the current lc, this class is not of interest
                    // we must check on the next level
                    if (_start_lambda_pp < lambda) {
                        this.SearchKNN (_start_lambda_pp, start_item);
                    }
                    return;
                }
                if (this.CENTERS_VISITED [start_lambda][centerID]) {
                    // some other search path yielded to this centerID before,
                    // so we are in a circular path, so we must review the object
                    if (centerID_objID < 0) {
                        this.SearchKNN(_start_lambda_pp, start_item);
                    }
                    return;
                }
                this.CENTERS_VISITED[start_lambda][centerID] = true;
                double dcq = this.Get_dcq(centerID_objID);
                bool review = ( dcq <= res.CoveringRadius + cov);
                var rs = lc.SEQ.Unravel(centerID);
                var count1 = rs.Count1;
                if (review) {
                    // all items in the same class should be tested on the next level because
                    // they will not discarded in the current level
                    /*if (_start_lambda_pp < lambda) {
                        for (int c = 1; c <= count1; ++c) {
                            var item = rs.Select1(c);
                            if (this.CANDS[item]) {
                                this.SearchKNN(_start_lambda_pp, item);
                            }
                        }
                    }*/
                } else {
                    // all items in the same class are automatically discarded
                    for (int c = 1; c <= count1; ++c) {
                        // Console.WriteLine ("==> {0}", c);
                        var item = rs.Select1(c);
                        this.CANDS[item] = false; // mark as reviewed and discarded
                    }
                }
            }
        }
    }
}
