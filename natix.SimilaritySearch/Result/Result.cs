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
//   Original filename: natix/SimilaritySearch/Result/Result.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The result set
	/// </summary>
	public class Result : IResult
	{
		bool ceilingKNN;
		double dmax;
		/// <summary>
		/// The maximum number of items to be stored
		/// </summary>
		public int kmax;
		
		static double MaxValue = double.MaxValue / 2;
		SkipList2<ResultPair> pairset;
		List<ResultPair> overflow;
		// this must be disabled until handle all removing exceptions (checking that the current
		// node in the AdaptiveContext is not removed)
		SkipList2AdaptiveContext<ResultPair> AdaptiveContext = null;
		/// <summary>
		/// True if this result is performing ceiling KNN
		/// </summary>
		public bool Ceiling {
			get { return this.ceilingKNN; }
		}
		/// <summary>
		/// Gets the number of objects of this result.
		/// </summary>
		public int K {
			get { return this.kmax; }
		}
		/// <summary>
		///  The current items in the result set
		/// </summary>
		public int Count {
			get {
				if (this.ceilingKNN) {
					return this.pairset.Count + this.overflow.Count;
				} else {
					return this.pairset.Count;
				}
			}
		}
		/// <summary>
		///  Returns the First (closer) result in the set
		/// </summary>
		public ResultPair First
		{
			get { return pairset.GetFirst (); }
		}
		/// <summary>
		///  Returns the Last (farthest) result in the set
		/// </summary>
		public ResultPair Last
		{
			get { return pairset.GetLast (); }
		}
		/// <summary>
		/// Pop First (closest) item
		/// </summary>
		/// <returns>
		/// A <see cref="ResultPair"/>
		/// </returns>
		public ResultPair PopFirst ()
		{
			var first = this.pairset.RemoveFirst ();
			return first;
		}
		/// <summary>
		/// Pop Last (farthest) item
		/// </summary>
		/// <returns>
		/// A <see cref="ResultPair"/>
		/// </returns>
		public ResultPair PopLast ()
		{
			var last = this.pairset.RemoveLast ();
			return last;
		}
	
		/// <summary>
		/// Remove the register with the given docid and distance
		/// </summary>
		/// <param name="docid">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="d">
		/// A <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Remove (int docid, double d)
		{
			try {
				this.pairset.Remove (new ResultPair (docid, d), this.AdaptiveContext);
			} catch (KeyNotFoundException) {
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// Returns true if the result set contains the docid with the given distance
		/// </summary>
		public bool Contains (int docid, double d)
		{
			return this.pairset.Contains (new ResultPair (docid, d));
		}

		/// <summary>
		///  checks for equallity of two result's set
		/// </summary>
		public bool Equals (IResult obj)
		{
			if (obj.Count != this.Count) {
				return false;
			}
			IEnumerator<ResultPair> it = obj.GetEnumerator ();
			it.MoveNext ();
			foreach (ResultPair p in this) {
				if (p.CompareTo (it.Current) != 0) {
					return false;
				}
				it.MoveNext ();
			}
			return true;
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="k">
		/// The number of nearest neighbors to search (use int.MaxValue or space.Length for range queries)
		/// </param>
		/// <param name="ceiling">
		/// Kept results items with radius equalt to the kth NN
		/// </param>
		public Result (int k, bool ceiling)
		{
			this.kmax = k;
			this.ceilingKNN = ceiling;
			if (this.ceilingKNN) {
				this.overflow = new List<ResultPair> ();
			}
			this.dmax = MaxValue;
			//this.Pool = new SortedDictionary<ResultPair, int>();
			this.pairset = new SkipList2<ResultPair> (0.5, (ResultPair x, ResultPair y) => x.CompareTo (y));
		}
		/// <summary>
		/// Constructor
		/// </summary>
		
		public Result (int k) : this(k, false)
		{
		}
		
		/// <summary>
		/// A String representing the result set
		/// </summary>
		public override string ToString ()
		{
			string s = "";
			foreach (ResultPair r in this) {
				s += r.ToString () + ", ";
			}
			s += "<TheEnd>";
			return s;
		}
		/// <summary>
		/// The radius of the knn item (or double.MaxValue if we have less items than k)
		/// </summary>
		
		public double CoveringRadius {
			get {
				if (this.Count < this.kmax) {
					return MaxValue;
				} else {
					return this.dmax;
				}
			}
		}
		double InnerRadius ()
		{
			return this.pairset.GetLast ().dist;
		}
		
		/// <summary>
		/// Enumerator
		/// </summary>
		/// <returns>
		/// </returns>
		public IEnumerator<ResultPair> GetEnumerator ()
		{
			if (this.ceilingKNN) {
				var L = new List<ResultPair> ();
				foreach (var m in this.pairset.Traverse()) {
					L.Add (m);
				}
				foreach (var m in this.overflow) {
					L.Add (m);
				}
				return L.GetEnumerator ();
			} else {
				return this.pairset.Traverse ().GetEnumerator ();
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator) ((Result) this).GetEnumerator(); 
		}
		/// <summary>
		///  Push a docid and a distance to the result set
		/// </summary>
		public bool Push (int docid, double d)
		{
			double covering = this.CoveringRadius;
			if (d == covering) {
				if (this.ceilingKNN) {
					this.overflow.Add (new ResultPair (docid, d));
					return true;
				}
				return false;
			}
			if (d > covering) {
				return false;
			}
			ResultPair r = new ResultPair (docid, d);
			bool removedLast = false;
			// we need a default value
			ResultPair last = new ResultPair (0, 0);
			//Console.WriteLine ("=== Inserting: {0}, Pool Length: {1}, kmax: {2}, ", r, pairset.Count, this.kmax);
			if (this.pairset.Count >= this.kmax) {
				removedLast = true;
				last = this.pairset.RemoveLast ();
			}
			this.pairset.Add (r, this.AdaptiveContext);
			if (removedLast || this.pairset.Count == this.kmax) {
				this.dmax = this.InnerRadius ();
				if (this.ceilingKNN) {
					if (last.dist == this.dmax) {
						if (this.overflow.Count == 0 || this.overflow [0].dist == last.dist) {
							this.overflow.Add (last);
						} else {
							this.overflow.Clear ();
						}
					} else {
						this.overflow.Clear ();
					}
				}
			}
			return true;
		}
	}
	
	/// <summary>
	/// Filter a Result creating a new Result object
	/// </summary>
	// public delegate IResult ResultFilter<T> (Index<T> index, T query, object internalquery, IResult r);

}
