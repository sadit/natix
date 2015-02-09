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
	public interface InvertedIndex : ILoadSave
	{
		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		int Add (IEnumerable<long> sortedlist);

		/// <summary>
		/// Adds a posting list to the index. Returns the corresponding symbol
		/// </summary>
		/// <param name="sortedlist">Sortedlist.</param>
		int Add (IEnumerable<int> sortedlist);

		List<long> this [int symbol] {
			get;
		}

		/// <summary>
		/// Returns the number of posting lists in the inverted index
		/// </summary>
		/// <value>The count.</value>
		int Count {
			get;
		}

		/// <summary>
		/// Gets the number of items in the inverted index
		/// </summary>
		/// <value>The number of items.</value>
		int NumberOfItems {
			get;
		}

		int PopCount (int symbol);
	}
}

