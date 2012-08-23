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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListIRRR.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	/// <summary>
	/// List of integers. It encodes each integer by bit popcount classes.
	/// </summary>
	public class ListIRRR : ListGenerator<uint>, ILoadSave
	{
		static IList<short> BinomialCoefficients;
		static int NumBits;
		static IList<byte> NumBitsPerClass;
		static IList<ushort> AccCounterPerClass;
		static IList<ushort> OffsetTable;

		public IList<int> Classes;
		public BitStream32 Offsets;
		public IList<int> StartOffset;
		public int count;
		BitStreamCtx ctx = new BitStreamCtx();
		
		/// <summary>
		/// Save the specified Output.
		/// </summary>
		public void Save (BinaryWriter Output)
		{
			ListIGenericIO.Save (Output, this.Classes);
			this.Offsets.Save (Output);
			ListIGenericIO.Save (Output, this.StartOffset);
			Output.Write (this.count);
		}
		
		/// <summary>
		/// Loads the list from a binary file
		/// </summary>
		public void Load (BinaryReader Input)
		{
			this.Classes = ListIGenericIO.Load (Input);
			this.Offsets = new BitStream32 ();
			this.Offsets.Load (Input);
			this.StartOffset = ListIGenericIO.Load (Input);
			this.count = Input.ReadInt32 ();
		}
			
		static byte GetClass (ushort b)
		{
			return (byte)(Bits.PopCount8[(byte)(b & 255)] +
				Bits.PopCount8[(byte)((b >> 8) & 255)]);
		}
				
		static ListIRRR ()
		{
			NumBits = 16;
			NumBitsPerClass = new byte[NumBits + 1];
			// OffsetTable = new ushort[1 << (NumBits - 1)];
			OffsetTable = new ushort[1 << NumBits];
			BinomialCoefficients = new short[] {
				1, 16,
				120, 560,
				1820, 4368,
				8008, 11440,
				12870, 11440,
				8008, 4368,
				1820, 560,
				120, 16,
				1
			};
			AccCounterPerClass = new ushort[NumBits + 1];
			var starting_positions = new ushort[NumBits + 1];
			ushort acc = 0;
			for (int i = 0; i < BinomialCoefficients.Count; i++) {
				var u = (byte)Math.Ceiling (Math.Log (BinomialCoefficients[i], 2));
				NumBitsPerClass[i] = u;
				AccCounterPerClass[i] = acc;
				starting_positions[i] = acc;
				acc += (ushort)BinomialCoefficients[i];
			}
			// now we can fill the table of offsets, just the first half. The second half
			// can be computed online using the reverse order and the complement class
			for (int i = 0; i < OffsetTable.Count; i++) {
				ushort ishort = (ushort)i;
				var klass = GetClass (ishort);
				// Console.WriteLine ("i: {0}, {1}, offset-table-size: {2}, klass: {3}, starting: {4}",
				// ishort, BinaryHammingSpace.ToAsciiString (ishort), OffsetTable.Count, klass, starting_positions[klass]);
				OffsetTable[starting_positions[klass]++] = ishort;
			}
		}
		
		int SamplingSizeOffset;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name='input'>
		/// The input list to be encoded
		/// </param>
		/// <param name='sampling_size_offset'>
		/// The gap between absolute rank values in the index
		/// </param>
		/// <param name='compress_with_huffman_wt'>
		/// If true, the classes will compressed with a wavelet tree with huffman  shape
		/// </param>
		public ListIRRR (IList<uint> input, int sampling_size_offset, bool compress_with_huffman_wt)
		{
			this.count = 0;
			this.Offsets = new BitStream32 ((input.Count >> 5) + 1);
			this.SamplingSizeOffset = sampling_size_offset;;
			var B = new BitStream32 ((input.Count * 5) >> 3);
			this.StartOffset = new List<int>(input.Count / this.SamplingSizeOffset);
			this.Classes = new ListIFS (5, B);
			for (int i = 0; i < input.Count; i++, this.count++) {
				this.EncodeItem(input[i]);
			}
			if (compress_with_huffman_wt) {
				this.Classes = ListIWT.ListWithHuffman(this.Classes, 5);
			}
		}

		void Seek(int start)
		{
			int start_offset_index = start / this.SamplingSizeOffset;
			/*Console.WriteLine("=== start: {0}, start_offset_index: {1}, start-offset-count: {2}",
				start, start_offset_index, this.StartOffset.Count);*/
			int offset_bitmap = this.StartOffset[ start_offset_index ];
			int klass;
			int I;
			for (int i = start_offset_index * this.SamplingSizeOffset; i < start; i++) {
				I = i << 1;
				klass = (int)this.Classes[ I ];
				offset_bitmap += NumBitsPerClass[ klass ];
				klass = (int)this.Classes[ I + 1 ];
				offset_bitmap += NumBitsPerClass[ klass ];
			}
			this.ctx.Seek(offset_bitmap);
		}
		
		// we always suppose several sequential GetItems,
		// other operations may not be efficient
		int last_index = int.MinValue;
		uint last_value = 0;
		public override uint GetItem (int index)
		{
			if (this.last_index == index) {
				return this.last_value;
			}
			if (this.last_index + 1 != index) {
				this.Seek (index);
			}
			this.last_index = index;
			this.last_value = this.DecodeNext (index << 1);
			return this.last_value;
		}
		
		/// <summary>
		/// Sets the item.
		/// </summary>
		public override void SetItem (int index, uint val)
		{
			throw new NotImplementedException ();
		}
		
		/// <summary>
		/// The number of items inside the list
		/// </summary>
		public override int Count {
			get {
				return this.count;
			}
		}
		
		void EncodeItem (uint u)
		{
			// if (this.StartOffset.Count > 0 && this.count % this.SamplingSizeOffset == 0) {
			if (this.count % this.SamplingSizeOffset == 0) {
				this.StartOffset.Add ((int)this.Offsets.CountBits);
			}
			var u1 = (ushort)(u & 0xffff);
			var u2 = (ushort)((u >> 16) & 0xffff);
			var class1 = GetClass (u1);
			var class2 = GetClass (u2);
			this.Classes.Add (class1);
			this.Classes.Add (class2);
			int numbits = NumBitsPerClass[class1];
			if (numbits > 0) {
				this.Offsets.Write (this.ComputeOffset16bit (u1, class1), numbits);
			}
			numbits = NumBitsPerClass[class2];
			if (numbits > 0) {
				this.Offsets.Write (this.ComputeOffset16bit (u2, class2), numbits);
			}
		}

		static BinarySearch<ushort> bsearch = new BinarySearch<ushort>();
		ushort ComputeOffset16bit (ushort item, byte klass)
		{
			int min = AccCounterPerClass[klass];
			int max = AccCounterPerClass.Count;
			if (klass < AccCounterPerClass.Count) {
				max = AccCounterPerClass[klass + 1];
			}
			int pos;
			bsearch.Search (item, OffsetTable, out pos, min, max);
			// var x = (ushort)GenericBinarySearch.Search<ushort> (item, OffsetTable, min, max);
			/*Console.WriteLine ("**** offset-table-count: {0}, pos: {1}, item: {2}, min: {3}, max: {4}, klass: {5}",
				OffsetTable.Count, pos, item, min, max, klass);
			Console.WriteLine ("**** offset-table: {0}, pos: {1}",
				OffsetTable[pos], pos);*/
			pos = pos - min;
			return (ushort)pos;
		}

		uint DecodeNext (int classindex)
		{
			ushort c1 = (ushort)this.Classes[classindex];
			ushort c2 = (ushort)this.Classes[classindex + 1];
			uint u = this._Decode (c1);
			u |= this._Decode (c2) << 16;
			return u;
		}

		uint _Decode (int klass)
		{
			if (klass == 0) {
				return 0;
			} else if (klass == 16) {
				return 0xffff;
			} else {
				int offset = (int) this.Offsets.Read (NumBitsPerClass[klass], this.ctx);
				return OffsetTable[AccCounterPerClass[klass] + offset];
			}
		}
	}
}
