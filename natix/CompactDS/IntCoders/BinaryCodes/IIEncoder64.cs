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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/IIEncoder64.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	public interface IIEncoder64 : ILoadSave
	{
		/// <summary>
		/// Decodes the next 64 bit integer (pointed by context ctx) from the bit stream
		/// </summary>
		long Decode (IBitStream stream, BitStreamCtx ctx);
		/// <summary>
		/// Encodes "u" into the specified "stream"
		/// </summary>
		void Encode (IBitStream stream, long u);
	}
}

