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
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SortingSearching
{
	/// <summary>
	/// The result set
	/// </summary>
	public class TopK<T>
	{
		double dmax;
		/// <summary>
		/// The maximum number of items to be stored
		/// </summary>
		int kmax;	
		public SkipList2<KeyValuePair<double,T>> Items;
		// this must be disabled until handle all removing exceptions (checking that the current
		// node in the AdaptiveContext is not removed)
		SkipList2<KeyValuePair<double,T>>.AdaptiveContext AdaptiveContext = null;
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
				return this.Items.Count;
			}
		}
	
		/// <summary>
		/// Returns true if the result set contains the docid with the given distance
		/// </summary>
		public bool Contains (double key, T u)
		{
			return this.Items.Contains (new KeyValuePair<double,T> (key, u));
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public TopK (int k)
		{
			this.kmax = k;
			this.dmax = double.MaxValue;
			this.Items = new SkipList2<KeyValuePair<double,T>> (0.5,
			                                                    (x,y) => x.Key.CompareTo (y.Key));
		}

		/// <summary>
		/// The radius of the knn item (or double.MaxValue if we have less items than k)
		/// </summary>
		
		public double Covering {
			get {
				return this.dmax;
			}
		}

		/// <summary>
		///  Push a docid and a distance to the result set
		/// </summary>
		public void Push (double key, T u)
		{
			var item = new KeyValuePair<double, T> (key, u);
			if (key < this.dmax || this.Items.Count < this.kmax) {
				this.Items.Add (item, this.AdaptiveContext);
			}
			if (this.Items.Count > this.kmax) {
				this.Items.RemoveLast();
			}
		}
	}
}
