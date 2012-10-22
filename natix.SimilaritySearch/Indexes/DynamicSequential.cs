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
		public Random rand;
		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicSequential ()
		{
			this.rand = new Random();
		}

		public DynamicSequential (int random_seed)
		{
			this.rand = new Random(random_seed);
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

		public void Remove (IResult res)
		{	
			foreach (var p in res) {
				this.Remove(p.docid);
			}
		}

		public int GetRandom ()
		{
			if (this.DOCS.Count == 0) {
				throw new KeyNotFoundException ("GetRandom cannot select an item from an empty set");
			}
			var docid = this.rand.Next (0, this.DB.Count);
			var node = this.DOCS.FindNode (docid, null);
			//Console.WriteLine ("RANDOM {0}, FIRST: {1}, LAST: {2}", docid, this.DOCS.GetFirst(), this.DOCS.GetLast());
			if (node == this.DOCS.LAST) {
				return this.DOCS.GetLast ();
			}
			if (node == this.DOCS.FIRST) {
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

		public void SearchExtremes (object q, IResult near, IResult far)
		{
			foreach (var docid in this.DOCS.Traverse()) {
				double d = this.DB.Dist (q, this.DB[docid]);
				if (!near.Push (docid, d)) {
					far.Push (docid, -d);
				}
			}
		}

		public void SearchExtremesKNN (object q, IResult near, IResult far, out double mean, out double stddev)
		{
			var L = new double[ this.DOCS.Count ];
			mean = 0;
			int i = 0;
			foreach (var docid in this.DOCS.Traverse()) {
				double d = this.DB.Dist (q, this.DB[docid]);
				L[i] = d;
				mean += d;
				++i;
			}
			mean /= i;
			stddev = 0;
			i = 0;
			foreach (var docid in this.DOCS.Traverse()) {
				double m = L[i] - mean;
				stddev += m * m;
				if (!near.Push (docid, L[i])) {
					far.Push (docid, -L[i]);
				}
				++i;
			}
			stddev = Math.Sqrt(stddev / L.Length);
		}

		public void SearchExtremesRange (object q, double alpha_stddev, int min_bs, out IResult near, out IResult far, out double mean, out double stddev)
		{
			var L = new double[ this.DOCS.Count ];
			mean = 0;
			int i = 0;
			var fixed_near = new Result(min_bs, false);
			var fixed_far = new Result(min_bs, false);
			double min = double.MaxValue;
			double max = double.MinValue;
			foreach (var docid in this.DOCS.Traverse()) {
				double d = this.DB.Dist (q, this.DB[docid]);
				L[i] = d;
				mean += d;
				++i;
				min = Math.Min(min, d);
				max = Math.Max(max, d);
			}
			mean /= L.Length;
			stddev = 0;
			i = 0;
			foreach (var docid in this.DOCS.Traverse()) {
				var d = L[i];
				double m = d - mean;
				stddev += m * m;
				++i;
			}
			stddev = Math.Sqrt(stddev / L.Length);
			var radius = stddev * alpha_stddev;
			near = new Result(L.Length, false);
			far = new Result(L.Length, false);
			i = 0;
			foreach (var docid in this.DOCS.Traverse()) {
				var d = L[i];
				if (d <= min + radius) {
					near.Push(docid, d);
				} else if (d >= max - radius) {
					far.Push (docid, -d);
				}
				if (!fixed_near.Push (docid, d)) {
					fixed_far.Push (docid, -d);
				}
				++i;
			}
			if (near.Count < fixed_near.Count && far.Count < fixed_far.Count) {
				near = fixed_near;
				far = fixed_far;
			}
		}
	}
}
