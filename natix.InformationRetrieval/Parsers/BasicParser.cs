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
		public Tokenizer InputTokenizer;
		
		public BasicParser (Tokenizer t)
		{
			this.InputTokenizer = t;
		}
		
		public string GetFileSeparator ()
		{
			return this.InputTokenizer.RecordSeparator.ToString();
		}

		public abstract void AddPlainString (string u);
		
		public virtual void AddSingleWord (Token token)
		{
			if (token.DataType == TokenType.Data && token.Data.Length == 0) {
					this.AddPlainString (u);
			}
		}

		public void Parse (string input_data)
		{
			foreach (var token in this.InputTokenizer.Parse(input_data, false)) {
				this.AddSingleWord (token);
			}
		}
	}
}
