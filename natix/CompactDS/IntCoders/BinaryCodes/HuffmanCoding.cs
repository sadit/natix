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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/HuffmanCoding.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace natix.CompactDS
{
	public class HuffmanCoding : IIEncoder32
	{
		
		public HuffmanTree Huffman;
		
		public HuffmanCoding ()
		{
		}
		
		public HuffmanCoding (HuffmanTree huffmantree)
		{
			this.Huffman = huffmantree;
		}
		
		public int Decode (IBitStream stream, BitStreamCtx ctx)
		{
			HuffmanInner node = this.Huffman.Root;
			while (true) {
				bool b = stream.Read (ctx);
				HuffmanNode next;
				if (b) {
					next = node.Right;
				} else {
					next = node.Left;
				}
				var leaf = next as HuffmanLeaf;
				if (leaf != null) {
					return leaf.Symbol;
				}
				node = next as HuffmanInner;
			}
		}
		
		public void Encode (IBitStream stream, int u)
		{
			var size = this.Huffman.Encode (stream, u);
			if (size == 0) {
				throw new KeyNotFoundException ("HuffmanCoding.Encode unknown symbol:" + u.ToString());
			}
		}
		
		public void Save (BinaryWriter Output)
		{
			this.Huffman.Save (Output);
		}
		
		public void Load (BinaryReader Input)
		{
			this.Huffman = new HuffmanTree ();
			this.Huffman.Load (Input);
		}
	}
}
