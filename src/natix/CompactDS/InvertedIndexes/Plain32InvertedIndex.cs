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
	public class Plain32InvertedIndex : InvertedIndex
	{
		List< List<int> > table;
		int numberOfItems;

		public Plain32InvertedIndex ()
		{
			this.table = new List<List<int>> ();
			this.numberOfItems = 0;
		}

		public static Plain32InvertedIndex Build(InvertedIndex invindex)
		{
			var I = new Plain32InvertedIndex ();
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
				var l = new List<int> (m);
				this.table.Add (l);
				PrimitiveIO<int>.LoadVector (Input, m, l);
			}
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.numberOfItems);
			Output.Write (this.table.Count);
			for (int i = 0; i < this.Count; ++i) {
				if (this [i] == null) {
					Output.Write (0);
				} else {
					Output.Write (this [i].Count);
					PrimitiveIO<int>.SaveVector (Output, this.table [i]);
				}
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
			if (this.table [symbol] == null) {
				this.table [symbol] = new List<int> ();
			}
			this.table [symbol].Add ((int)item);
			++this.numberOfItems;
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
			var list = new List<int> ();
			foreach (var u in sortedlist) {
				list.Add ((int)u);
			}
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
			var list = new List<int> (sortedlist);
			this.table.Add (list);
			this.numberOfItems += list.Count;
			return this.table.Count - 1;
		}

		static List<long> EMPTY = new List<long>();

		public List<long> this[int symbol] {
			get {
				if (this.table[symbol] == null) {
					return EMPTY;
				}
				var list32 = this.table [symbol];
				var list64 = new List<long> (list32.Count);
				foreach (var u in list32) {
					list64.Add (u);
				}
				return list64;
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
