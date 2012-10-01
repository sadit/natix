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
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class TopKFreqCoder : ISymbolCoder
	{
		public ISymbolCoder NotFreqCoder;
		public IList<int> Dic;

		public TopKFreqCoder (int K, IList<int> alphabet_freqs, ISymbolCoder not_freq_coder)
		{
			var top = new TopK<int> (K);
			var n = alphabet_freqs.Count;
			this.Dic = new int[K];
			int i;
			for (i = 0; i < n; ++i) {
				top.Push (-alphabet_freqs [i], i);
			}
			i = 0;
			foreach (var p in top.Items.Traverse()) {
				this.Dic[i] = p.Value;
				++i;
			}
			this.NotFreqCoder = not_freq_coder;
		}

		public TopKFreqCoder()
		{
		}

		public List<WTM_Symbol> Encode (int symbol, List<WTM_Symbol> output = null)
		{
			if (output == null) {
				output = new List<WTM_Symbol> ();
			}
			var K = this.Dic.Count;
			var numbits = (byte)ListIFS.GetNumBits(K);
			for (int i = 0; i < K; ++i) {
				if (this.Dic[i] == symbol) {
					output.Add(new WTM_Symbol(i, numbits));
					return output;
				}
			}
			output.Add(new WTM_Symbol(K, numbits));
			output = this.NotFreqCoder.Encode(symbol, output);
			return output;
		}

		public int Decode (List<int> codes)
		{
			if (codes.Count == 1) {
				return this.Dic[codes[0]];
			}
			codes.RemoveAt(0);
			return this.NotFreqCoder.Decode(codes);
		}

		public void Load (BinaryReader Input)
		{
			this.NotFreqCoder = SymbolCoderGenericIO.Load(Input);
			this.Dic = ListIGenericIO.Load(Input);
		}

		public void Save(BinaryWriter Output)
		{
			SymbolCoderGenericIO.Save(Output, this.NotFreqCoder);
			ListIGenericIO.Save(Output, this.Dic);
		}
	}
}

