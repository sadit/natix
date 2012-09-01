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
//   Original filename: natix/ftindex2/Parsers/QueryParser.cs
// 
using natix;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace natix.InformationRetrieval
{
	public class QueryParser : BasicParser
	{
		public IList<string> Query;
		
		public QueryParser (Tokenizer tokenizer) : base(tokenizer)
		{
			this.Query = new List<string> ();
		}
		
		public override void AddPlainString (string u)
		{
			this.Query.Add (u);
		}
	}
}
