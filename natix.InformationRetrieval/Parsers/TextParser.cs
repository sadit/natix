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
//   Original filename: natix/ftindex2/Parsers/TextParser.cs
// 
using natix;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using natix.Sets;
using natix.CompactDS;
using natix.SortingSearching;


namespace natix.InformationRetrieval
{
	public class TextParser : BasicParser
	{
		// Temporary variables
		public IDictionary<string, int> Voc;
		/// <summary>
		/// The new text (replacing each word by an identifier to the vocabulary)
		/// </summary>
		public List<int> Seq;
		
		public TextParser (Tokenizer t) : base(t)
		{
			this.Voc = new Dictionary<string, int> ();
			//this.InvIndex = new List<IList<int>> ();
			this.Seq = new List<int> ();
		}
		
		public override void AddPlainString (string u)
		{
			int word_id;
			if (!this.Voc.TryGetValue (u, out word_id)) {
				word_id = this.Voc.Count;
				this.Voc [u] = word_id;
				if (this.Seq.Count % 10000 == 0) {
					Console.WriteLine ("len-alphabet: {0}, len-text: {1}", this.Voc.Count, this.Seq.Count);
				}
			}
			this.Seq.Add (word_id);			
		}

	}
}
