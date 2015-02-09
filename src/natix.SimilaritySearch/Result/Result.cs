//
//   Copyright 2012-2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
		SkipList2<ItemPair> pairset;
		// this must be disabled until handle all removing exceptions (checking that the current
		// node in the AdaptiveContext is not removed)
		SkipList2<ItemPair>.AdaptiveContext AdaptiveContext = null;

		/// <summary>
		/// Gets the number of objects of this result.
		/// </summary>
		public int K { get; set; }
		/// <summary>
		///  The current items in the result set
		/// </summary>
		public int Count {
			get {
				return this.pairset.Count;
			}
		}
		/// <summary>
		///  Returns the First (closer) result in the set
		/// </summary>
		public ItemPair First
		{
			get {
				return pairset.GetFirst ();
			}
		}
		/// <summary>
		///  Returns the Last (farthest) result in the set
		/// </summary>
		public ItemPair Last
		{
			get {
				return pairset.GetLast ();
			}
		}
		/// <summary>
		/// Pop First (closest) item
		/// </summary>
		/// <returns>
		/// A <see cref="ResultPair"/>
		/// </returns>
		public ItemPair PopFirst ()
		{
			var first = this.pairset.RemoveFirst ();
			return first;
		}
		/// <summary>
		/// Pop Last (farthest) item
		/// </summary>
		public ItemPair PopLast ()
		{
			var last = this.pairset.RemoveLast ();
			return last;
		}
	
		/// <summary>
		/// Remove the register with the given docid and distance
		/// </summary>
		public bool Remove (int docid, double d)
		{
			try {
				this.pairset.Remove (new ItemPair(docid, d), this.AdaptiveContext);
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
			return this.pairset.Contains (new ItemPair(docid, d));
		}

		/// <summary>
		///  checks for equallity of two result's set
		/// </summary>
		public bool Equals (IResult obj)
		{
			if (obj.Count != this.Count) {
				return false;
			}
			var a = new List<ItemPair> (this);
			var b = new List<ItemPair> (obj);
			for (int i = 0; i < a.Count; ++i) {
				if (a [i].CompareTo (b [i]) != 0) {
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		public Result (int k)
		{
			this.K = k;
			this.pairset = new SkipList2<ItemPair> (0.5, (ItemPair x, ItemPair y) => x.CompareTo (y));
		}

		/// <summary>
		/// The radius of the knn item (or double.MaxValue if we have less items than k)
		/// </summary>
		
		public virtual double CoveringRadius {
			get {
				if (this.Count < this.K) {
					return Double.MaxValue;
				} else {
					return this.Last.Dist;
				}
			}
		}

		/// <summary>
		/// Enumerator
		/// </summary>
		public IEnumerator<ItemPair> GetEnumerator ()
		{
			return this.pairset.Traverse ().GetEnumerator ();
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
			if (d >= covering) {
				return false;
			}
			if (this.Count == this.K) {
				this.PopLast ();
			}
			this.pairset.Add (new ItemPair (docid, d), this.AdaptiveContext);
			return true;
		}
	}
}
