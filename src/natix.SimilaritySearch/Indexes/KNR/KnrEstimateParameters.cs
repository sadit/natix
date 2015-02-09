//
//   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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

using System;
using System.IO;
using System.Collections.Generic;
using natix.CompactDS;
using natix.Sets;
using natix.SortingSearching;
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
	public class KnrEstimateParameters
	{
		public KnrEstimateParameters ()
		{
		}

		public static int EstimateKnrEnsuringSharedNeighborhoods(MetricDB db, Index refs, int k, int numQueries = 256)
		{
			// this strategy consist on ensure that neighborhoods of the query and all its knn are shared
			// update: we introduce a probability to reduce noisy hard queries
			// NOTICE It cannot be adjusted for 1-nn because we are using database items as training objects
			// it will produce valid values for 2-nn and more
			Sequential seq = new Sequential ();
			var overlappingMinProb = 1.0;
			if (k < 10) {
				overlappingMinProb = 1.0;
			}
			seq.Build (db);
			var n = db.Count;
			var Kmax = 128; // large k will need no extra items, but smaller ones (1 or 2) will need a small constant
			var Kmin = 1;

			foreach (var qID in RandomSets.GetRandomSubSet (numQueries, n)) {
				var q = db [qID];
				var qknr = Result2Sequence(refs.SearchKNN(q, Kmax));
				var list = new List<int[]> (k);

				foreach (var p in seq.SearchKNN (db [qID], k)) {
					list.Add (Result2Sequence(refs.SearchKNN(db[p.ObjID], Kmax)));
				}

				var qset = new HashSet<int>();
				var overlapping = 0;

				for (int i = 0; i < Kmin; ++i) {
					qset.Add (qknr [i]);
				}
				for (int i = 0; i < Kmax && overlapping < list.Count * overlappingMinProb; ++i) {
					qset.Add (qknr [i]);
					overlapping = 0;
					for (int j = 0; j < list.Count; ++j) {
						if (list [j] == null) {
							++overlapping;
						} else if (qset.Contains(list [j] [i])) {
							list [j] = null;
							++overlapping;
						}
					}
					Kmin = Math.Max (Kmin, i + 1);
				}
			}
			return Kmin;
		}
//
//		public static KnrFP EstimateKnrJaccard(MetricDB db, Index refs, int k, int numQueries = 256)
//		{
//			var Kmax = 128; // large k will need no extra items, but smaller ones (1 or 2) will need a small constant
//			var Kmin = 1;
//			var minrecall = 0.95;
//			var minmatches = Math.Ceiling(minrecall * k);
//			var knrmax = new KnrFP ();
//			knrmax.Build (db, refs, Kmax);
//			// this strategy consist on ensure that neighborhoods of the query and all its knn are shared
//			// update: we introduce a probability to reduce noisy hard queries
//			Sequential seq = new Sequential ();
//			seq.Build (db);
//			var n = db.Count;
//
//			var D = new int[n];
//			foreach (var qID in RandomSets.GetRandomSubSet (numQueries, n)) {
//				var q = db [qID];
//				var qknr = Result2Sequence(refs.SearchKNN(q, Kmax));
//
//				var qset = new HashSet<int>();
//				foreach (var p in seq.SearchKNN (db [qID], k)) {
//					qset.Add (p.docid);
//				}
//				for (int i = 0; i < n; ++i) {
//					D [i] = 0;
//				}
//				for (int i = 0; i < n; ++i) {
//					for (int j = 0; j < Kmax && matches < minmatches; ++j) {
//					}
//					for (int j = 0; j < Kmax; ++j) {
//
//					}
//					D []
//				}
//				for (int i = 0; i < Kmin; ++i) {
//
//				}
//
//				for (int i = 0; i < Kmax && overlapping < list.Count ; ++i) {
//					qset.Add (qknr [i]);
//					overlapping = 0;
//					for (int j = 0; j < list.Count; ++j) {
//						if (list [j] == null) {
//							++overlapping;
//						} else if (qset.Contains(list [j] [i])) {
//							list [j] = null;
//							++overlapping;
//						}
//					}
//					Kmin = Math.Max (Kmin, i + 1);
//				}
//			}
//			return Kmin;
//		}

		public static int[] Result2Sequence (IResult knr)
		{
			var seq = new int[knr.Count];
			int i = 0;
			foreach (var p in knr) {
				seq [i] = p.ObjID;
				++i;
			}
			return seq;
		}
	}
}

