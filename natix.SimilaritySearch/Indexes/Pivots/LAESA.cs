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
	public class LAESA : BasicIndex, IndexSingle
	{
		public List<double>[] DIST;
		public MetricDB PIVS;

		public LAESA ()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.PIVS = SpaceGenericIO.SmartLoad(Input, false);
			this.DIST = new List<double>[this.PIVS.Count];
			for (int i = 0; i < this.PIVS.Count; ++i) {
				this.DIST[i] = new List<double>(this.DB.Count);
				PrimitiveIO<double>.ReadFromFile(Input, this.DB.Count, this.DIST[i]);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave (Output, this.PIVS);
			for (int i = 0; i < this.PIVS.Count; ++i) {
				PrimitiveIO<double>.WriteVector(Output, this.DIST[i]);
			}
		}

		public void Build (LAESA idx, int num_pivs)
		{
			this.DB = idx.DB;
			var P = (idx.PIVS as SampleSpace);
			var S = new int[num_pivs];
			this.DIST = new List<double>[num_pivs];
			int I = 0;
			for (int pivID = 0; pivID < num_pivs; ++pivID) {
				S[pivID] = P.SAMPLE[pivID];
				this.DIST[pivID] = idx.DIST[pivID];
				if (I % 10 == 0) {
					Console.WriteLine("LAESA Build, advance {0}/{1} (approx.) ", I, num_pivs);
				}
				I++;
			}
			this.PIVS = new SampleSpace("", P.DB, S);
		}

		public void Build (MetricDB db, int num_pivs)
		{
			this.DB = db;
			this.PIVS = new SampleSpace("", db, num_pivs);
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
			ParallelOptions ops = new ParallelOptions ();
			ops.MaxDegreeOfParallelism = -1;
			Parallel.For (0, num_pivs, ops, one_pivot);
//			for (int i = 0; i < num_pivs; ++i) {
//				one_pivot(i);
//			}
		}

		public override IResult SearchKNN (object q, int K, IResult res)
        {		
            var m = this.PIVS.Count;
            var n = this.DB.Count;
            var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
            var dqp_cache = new Dictionary<int,double>();
			// todo: randomize
			for (int docID = 0; docID < n; ++docID) {
				if (dqp_cache.ContainsKey(docID)) {
					continue;
				}
				bool check_object = true;
				for (int __pivID = 0; __pivID < m; ++__pivID) {
                    var db_pivID = _PIVS[__pivID];
                    double dqp;
                    if (!dqp_cache.TryGetValue(db_pivID, out dqp)) {
                        dqp = this.DB.Dist(q, this.DB[db_pivID]);
                        ++this.internal_numdists;
                        dqp_cache[db_pivID] = dqp;
                        if (db_pivID >= docID) {
                            res.Push(db_pivID, dqp);
                            if (db_pivID == docID) {
                                check_object = false;
                                break;
                            }
                        }
                    }
					var dpu = this.DIST[__pivID][docID];
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

//		public override IResult SearchRange (object q, double radius)
//		{
//			var m = this.PIVS.Count;
//			var n = this.DB.Count;
//			HashSet<int> A = null;
//			HashSet<int> B = null;
//			for (int piv_id = 0; piv_id < m; ++piv_id) {
//				var dqp = this.DB.Dist (q, this.PIVS [piv_id]);
//                ++this.internal_numdists;
//				if (A == null) {
//					A = new HashSet<int>();
//					for (int i = 0; i < n; ++i) {
//						var dpu = this.DIST[piv_id][i];
//						if (Math.Abs (dqp - dpu) <= radius) {
//							A.Add(i);
//						}
//					}
//				} else {
//					B = new HashSet<int>();
//					foreach (var i in A) {
//						var dpu = this.DIST[piv_id][i];
//						if (Math.Abs (dqp - dpu) <= radius) {
//							B.Add(i);
//						}
//					}
//					A = B;
//				}
//			}
//			var res = new Result(this.DB.Count, false);
//			foreach (var docid in A) {
//				var d = this.DB.Dist(this.DB[docid], q);
//				if (d <= radius) {
//					res.Push(docid, d);
//				}
//			}
//			return res;
//		}


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
            var res = new Result(this.DB.Count, false);
            var dqp_cache = new Dictionary<int, double>();
            var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
            var n = this.DB.Count;
            var m = this.PIVS.Count;
            for (int docID = 0; docID < n; ++docID) {
                if (dqp_cache.ContainsKey(docID)) {
                    continue;
                }
                bool check_object = true;
                for (int __pivID = 0; __pivID < m; ++__pivID) {
                    var db_pivID = _PIVS[__pivID];
                    double dqp;
                    if (!dqp_cache.TryGetValue(db_pivID, out dqp)) {
                        dqp = this.DB.Dist(q, this.DB[db_pivID]);
                        ++this.internal_numdists;
                        dqp_cache[db_pivID] = dqp;
                        if (db_pivID >= docID) {
                            if (dqp <= radius) {
                                res.Push(db_pivID, dqp);
                            }
                            if (db_pivID == docID) {
                                check_object = false;
                                break;
                            }
                        }
                    }
                    var dpu = this.DIST[__pivID][docID];
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

