//
//  Copyright 2014 Eric S. Téllez <eric.tellez@infotec.com.mx>
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
	/// <summary>
	/// A stream and random access of positive long values.
	/// </summary>
	public class LongStream : ILoadSave
	{
		OctetStream ostream;
		int count;
		List<int> offsets;
		OctetStream.Context ctx;

		static FinalTaggedByteCode CODER = new FinalTaggedByteCode();
	
		static int OFFSET_MASK = 15;
		static int OFFSET_MASK_LENGTH = 4;

		public LongStream ()
		{
			this.ostream = new OctetStream ();
			this.offsets = new List<int> ();
			this.count = 0;
			this.ctx = new OctetStream.Context ();
		}

		public LongStream (LongStream longstream)
		{
			this.ostream = new OctetStream (longstream.ostream);
			this.offsets = longstream.offsets;
			this.count = longstream.count;
			this.ctx = new OctetStream.Context ();
		}

		public int Count {
			get {
				return this.count;
			}
		}

		public long Read ()
		{
			return CODER.Decode (this.ostream, this.ctx);
		}

		/// <summary>
		/// Add the specified long value to the stream. Returns the offset in the underlying OctetStream.
		/// </summary>
		/// <param name="v">V.</param>
		public int Add (long v)
		{
			var pos = this.ostream.Count;
			if ((this.count & OFFSET_MASK) == 0) { // an absolute sample each 16 numbers, 2 extra bits per entry
				this.offsets.Add (this.ostream.Count);
			}
			CODER.Encode (v, this.ostream);
			++this.count;
			return pos;
		}

		/// <summary>
		/// Add the specified long value to the stream. Returns the offset of the first inserted value in the underlying OctetStream.
		/// </summary>
		/// <param name="v">V.</param>
		public int Add (IEnumerable<long> array)
		{
			var pos = this.ostream.Count;
			foreach (var v in array) {
				this.Add (v);
			}
			return pos;
		}
		
		public void Load (BinaryReader Input)
		{
			this.count = Input.ReadInt32 ();
			var m = Input.ReadInt32 ();
			this.offsets.Clear ();
			PrimitiveIO<int>.LoadVector (Input, m, this.offsets);
			this.ostream.Load (Input);
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.count);
			Output.Write (this.offsets.Count);
			PrimitiveIO<int>.SaveVector (Output, this.offsets);
			this.ostream.Save (Output);
		}

		/// <summary>
		/// Retrives the long stored at <term>index</term> position of the stream
		/// </summary>
		public long this[int index] {
			get {
				var ctx = new OctetStream.Context (this.offsets [index >> OFFSET_MASK_LENGTH]);
				int resting = index & OFFSET_MASK;
				long value = CODER.Decode (this.ostream, ctx);

				for (int i = 0; i < resting; ++i) {
					value = CODER.Decode (this.ostream, ctx);
				}

				return value;
			}
		}

		public List<long> Decompress (int firstIndex, int count)
		{
			var list = new List<long> ();
			this.Decompress (list, firstIndex, count);
			return list;
		}

		public void Decompress (List<long> list, int firstIndex, int count) {
			var ctx = new OctetStream.Context ();
			ctx.Offset = this.offsets [firstIndex >> OFFSET_MASK_LENGTH];
			int resting = firstIndex & OFFSET_MASK;

			for (int i = 0; i < resting; ++i) {
				CODER.Decode (this.ostream, ctx);
			}

			for (int i = 0; i < count; ++i) {
				var u = CODER.Decode (this.ostream, ctx);
				list.Add (u);
			}
		}

		public void Decompress (List<long> list, OctetStream.Context ctx, int count) {
			for (int i = 0; i < count; ++i) {
				var u = CODER.Decode (this.ostream, ctx);
				list.Add (u);
			}
		}
	}
}

