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
	public class SequenceInvertedIndex : InvertedIndex
	{
		Sequence seq;

		public SequenceInvertedIndex()
		{
			this.seq = null;
		}

		public static SequenceInvertedIndex Build(InvertedIndex invindex, SequenceBuilder builder)
		{
			var newinvindex = new SequenceInvertedIndex ();

			var xseq = new int[invindex.NumberOfItems];
			for (int sym = 0; sym < invindex.Count; ++sym) {
				var list = invindex [sym];
				foreach (var objID in list) {
					xseq [objID] = sym;
				}
			}
			newinvindex.seq = builder.Invoke (xseq, invindex.Count);
			return newinvindex;
		}

		public void Load (BinaryReader Input)
		{
			this.seq = GenericIO<Sequence>.Load (Input);
		}

		public void Save (BinaryWriter Output)
		{
			GenericIO<Sequence>.Save (Output, this.seq);
		}

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		public int Add(IEnumerable<long> sortedlist)
		{
			throw new NotSupportedException ();
		}

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		public int Add(IEnumerable<int> sortedlist)
		{
			throw new NotSupportedException ();
		}

		public void Decompress(List<long> list, int symbol)
		{
			var unravel = this.seq.Unravel (symbol);
			var m = unravel.Count1;
			if (list.Capacity < m) {
				list.Capacity = m;
			}
			for (int i = 1; i <= m; ++i) {
				list.Add (unravel.Select1 (i));
			}
		}

		public List<long> this[int symbol] {
			get {
				var list = new List<long> ();
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
				return this.seq.Sigma;
			}
		}

		/// <summary>
		/// Gets the number of items in the inverted index
		/// </summary>
		/// <value>The number of items.</value>
		public int NumberOfItems {
			get {
				return this.seq.Count;
			}
		}

		public int PopCount (int symbol)
		{
			return this.seq.Unravel(symbol).Count1;
		}
	}
}

