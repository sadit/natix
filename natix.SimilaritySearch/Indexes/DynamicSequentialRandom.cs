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
//   Original filename: natix/SimilaritySearch/Indexes/DynamicSequential.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NDesk.Options;
using natix;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The sequential index
	/// </summary>
	public class DynamicSequentialRandom : DynamicSequential
	{        
		public SkipList2<int> DOCS;
		public Random rand;
		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicSequentialRandom () : base()
		{
			this.rand = new Random();
		}

		public DynamicSequentialRandom (int random_seed)
		{
			this.rand = new Random(random_seed);
		}

		public override void Remove (int docid)
		{
			this.DOCS.Remove(docid, null);
		}

		public int GetRandom ()
		{
			if (this.DOCS.Count == 0) {
				throw new KeyNotFoundException ("GetRandom cannot select an item from an empty set");
			}
			var docid = this.rand.Next (0, this.DB.Count);
			var node = this.DOCS.FindNode (docid, null);
			//Console.WriteLine ("RANDOM {0}, FIRST: {1}, LAST: {2}", docid, this.DOCS.GetFirst(), this.DOCS.GetLast());
			if (node == this.DOCS.TAIL) {
				return this.DOCS.GetLast ();
			}
			if (node == this.DOCS.HEAD) {
				return this.DOCS.GetFirst();
			}
			return node.data;
		}

		/// <summary>
		/// API build command
		/// </summary>
		public virtual void Build (MetricDB db, IList<int> sample = null)
		{
			this.DB = db;
			if (sample == null) {
				sample = RandomSets.GetExpandedRange (this.DB.Count);
			}
			this.DOCS = new SkipList2<int> (0.5, (x,y) => x.CompareTo (y));
			var ctx = new SkipList2<int>.AdaptiveContext(true, this.DOCS.HEAD);
			foreach (var s in sample) {
				this.DOCS.Add(s, ctx);
			}
		}

        public override IEnumerable<int> Iterate ()
        {
            return this.DOCS.Traverse();
        }

        public override int Count {
            get {
                return this.DOCS.Count;
            }
        }
	}
}
