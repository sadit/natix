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
using natix.CompactDS;
using natix.InformationRetrieval;
using System.Collections;
using System.Collections.Generic;

namespace cftdb
{
	public class ColumnBuilder
	{
		public Dictionary<string,int> D;
		public List<int> C;

		public ColumnBuilder ()
		{
			this.D = new Dictionary<string,int>();
			this.C = new List<int>();
		}

		public int GetWordId (string w)
		{
			int word_id;
			if (D.TryGetValue (w, out word_id)) {
				return word_id;
			}
			word_id = D.Count;
			D[w] = word_id;
			return word_id;
		}

		public void Add(string w)
		{
			var word_id = this.GetWordId(w);
			C.Add(word_id);
		}

		public Column Finish(string rec_sep_string, SequenceBuilder seq_builder)
		{
			var voc = MapVocSeq.SortingVoc(this.D, this.C);
			Console.WriteLine("=== finishing column> voc.count: {0}, text.count: {1}", voc.Count, this.C.Count);
			var seq = seq_builder(this.C, voc.Count);
			return new Column(seq, voc, rec_sep_string);
		}
	}
}