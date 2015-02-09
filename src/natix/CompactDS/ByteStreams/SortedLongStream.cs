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

namespace natix.CompactDS
{
	public class SortedLongStream : ILoadSave
	{
		LongStream lstream;

		public SortedLongStream ()
		{
			this.lstream = new LongStream();
		}

		public void Load (BinaryReader Input)
		{
			this.lstream.Load (Input);
		}

		public void Save (BinaryWriter Output)
		{
			this.lstream.Save (Output);
		}

		public void Add(IEnumerable<long> sortedlist)
		{
			long prev = -1;
			foreach (var item in sortedlist) {
				if (prev == -1) {
					this.lstream.Add (item);
				} else {
					this.lstream.Add (item - prev);
				}
				prev = item;
			}
		}

		public void Add(IEnumerable<int> sortedlist)
		{
			int prev = -1;
			foreach (var item in sortedlist) {
				if (prev == -1) {
					this.lstream.Add (item);
				} else {
					this.lstream.Add (item - prev);
				}
				prev = item;
			}
		}

		public List<long> Decompress()
		{
			var list = this.lstream.Decompress (0, this.lstream.Count);

			for (int i = 1; i < list.Count; ++i) {
				list [i] += list [i - 1];
			}

			return list;
		}

	}
}

