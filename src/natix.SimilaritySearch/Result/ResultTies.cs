//
//   Copyright 2012-2014 Eric Sadit Tellez <eric.tellez@infotec.com.mx>
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
//   Original filename: natix/SimilaritySearch/Result/ResultTies.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class ResultTies : IResult
	{
		public SortedList<double, List<int> > pool;
		int counter;
		
		public IEnumerator<ItemPair> GetEnumerator ()
		{
			for (int i = 0, numkeys = this.pool.Keys.Count; i < numkeys; i++) {
				var key = this.pool.Keys[i];
				foreach (int docid in this.pool.Values[i]) {
					yield return new ItemPair {ObjID = docid, Dist = key};
				}
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((ResultTies)this).GetEnumerator ();
		}
		
		public int K { get; set; }

		public int Count {
			get {
				return counter;
			}
		}
		
		public ResultTies (int k)
		{
			this.K = k;
			this.pool = new SortedList<double, List<int>> ();
		}		

		public bool Push (int docid, double dist)
		{
			double covering = this.CoveringRadius;
			if (covering <= dist) {
				return false;
			}
	
			List<int> list;
			if (!this.pool.TryGetValue (dist, out list)) {
				list = new List<int> ();
				this.pool.Add (dist, list);
			}
			this.counter++;
			if (this.counter > this.K) {
				this.PopLast ();
			}
			list.Add (docid);
			return true;
		}
		
		public List<int> GetFirstList (out double dist)
		{
			dist = this.pool.Keys[0];
			return this.pool.Values[0];
		}
		
		public List<int> GetLastList (out double dist)
		{
			var pos = this.pool.Keys.Count - 1;
			dist = this.pool.Keys[pos];
			return this.pool.Values[pos];
		}

		public ItemPair First {
			get {
				double dist;
				var list = this.GetFirstList (out dist);
				return new ItemPair {ObjID = list[0], Dist = dist};
			}
		}
		
		public ItemPair Last {
			get {
				double dist;
				var list = this.GetLastList (out dist);
				return new ItemPair {ObjID = list[0], Dist = dist};
			}
		}

		public double LastKey {
			get {
				return this.pool.Keys[this.pool.Keys.Count - 1];
			}
		}

		public void PopLast ()
		{
			var lastpos = this.pool.Keys.Count - 1;
			var lastlist = this.pool.Values [lastpos];
			if (lastlist.Count > 1) {
				lastlist.RemoveAt (lastlist.Count - 1);
			} else {
				this.pool.RemoveAt (lastpos);
			}
			this.counter--;
		}
		
		public double CoveringRadius {
			get {
				if (this.counter < this.K) {
					return double.MaxValue;
				}
				return this.pool.Keys[this.pool.Keys.Count - 1];
			}
		}
	}
}