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
//   Original filename: natix/ftindex2/Parsers/BasicParser.cs
// 
using natix;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace natix.InformationRetrieval
{	
	public abstract class BasicParser
	{
		ParserRegex parser_regex;
		
		public BasicParser (ParserRegex parser)
		{
			this.parser_regex = parser;
		}
		
		public string GetFileSeparator ()
		{
			return this.parser_regex.FileSeparator;
		}

		public abstract void AddPlainString (string u);
		
		public virtual void AddSingleWord (string u, EntityType t)
		{
			if (u.Length == 0) {
				return;
			}
			if (t == EntityType.SpecialCharacters || t == EntityType.WhiteSpaces) {
				for (int i = 0; i < u.Length; i++) {
					this.AddSingleWord (u [i].ToString (), EntityType.PlainString);
				}
				return;
			}
			this.AddPlainString (u);
		}

		void AddWhiteSpace (string w)
		{
			// for (int i = 0; i < w.Length; i++) {
			//	this.AddSingleWord (w[i].ToString());
			// }
			this.AddSingleWord (w, EntityType.WhiteSpaces);
		}
		
		void ParseWord (string w)
		{
			var M = this.parser_regex.RegexWord.Match (w);
			if (M.Success) {
				while (M.Success) {
					this.AddSingleWord (M.Groups [1].Value, EntityType.SpecialCharacters);
					this.AddSingleWord (M.Groups [2].Value, EntityType.PlainString);
					this.AddSingleWord (M.Groups [3].Value, EntityType.SpecialCharacters);
					M = M.NextMatch ();
				}
			} else {
				this.AddSingleWord (w, EntityType.SpecialCharacters);
			}
		}
		
		public void Parse (string input_data)
		{
			var M = this.parser_regex.RegexWhiteSpace.Match (input_data);
			if (M.Success) {
				while (M.Success) {
					this.AddWhiteSpace (M.Groups [1].Value);
					this.ParseWord (M.Groups [2].Value);
					this.AddWhiteSpace (M.Groups [3].Value);
					M = M.NextMatch ();
				}
			} else {
				this.AddWhiteSpace (input_data);
			}
		}
	}
}
