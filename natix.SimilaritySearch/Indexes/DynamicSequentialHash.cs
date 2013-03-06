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
//   Original filename: natix/SimilaritySearch/Indexes/DynamicSequentialHash.cs
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
	public class DynamicSequentialHash : BasicIndex
	{
		public HashSet<int> DOCS;
		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicSequentialHash ()
		{
		}

		public void Remove (int docid)
        {
            this.DOCS.Remove(docid);
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

		/// <summary>
		/// API build command
		/// </summary>
		public virtual void Build (MetricDB db, IList<int> sample = null)
		{
			this.DB = db;
			if (sample == null) {
				sample = RandomSets.GetExpandedRange (this.DB.Count);
			}
            this.DOCS = new HashSet<int>();
			foreach (var s in sample) {
				this.DOCS.Add(s);
			}
		}

		/// <summary>
		/// Search by range
		/// </summary>
		public override IResult SearchRange (object q, double radius)
		{
			var r = new Result (this.DOCS.Count);
			foreach (var docid in this.DOCS) {
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
			foreach (var docid in this.DOCS) {
				double d = this.DB.Dist (q, this.DB[docid]);
				R.Push (docid, d);
			}
			return R;
		}

		public void SearchExtremes (object q, IResult near, IResult far)
		{
			foreach (var docid in this.DOCS) {
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
			foreach (var docid in this.DOCS) {
				double d = this.DB.Dist (q, this.DB[docid]);
				L[i] = d;
				mean += d;
				++i;
			}
			mean /= i;
			stddev = 0;
			i = 0;
			foreach (var docid in this.DOCS) {
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
			var fixed_near = new Result (min_bs, false);
			var fixed_far = new Result (min_bs, false);
			double min = double.MaxValue;
			double max = double.MinValue;
			foreach (var docid in this.DOCS) {
				double d = this.DB.Dist (q, this.DB [docid]);
				L [i] = d;
				mean += d;
				++i;
				min = Math.Min (min, d);
				max = Math.Max (max, d);
			}
			mean /= L.Length;
			stddev = 0;
			i = 0;
			foreach (var docid in this.DOCS) {
				var d = L [i];
				double m = d - mean;
				stddev += m * m;
				++i;
			}
			stddev = Math.Sqrt (stddev / L.Length);
			var __alpha_stddev = alpha_stddev;
			if (alpha_stddev < 0) {
				// this is a value describing the frontier between discarding and not discarding
				// using the given pivot but using a query following the same distribution
				__alpha_stddev = (max - 3 * min) / (2 * stddev);
				if (__alpha_stddev < 0) {
					// it cannot be smaller than 0
					__alpha_stddev = 0;
				} else {
					// alpha_stddev has the negative value of the scaling of the dynamically
					// computed __alpha_stddev
					__alpha_stddev = Math.Abs( alpha_stddev * __alpha_stddev );
				}
			}
			var radius = stddev * __alpha_stddev;
			// Console.WriteLine ("read alpha_stddev: {0}, stddev: {1}, radius: {2}, min: {3}, max: {4}, raw alpha: {5}, n: {6}", __alpha_stddev, stddev, radius, min, max, (max-3*min)/stddev, this.DOCS.Count);
			near = new Result(L.Length, false);
			far = new Result(L.Length, false);
			i = 0;
			foreach (var docid in this.DOCS) {
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
