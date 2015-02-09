//
//  Copyright 2012  Eric Sadit Tellez Avila
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
	public class EqualSizeCoder : ISymbolCoder
	{
		public IIEncoder32 Coder;
		public byte bits_per_code;

		public EqualSizeCoder (byte bits_per_code, int max_value)
		{
			this.bits_per_code = bits_per_code;
			var bits_per_symbol = ListIFS.GetNumBits(max_value);
			this.Coder = new BinaryCoding(bits_per_symbol);
//			if (coder == null) {
//				//var numbits = (int)Math.Ceiling(numbits * 1.0 / this.bits_per_code) * this.bits_per_code;
//				//coder = new BinaryCoding(numbits);
//
//			}
//			this.Coder = coder;
		}

		public EqualSizeCoder()
		{
		}

		public List<WTM_Symbol> Encode (int symbol, List<WTM_Symbol> output = null)
		{
			var coderstream = new BitStream32 ();
			this.Coder.Encode (coderstream, symbol);
			int numbits = (int)coderstream.CountBits;
			var ctx = new BitStreamCtx (0);
			if (output == null) {
				output = new List<WTM_Symbol> ();
			}
			for (int i = 0; i < numbits; i+= this.bits_per_code) {
				int code = (int)coderstream.Read (this.bits_per_code, ctx);
				output.Add(new WTM_Symbol(code, (byte) Math.Min (this.bits_per_code, numbits - i)));
				// Console.WriteLine("get-mini symbol: {0}, numbits: {1}, i: {2}, code: {3}", symbol, numbits, i, code);
			}
			return output;
		}

		public int Decode (List<int> codes)
		{
			int symbol = 0;
			for (int i = 0; i < codes.Count; ++i) {
				symbol |= codes[i] << (this.bits_per_code * i);
			}
			return symbol;
		}

		public void Load (BinaryReader Input)
		{
			this.Coder = IEncoder32GenericIO.Load(Input);
			this.bits_per_code = Input.ReadByte();
		}
		public void Save(BinaryWriter Output)
		{
			IEncoder32GenericIO.Save(Output, this.Coder);
			Output.Write((byte) this.bits_per_code);
		}
	}
}

