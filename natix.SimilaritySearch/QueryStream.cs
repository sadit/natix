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
//using NUnit.Framework;
using System.Xml;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;


namespace natix.SimilaritySearch
{
	/// <summary>
	/// The pair to be searched
	/// </summary>
	public struct CommandQuery
	{
		/// <summary>
		/// Query Type
		/// </summary>
		public bool QTypeIsRange;
		/// <summary>
		/// Query Argument 
		/// </summary>
		public double  QArg;
		/// <summary>
		/// Query object in string representation
		/// </summary>
		public string QRaw;		
		/// <summary>
		/// Constructor
		/// </summary>

		public CommandQuery (string qraw, double qarg, bool qtypeisrange)
		{
			this.QRaw = qraw;
			this.QArg = qarg;
			this.QTypeIsRange = qtypeisrange;
		}	
		
		public double EncodeQTypeQArgInSign ()
		{
			if (this.QTypeIsRange) {
				return Math.Abs (this.QArg);
			} else {
				if (this.QArg > 0) {
					return -this.QArg;
				} else {
					return this.QArg;
				}
			}
		}
	}
	
	/// <summary>
	/// Query Reader
	/// </summary>
	public class QueryStream
	{
		static char[] sep1 = new char[] {','};
		static char[] sep2 = new char[] {' '};
		string[] commands;
		/// <summary>
		/// Constructor from file (use "-" to read from standard input)
		/// </summary>
		public QueryStream (string qname)
		{
			this.commands = File.ReadAllLines (qname);
		}

		public IEnumerable<CommandQuery> Iterate ()
		{
			foreach (var line in commands) {
				if (line == "-0") {
					break;
				}
				string[] s = line.Split (sep1, 2);
				CommandQuery cmd;
				if (s [0].StartsWith ("search")) {
					var m = s [0].Split (sep2);
					if (m.Length != 3) {
						throw new ArgumentOutOfRangeException ("search cmd must be 'search {knn|range}' arg");
					}
					var qtype = m [1];
					var qarg = double.Parse(m [2]);
					var qraw = s [1];
					cmd = new CommandQuery (qraw, qarg, qtype.CompareTo ("range") == 0);
				} else {
					var qarg = double.Parse (s [0]);
					cmd = new CommandQuery (s [1], Math.Abs (qarg), qarg >= 0);
				}
				yield return cmd;
			}
		}
	}
}
