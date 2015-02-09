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
	public class CompressedInvertedIndex : InvertedIndex
	{
		LongStream lstream;
		List<int> offsets;
		List<int> lengths;
		int numberOfItems;

		public CompressedInvertedIndex ()
		{
			this.lstream = new LongStream();
			this.offsets = new List<int> ();
			this.lengths = new List<int> ();
			this.numberOfItems = 0;
		}

		public static CompressedInvertedIndex Build (InvertedIndex invindex)
		{
			var cii = new CompressedInvertedIndex ();
			for (int s = 0; s < invindex.Count; ++s) {
				cii.Add (invindex [s]);
			}
			return cii;
		}

		public void Load (BinaryReader Input)
		{
			this.numberOfItems = Input.ReadInt32 ();
			var sigma = Input.ReadInt32 ();
			PrimitiveIO<int>.LoadVector (Input, sigma, this.offsets);
			PrimitiveIO<int>.LoadVector (Input, sigma, this.lengths);
			this.lstream.Load (Input);
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.numberOfItems);
			Output.Write (this.offsets.Count);
			PrimitiveIO<int>.SaveVector (Output, this.offsets);
			PrimitiveIO<int>.SaveVector (Output, this.lengths);
			this.lstream.Save (Output);
		}

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		public int Add(IEnumerable<long> sortedlist)
		{
			long prev = -1;
			var pos = 0;
			var count = 0;
			foreach (var item in sortedlist) {
				if (prev == -1) {
					pos = this.lstream.Add (item);
				} else {
					this.lstream.Add (item - prev);
				}
				prev = item;
				++count;
			}
			this.offsets.Add (pos);
			this.lengths.Add (count);
			this.numberOfItems += count;
			return this.offsets.Count - 1;
		}

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		public int Add(IEnumerable<int> sortedlist)
		{
			long prev = -1;
			var pos = 0;
			var count = 0;

			foreach (var item in sortedlist) {
				if (prev == -1) {
					pos = this.lstream.Add (item);
				} else {
					this.lstream.Add (item - prev);
				}
				prev = item;
				++count;
			}

			this.offsets.Add (pos);
			this.lengths.Add (count);
			this.numberOfItems += count;
			return this.offsets.Count - 1;
		}

		public void Decompress(List<long> list, int symbol)
		{
			var m = this.lengths [symbol];
			var ctx = new OctetStream.Context (this.offsets [symbol]);
			this.lstream.Decompress (list, ctx, m);

			for (int i = 1; i < list.Count; ++i) {
				list [i] += list [i - 1];
			}
		}

		public List<long> this[int symbol] {
			get {
				var list = new List<long> (this.lengths[symbol]);
				this.Decompress(list, symbol);
				return list;
			}
		}

		/// <summary>
		/// Returns the number of posting lists in the inverted index
		/// </summary>
		/// <value>The count.</value>
		public int Count {
			get {
				return this.offsets.Count;
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
			return this.lengths [symbol];
		}
	}
}

