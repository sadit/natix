//
//  Copyright 2012  Eric Sadit Tellez Avila <donsadit@gmail.com>
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
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.InformationRetrieval
{
	public class BasicTokenizer
	{
		public char FieldSeparator;
		public char RecordSeparator;
		public char ScapeChar;

		public BasicTokenizer ()
		{
		}

		public BasicTokenizer (char field_sep, char record_sep, char scape_char)
		{
			this.FieldSeparator = field_sep;
			this.RecordSeparator = record_sep;
			this.ScapeChar = scape_char;
		}

		public virtual void Load(BinaryReader Input)
		{
			this.FieldSeparator = Input.ReadChar();
			this.RecordSeparator = Input.ReadChar();
			this.ScapeChar = Input.ReadChar();
		}

		public virtual void Save(BinaryWriter Output)
		{
			Output.Write((char)this.FieldSeparator);
			Output.Write((char)this.RecordSeparator);
			Output.Write((char)this.ScapeChar);
		}


		public virtual IEnumerable<int> ReadInputStream (StreamReader Input)
		{
			while (!Input.EndOfStream) {
				yield return Input.Read();
			}
		}

		public virtual IEnumerable<int> ReadInputString (string Input)
		{
			for (int i = 0; i < Input.Length; ++i) {
				yield return Input[i];
			}
		}

		public virtual IEnumerable<Token> Parse (StreamReader Input, bool parsing_query)
		{
			return this.Parse(this.ReadInputStream(Input), parsing_query);
		}

		public virtual IEnumerable<Token> Parse (string Input, bool parsing_query)
		{
			return this.Parse(this.ReadInputString(Input), parsing_query);
		}

		public virtual IEnumerable<Token> Parse (IEnumerable<int> Input, bool parsing_query)
		{
			StringBuilder w = new StringBuilder ();
			bool check_control = true;
			foreach (char c in Input) {
				if (check_control) {
					if (c == this.FieldSeparator) {
						if (w.Length > 0) {
							yield return new Token (TokenType.Data, w.ToString ());
							w.Clear ();
						}
						yield return new Token (TokenType.FieldSeparator, c.ToString());
						continue;
					}
					if (c == this.RecordSeparator) {
						if (w.Length > 0) {
							yield return new Token (TokenType.Data, w.ToString ());
							w.Clear ();
						}
						yield return new Token (TokenType.RecordSeparator, c.ToString());
						continue;
					}
					if (c == this.ScapeChar) {
						check_control = false;
						continue;
					}
				}
				if (Char.IsLetterOrDigit(c)) {
					w.Append(c);
				} else {
					if (w.Length > 0) {
						yield return new Token (TokenType.Data, w.ToString ());
						w.Clear ();
					}
					yield return new Token (TokenType.Data, c.ToString());
				}
				check_control = true;
			}
			if (w.Length > 0) {
				yield return new Token(TokenType.Data, w.ToString());
				w.Clear();
				if (!parsing_query) {
					yield return new Token(TokenType.RecordSeparator, this.RecordSeparator.ToString());
				}
			}
		}

	}
}

