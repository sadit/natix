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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListIWT.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// list of integers reconstructing splitting each one in 4 bytes (each byte can be compressed if the right coder is used)
	/// </summary>
	public class ListIWT : ListGenerator<int>, ILoadSave
	{
		public WaveletTree WT;
		
		public void Save (BinaryWriter Output)
		{
			this.WT.Save (Output);
		}
		
		public void Load (BinaryReader Input)
		{
			this.WT = new WaveletTree ();
			this.WT.Load (Input);
		}

		public static ListIWT ListWithHuffman (IList<int> data, int numbits)
		{
			var max = 1 << numbits;
			var model = new int[max];
			var mask = max - 1;
			// var L = new ListGen<uint> ((int i) => (uint)data[i], data.Count);
			// Console.WriteLine ("XXXXXXX L: {0}, A: {1}", L.Count, A.Count);
			for (int i = 0; i < data.Count; i++) {
				var u = data[i] & mask;
				model[u]++;
			}
			var huffmantree = new HuffmanTree ();
			huffmantree.Build (model);
			var huffmancoder = new HuffmanCoding (huffmantree);
			var wt = new WaveletTree ();
			wt.Build (huffmancoder, max, data);
			return new ListIWT(wt);
		}
		
		
		public ListIWT (WaveletTree wt) : base()
		{
			this.WT = wt;
		}

		
		public override int Count {
			get {
				return this.WT.Count;
			}
		}

		public override void Add (int item)
		{
			throw new NotImplementedException ();
		}
		
		public override int GetItem (int index)
		{
			return this.WT.Access (index);
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}
	}
}
