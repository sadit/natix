//
//  Copyright 2014  Eric S. Téllez <eric.tellez@infotec.com.mx>
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

using natix;

namespace natix.CompactDS
{
	public class Plain64PairInvertedIndex : ILoadSave
	{
		public class Node : ILoadSave
		{
			public double maxdistance;
			public List<long> items;
			public List<double> distances;

			public double UpperBound(double dist)
			{
				// In this implementation lower and upper are the same
				return dist; 
			}

			public Node ()
			{
				this.maxdistance = 0;
				this.items = new List<long> ();
				this.distances = new List<double> ();
			}

			public void Add(long item, double dist)
			{
				this.items.Add (item);
				this.distances.Add (dist);

				if (this.maxdistance < dist) {
					this.maxdistance = dist;
				}
			}

			public void Add(IEnumerable<long> items, IEnumerable<double> dists)
			{
				this.items.AddRange (items);
				this.distances.AddRange (dists);

				foreach (var d in this.distances) {
					if (this.maxdistance < d) {
						this.maxdistance = d;
					}
				}
			}

			public int Count {
				get {
					return this.items.Count;
				}
			}

			public void Save(BinaryWriter Output)
			{
				Output.Write (this.maxdistance);
				Output.Write (this.items.Count);
				PrimitiveIO<long>.SaveVector (Output, this.items);
				PrimitiveIO<double>.SaveVector (Output, this.distances);
			}

			public void Load(BinaryReader Input)
			{
				this.maxdistance = Input.ReadDouble ();
				var m = Input.ReadInt32 ();
				PrimitiveIO<long>.LoadVector (Input, m, this.items);
				PrimitiveIO<double>.LoadVector (Input, m, this.distances);
			}
		}

		List< Node > postinglist;
		int numberOfItems;

		public Plain64PairInvertedIndex ()
		{
			this.postinglist = new List<Node> ();
			this.numberOfItems = 0;
		}

		public void Load (BinaryReader Input)
		{
			this.numberOfItems = Input.ReadInt32 ();
			var sigma = Input.ReadInt32 ();
			CompositeIO<Node>.LoadVector (Input, sigma, this.postinglist);
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.numberOfItems);
			Output.Write (this.postinglist.Count);
			CompositeIO<Node>.SaveVector (Output, this.postinglist);
		}

		public void Initialize(int sigma)
		{
			while (this.postinglist.Count < sigma) {
				this.postinglist.Add (null);
			}
		}

		public void AddItem (int symbol, long item, double distance)
		{
			var node = this.postinglist [symbol];
			if (node == null) {
				node = this.postinglist [symbol] = new Node ();
			}
			node.Add (item, distance);
			++this.numberOfItems;
		}

		public void Trim (int size)
		{
			while (size < this.postinglist.Count) {
				var last = this.postinglist.Count - 1;
				var t = this.postinglist [last];
				if (t != null && t.Count > 0) {
					this.numberOfItems -= t.Count;
				}
				this.postinglist.RemoveAt(last);
			}
		}

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		public int Add(IEnumerable<long> sortedlist, IEnumerable<double> distances)
		{
			var node = new Node ();
			node.Add (sortedlist, distances);
			this.postinglist.Add (node);
			this.numberOfItems += node.Count;
			return this.postinglist.Count - 1;
		}

		static Node EMPTY = new Node();

		public Node this[int symbol] {
			get {
				if (this.postinglist [symbol] == null) {
					return EMPTY;
				}
				return this.postinglist [symbol];
			}
		}

		/// <summary>
		/// Returns the number of posting lists in the inverted index
		/// </summary>
		/// <value>The count.</value>
		public int Count {
			get {
				return this.postinglist.Count;
			}
		}

		/// <summary>
		/// Gets the number of items in the inverted index
		/// </summary>
		/// <value>The number of items.</value>
		public int NumberOfItems {
			get {
				return this.numberOfItems;
			}
		}

		public int PopCount (int symbol)
		{
			if (this.postinglist [symbol] == null) {
				return 0;
			}
			return this.postinglist [symbol].Count;
		}
	}
}

