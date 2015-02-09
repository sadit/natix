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
using System.Collections.Generic;

namespace natix.CompactDS
{
	public struct WTM_Symbol
	{
		public int symbol;
		public byte numbits;

		public WTM_Symbol (int symbol, byte numbits)
		{
			this.symbol = symbol;
			this.numbits = numbits;
		}
	}

	public interface ISymbolCoder : ILoadSave
	{
		List<WTM_Symbol> Encode(int symbol, List<WTM_Symbol> output);
		int Decode(List<int> codes);
	}
}

