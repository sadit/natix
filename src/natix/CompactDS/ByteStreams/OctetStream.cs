//
//   Copyright 2012,2013,2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// Creates a stream of bytes. Reads and writes bytes from and to the current position in a given Context object
	/// </summary>
	public class OctetStream : ILoadSave
	{
		public class Context {
			public int Offset;

			public Context(Context ctx) {
				this.Offset = ctx.Offset;
			}

			public Context() {
				this.Offset = 0;
			}

			public Context(int off) {
				this.Offset = off;
			}
		}

		List<byte> buff;

		public OctetStream ()
		{
			this.buff = new List<byte> ();
		}

		public OctetStream (List<byte> input_stream)
		{
			this.buff = input_stream;
		}

		public OctetStream (OctetStream oct_stream)
		{
			this.buff = oct_stream.buff;
		}

		public int Count {
			get {
				return this.buff.Count;
			}
		}

		public void Load (BinaryReader Input)
		{
			this.buff.Clear ();
			var n = Input.ReadInt32 ();
			PrimitiveIO<byte>.LoadVector (Input, n, this.buff);
		}

		public void Save (BinaryWriter Output)
		{

			Output.Write (this.buff.Count);
			PrimitiveIO<byte>.SaveVector (Output, this.buff);
		}

		/// <summary>
		/// Read the current value at Position, then increments ctx.Offset
		/// </summary>
		public byte Read (Context ctx)
		{
			var v = this.buff [ctx.Offset];
			++ctx.Offset;
			return v;
		}

		/// <summary>
		/// Add a value to the end of the stream. Returns the insertion position
		/// </summary>
		public int Add (byte b)
		{
			var pos = this.buff.Count;
			this.buff.Add (b);
			return pos;
		}


		/// <summary>
		/// Add a sequence of values to the end of the stream. Returns the first insertion position.
		/// Empty arrays will return the insertion position anyway of a non empty array.
		/// </summary>
		public int Add (IEnumerable<byte> array)
		{
			var pos = this.buff.Count;
			foreach (var a in array) {
				this.buff.Add (a);
			}
			return pos;
		}

		/// <summary>
		/// Writes a byte to the position indicated in ctx.Offset, then increments ctx.Offset
		/// </summary>
		public void Write (byte b, Context ctx)
		{
			if (ctx.Offset == this.buff.Count) {
				this.buff.Add (b);
			} else if (ctx.Offset > this.buff.Count) {
				throw new ArgumentOutOfRangeException ("Trying to write out of declared bounds");
			} else {
				this.buff [ctx.Offset] = b;
			}
			++ctx.Offset;
		}

		/// <summary>
		/// Writes <term>times</term> times the given byte.
		/// </summary>
		public void Write (byte b, int times, Context ctx)
		{
			for (int i = 0; i < times; ++i) {
				this.Write (b, ctx);
			}
		}

		/// <summary>
		/// Writes a list of bytes to the position indicated in ctx.
		/// </summary>
		public void Write (byte[] arr, Context ctx)
		{
			foreach (var b in arr) {
				this.Write (b, ctx);
			}
		}
	}
}

