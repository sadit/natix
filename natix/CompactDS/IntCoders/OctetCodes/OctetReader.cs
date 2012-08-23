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
//   Original filename: natix/CompactDS/IntCoders/OctetCodes/OctetReader.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class OctetReader
	{
		IList<byte> stream;
		int pos;
		
		public OctetReader (IList<byte> input_stream)
		{
			this.stream = input_stream;
			this.pos = 0;
		}
		
		public int Position {
			get {
				return this.pos;
			}
			set {
				this.pos = value;
			}
		}
		
		public int Count {
			get {
				return this.stream.Count;
			}
		}
		
		public byte Read ()
		{
			var v = this.stream [this.pos];
			this.pos++;
			return v;
		}
	}
}

