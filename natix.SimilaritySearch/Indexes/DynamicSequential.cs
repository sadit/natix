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
	public class DynamicSequential : BasicIndex
	{
		public SkipList2<int> DOCS;
		Random rand = new Random();
		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicSequential ()
		{
		}

		public void Remove (int docid)
		{
			this.DOCS.Remove(docid, null);
		}

		public void Remove (IEnumerable<int> docs)
		{	
			foreach (var docid in docs) {
				this.Remove(docid);
			}
		}

		public int GetRandom ()
		{
			if (this.DOCS.Count == 0) {
				throw new KeyNotFoundException("GetRandom cannot select an item from an empty set");
			}
			var docid = this.rand.Next (0, this.DB.Count);
			var node = this.DOCS.FindNode (docid, null);
			if (node == this.DOCS.LAST) {
				return this.DOCS.GetLast();
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
			var ctx = new SkipList2AdaptiveContext<int>(true, this.DOCS.FIRST);
			foreach (var s in sample) {
				this.DOCS.Add(s, ctx);
			}
		}

		/// <summary>
		/// Search by range
		/// </summary>
		public override IResult SearchRange (object q, double radius)
		{
			var r = new Result (this.DOCS.Count);
			foreach (var docid in this.DOCS.Traverse()) {
				double d = this.DB.Dist (q, this.DB[docid]);
				if (d <= radius) {
					r.Push (docid, d);
				}
			}
			return r;
		}
		
		/// <summary>
		/// KNN Search
		/// </summary>
		public override IResult SearchKNN (object q, int k, IResult R)
		{
			foreach (var docid in this.DOCS.Traverse()) {
				double d = this.DB.Dist (q, this.DB[docid]);
				R.Push (docid, d);
			}
			return R;
		}

	}
}
