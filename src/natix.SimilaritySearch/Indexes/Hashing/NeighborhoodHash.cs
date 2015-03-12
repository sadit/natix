// 
//  Copyright 2012-2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
	public class NeighborhoodHash : BasicIndex
	{
		public int SymbolsPerHash = 0;
		public int CountSymbolBits = 0;
		public int NeighborhoodExpansion = 0;

		public Index R;
        public Dictionary<long, List<int>> hashTable;

		static int countBits(int numrefs)
		{
			return (int)Math.Ceiling (Math.Log (numrefs, 2));
		}

		public override void Save (BinaryWriter Output)
        {
            base.Save (Output);
			Output.Write (this.SymbolsPerHash);
			Output.Write (this.NeighborhoodExpansion);
            IndexGenericIO.Save (Output, this.R);
			Output.Write (this.hashTable.Count);
            foreach (var p in this.hashTable) {
                Output.Write (p.Key);
				Output.Write (p.Value.Count);
				PrimitiveIO<int>.SaveVector(Output, p.Value);
            }
		}

		public override void Load (BinaryReader Input)
        {
            base.Load (Input);
			this.SymbolsPerHash = Input.ReadInt32 ();
			this.NeighborhoodExpansion = Input.ReadInt32 ();
            this.R = IndexGenericIO.Load (Input);
			this.CountSymbolBits = countBits(this.R.DB.Count);
            var num_keys = Input.ReadInt32 ();
            this.hashTable = new Dictionary<long, List<int>> (num_keys);

			for (int i = 0; i < num_keys; ++i) {
                var key = Input.ReadInt64 ();
				var len = Input.ReadInt32 ();
				// Console.WriteLine ("hash: {0}, popcount: {1}", key, len);
				var value = new List<int>(len);
				PrimitiveIO<int>.LoadVector(Input, len, value);
                this.hashTable.Add (key, value);
            }
		}

		public NeighborhoodHash () : base()
		{
		}

		public void Build (MetricDB db, int symbolsPerHash)
		{
			this.Build (db, symbolsPerHash, symbolsPerHash, new Random());
		}

		public void Build (MetricDB db, int symbolsPerHash, int neighborhoodExpansion, Random rand)
		{
            this.DB = db;
			int n = db.Count;
			this.SymbolsPerHash = symbolsPerHash; // very small values, i.e., 2, 3, 4
			this.NeighborhoodExpansion = neighborhoodExpansion; // neighborhoodExpansion >= symbolsPerHash
			var numrefs = (int)(Math.Pow(n, 1.0 / this.SymbolsPerHash));
			this.CountSymbolBits = countBits(numrefs);

            var refs = new SampleSpace("", db, numrefs, rand);
			//var seq = new MILCv3 ();
			if (numrefs > 200) {
				var seq = new NILC ();
				var ilc = new ILC ();
				ilc.Build (refs, this.SymbolsPerHash, 1);
				seq.Build (ilc);
				this.R = seq;
			} else {
				var seq = new Sequential();
				seq.Build (refs);
				this.R = seq;
			}
			var G = new KnrFP ();
			G.Build (db, this.R, this.SymbolsPerHash);
			var knrcopy = new int[this.SymbolsPerHash];
            this.hashTable = new Dictionary<long, List<int>> ();
			for (int objID = 0; objID < n; ++objID) {
				var knr = G [objID] as int[];
				knr.CopyTo (knrcopy, 0);
				var hash = this.EncodeKnr (knrcopy); // EncodeKnr destroys the reference order

                List<int> L;
                if (!this.hashTable.TryGetValue(hash, out L)) {
                    L = new List<int>();
                    this.hashTable.Add(hash, L);
                }
                L.Add (objID);
            }
			double avg_len = 0;
			double avg_len_sq = 0;

			foreach (var list in this.hashTable.Values) {
				avg_len += list.Count;
				avg_len_sq += list.Count * list.Count;
			}
			avg_len /= this.hashTable.Count;
			avg_len_sq /= this.hashTable.Count;
			Console.WriteLine ("=== created hash table with {0} keys, items: {1}, popcount mean: {2}, popcount stddev: {3}",
			                   this.hashTable.Count, n, avg_len, Math.Sqrt(avg_len_sq - avg_len * avg_len));
		}	

		// bubble sort is simple and faster for small arrays, and even it adapts to the instance complexity
		public static void BubbleSort(int[] seq)
		{
			int swaps;
			do {
				swaps = 0;
				for (int i = 1; i < seq.Length; ++i) {
					var pred = i - 1;
					if (seq [pred] > seq [i]) {
						++swaps;
						var tmp = seq [i];
						seq [i] = seq [pred];
						seq [pred] = tmp;
					}
				}
			} while (swaps > 0);
		}

		public long EncodeKnr(int[] seq)
		{
			BubbleSort (seq);
			//Array.Sort (seq);
			long hash = seq [0];
			int shift = this.CountSymbolBits;
			long u;
			for (int i = 1; i < this.SymbolsPerHash; ++i, shift += this.CountSymbolBits) {
				u = seq [i];
				hash |= u << shift;
			}
			return hash;
		}

		public List<int> RankRefs(object q)
		{
			var start_cost = this.DB.NumberDistances;
			var near = this.R.SearchKNN(q, this.R.DB.Count);
			this.internal_numdists += this.DB.NumberDistances - start_cost;
			var list = new List<int>(near.Count);
			foreach (var p in near) {
				list.Add (p.ObjID);
			}
			return list;
		}

		public IEnumerable<List<int>> FetchPostingLists (List<int> rankedList, int start)
		{
			var seq = new int[this.SymbolsPerHash];
			//var exp = Math.Min(idx.NeighborhoodExpansion, start + 10);
			var exp = this.NeighborhoodExpansion;
			switch (this.SymbolsPerHash) {
				case 2:
				//for (int i = 0; i < exp - 1; ++i) {
				for (int j = start + 1; j < exp; ++j) {
					seq [0] = rankedList [start];
					seq [1] = rankedList [j];
					var hash = this.EncodeKnr (seq);
					List<int> f;
					if (this.hashTable.TryGetValue (hash, out f)) {
						yield return f;
					}
				}
				//}
				break;
				case 3:
				//for (int i = 0; i < exp - 2; ++i) {
				for (int j = start + 1; j < exp - 1; ++j) {
					for (int k = j + 1; k < exp; ++k) {
						seq [0] = rankedList [start];
						seq [1] = rankedList [j];
						seq [2] = rankedList [k];
						var hash = this.EncodeKnr (seq);
						List<int> f;
						if (this.hashTable.TryGetValue (hash, out f)) {
							yield return f;
						}
					}
				}
				//}
				break;
				case 4:
				//for (int i = 0; i < exp - 3; ++i) {
				for (int j = start + 1; j < exp - 2; ++j) {
					for (int k = j + 1; k < exp - 1; ++k) {
						for (int l = k + 1; l < exp; ++l) {
							seq [0] = rankedList [start];
							seq [1] = rankedList [j];
							seq [2] = rankedList [k];
							seq [3] = rankedList [l];
							var hash = this.EncodeKnr (seq);
							List<int> f;
							if (this.hashTable.TryGetValue (hash, out f)) {
								yield return f;
							}
						}
					}
				}
				//}
				break;
				default:
				throw new ArgumentOutOfRangeException ("Invalid NeighborhoodExpansion");
			}
		}

		public void InternalSearchKNN (object q, IResult res, IEnumerable<List<int>> postingLists, HashSet<int> evaluated)
		{
			if (evaluated == null) {
				foreach (List<int> list in postingLists) {
					foreach (var docID in list) {
						double d = this.DB.Dist (q, this.DB [docID]);
						res.Push (docID, d);
					}
				}
			} else {
				foreach (List<int> list in postingLists) {
					foreach (var docID in list) {
						if (evaluated.Add (docID)) {
							double d = this.DB.Dist (q, this.DB [docID]);
							res.Push (docID, d);
						}
					}
				}
			}
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			int exp = this.NeighborhoodExpansion - this.SymbolsPerHash + 1;
			var rankedList = this.RankRefs (q);
			for (int start = 0; start < exp; ++start) {
				var postingLists = this.FetchPostingLists (rankedList, start);
				this.InternalSearchKNN (q, res, postingLists, null);
			}
			return res;
		}
	}
}