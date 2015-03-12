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
//   Original filename: natix/SimilaritySearch/QueryStream.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace natix.SimilaritySearch
{
	/// <summary>
	/// Query Reader
	/// </summary>
	public class QueryStream : IQueryStream
	{
		string[] commands;
		double queryArg = -1;
		int maxQueries;

		/// <summary>
		/// Constructor from file (use "-" to read from standard input)
		/// </summary>
		public QueryStream (string qname, double queryArg, int maxQueries=int.MaxValue)
		{
			this.commands = File.ReadAllLines (qname);
			this.queryArg = queryArg;
			this.maxQueries = maxQueries;
		}

		public IEnumerable<CommandQuery> Iterate ()
		{
			foreach (var _line in commands) {
				var line = _line.Trim ();
				if (line == "-0") {
					break;
				}
				if (line.Length == 0) {
					continue;
				}
				CommandQuery cmd;
				cmd = new CommandQuery (line, Math.Abs (this.queryArg), this.queryArg >= 0);
				yield return cmd;
			    --this.maxQueries;
			    if (this.maxQueries == 0) {
				break;
			    }

			}
		}
	}
}
