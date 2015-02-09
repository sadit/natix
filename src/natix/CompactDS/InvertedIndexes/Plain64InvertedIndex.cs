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
	public class Plain64InvertedIndex : InvertedIndex
	{
		List< List<long> > table;
		int numberOfItems;

		public Plain64InvertedIndex ()
		{
			this.table = new List<List<long>> ();
			this.numberOfItems = 0;
		}

		public static Plain64InvertedIndex Build(InvertedIndex invindex)
		{
			var I = new Plain64InvertedIndex ();
			for (int i = 0; i < invindex.Count; ++i) {
				I.Add (invindex[i]);
			}
			return I;
		}

		public void Load (BinaryReader Input)
		{
			this.numberOfItems = Input.ReadInt32 ();
			var sigma = Input.ReadInt32 ();
			for (int i = 0; i < sigma; ++i) {
				var m = Input.ReadInt32 ();
				var l = new List<long> (m);
				this.table.Add (l);
				PrimitiveIO<long>.LoadVector (Input, m, l);
			}
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.numberOfItems);
			Output.Write (this.table.Count);
			for (int i = 0; i < this.Count; ++i) {
				Output.Write (this[i].Count);
				PrimitiveIO<long>.SaveVector (Output, this[i]);
			}
		}

		public void Initialize(int sigma)
		{
			while (this.table.Count < sigma) {
				this.table.Add (null);
			}
		}

		public void AddItem (int symbol, long item)
		{
			try {
				if (this.table [symbol] == null) {
					this.table [symbol] = new List<long> ();
				}
				this.table [symbol].Add (item);
				++this.numberOfItems;
			} catch (Exception e){
				Console.WriteLine ("---- symbol: {0}, size: {1}", symbol, this.table.Count);
				throw e;
			}
		}

		public void Trim (int size)
		{
			while (size < this.table.Count) {
				var last = this.table.Count - 1;
				var t = this.table [last];
				if (t != null && t.Count > 0) {
					this.numberOfItems -= t.Count;
				}
				this.table.RemoveAt(last);
			}
		}

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		public int Add(IEnumerable<long> sortedlist)
		{
			var list = new List<long> (sortedlist);
			this.table.Add (list);
			this.numberOfItems += list.Count;
			return this.table.Count - 1;
		}

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		public int Add(IEnumerable<int> sortedlist)
		{
			var list = new List<long> ();
			foreach (var u in sortedlist) {
				list.Add (u);
			}
			this.table.Add (list);
			this.numberOfItems += list.Count;
			return this.table.Count - 1;
		}

		static List<long> EMPTY = new List<long>();

		public List<long> this[int symbol] {
			get {
				if (this.table [symbol] == null) {
					return EMPTY;
				}
				return this.table [symbol];
			}
		}

		/// <summary>
		/// Returns the number of posting lists in the inverted index
		/// </summary>
		/// <value>The count.</value>
		public int Count {
			get {
				return this.table.Count;
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
			return this.table [symbol].Count;
		}
	}
}

