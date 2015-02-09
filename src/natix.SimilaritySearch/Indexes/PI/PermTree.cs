//
//  Copyright 2013  Eric S. Tellez
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
using System.Collections.Generic;
using System;

namespace natix.SimilaritySearch
{
	public class PermTree : BasicIndex
	{
		Node root;
		public int MAXCAND = 1000;

		public PermTree ()
		{
		}

		public void Build(MetricDB db, int bucketSize)
		{
			if (bucketSize < 4 || bucketSize > 16) {
				throw new ArgumentOutOfRangeException ("bucketSize must be a value between 4 and 16");
			}
			this.DB = db;
			Random rand = new Random ();
			this.root = new Node (db, rand, bucketSize);
			for (int objID = 0; objID < db.Count; ++objID) {
				this.root.Add (objID);
			}
			if (!this.root.IsLeaf) {
				this.root.Build (db, rand, bucketSize);
			}
		}

		public override IResult SearchKNN(object q, int k, IResult res)
		{
			this.root.Search (this.DB, q, res, this.MAXCAND);
			return res;
		}

		public class Node
		{
			public int[] refs;
			public List<int> bag;
			public Dictionary<long, Node> children;
			int count = 0;

			public Node(MetricDB db, Random rand, int bucketSize)
			{
				this.bag = new List<int>();
			}

			public long ComputeFingerprint(object u, MetricDB db)
			{
				byte[] near = new byte[refs.Length];
				double[] dist = new double[refs.Length];

				for (byte i = 0; i < refs.Length; ++i) {
					int refID = refs [i];
					near [i] = i;
					dist [i] = db.Dist (db [refID], u);
				}
				Array.Sort<double,byte> (dist, near);
				near = RandomSets.GetInverse (near);
				long h = 0;
				for (byte i = 0; i < refs.Length; ++i) {
					h |= ((long)(near[i])) << (i << 2); // this is enough for 16 references
				}
				return h;
			}

			public void Add(int objID) {
				this.bag.Add (objID);
				++this.count;
			}
		
			public void Build(MetricDB db, Random rand, int bucketSize)
			{
				this.refs = RandomSets.GetRandomSubSet(this.bag.Count, bucketSize, rand);
				for (int i = 0; i < this.refs.Length; ++i) {
					this.refs [i] = this.bag [this.refs [i]];
				}
				this.children = new Dictionary<long, Node>();
				foreach (var objID in this.bag) {
					var h = this.ComputeFingerprint (db [objID], db);
					Node node;
					if (!this.children.TryGetValue (h, out node)) {
						node = new Node (db, rand, bucketSize);
						this.children.Add (h, node);
					}
					node.Add (objID);
				}
				this.bag = null;
				foreach (var p in this.children) {
					if (!p.Value.IsLeaf) {
						p.Value.Build (db, rand, bucketSize);
					}
				}
			}

			public bool IsLeaf {
				get {
					return this.count < 32;
				}
			}

			public double distL1 (long a, long b)
			{
				double s = 0;
				for (int i = 0; i < 16; ++i) {
					s += Math.Abs ((0xF & a) - (0xF & b));
					a >>= 4;
					b >>= 4;
				}
				return s;
			}

			public double distL2 (long a, long b)
			{
				double s = 0;
				for (int i = 0; i < 16; ++i) {
					var x = (0xF & a) - (0xF & b);
					s += x * x;
					a >>= 4;
					b >>= 4;
				}
				return s; // sin raiz, solo nos interesa el orden
			}

			public void Search(MetricDB db, object q, IResult res, int numCandidates)
			{
				if (this.IsLeaf) {
					foreach (var docID in this.bag) {
						var d = db.Dist (db [docID], q);
						res.Push (docID, d);
					}
				} else {
					var hash = this.ComputeFingerprint (q, db);
					long[] near = new long[this.children.Count];
					double[] dist = new double[this.children.Count];
					int i = 0;
					foreach (var p in this.children.Keys) {
						near [i] = p;
						dist [i] = distL1 (hash, p);
						++i;
					}
					Array.Sort<double, long> (dist, near);
					dist = null;
					i = 0;
					while (i < near.Length && numCandidates > 0) {
						var node = this.children [near [i]];
						node.Search (db, q, res, numCandidates);
						numCandidates -= node.count;
						++i;
					}
				}
			}
		}
	}
}
