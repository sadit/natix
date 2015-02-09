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
//   Original filename: natix/SimilaritySearch/Commands.cs
// 
using System;
//using NUnit.Framework;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;
using NDesk.Options;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// A filter/notifier handler for every query's result
	/// </summary>
	//public delegate IResult SearchFilter (string qraw, double qtype, IResult res, Index index);

	/// <summary>
	/// Search information and options
	/// </summary>
	public class ShellSearchOptions
	{
		/// <summary>
		/// File name or identifier of the queries
		/// </summary>
		public string QueryName;
		/// <summary>
		/// Index name or identifier for the index
		/// </summary>
		public string IndexName;
		/// <summary>
		/// Filename to save the results. Setting to null avoids the storage. It is null by default.
		/// </summary>
		public string ResultName = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public ShellSearchOptions (string queryname, string indexname, string resultname = null, int showmaxres=128)
		{
			this.QueryName = queryname;
			this.IndexName = indexname;
			this.ResultName = resultname;
		}
	}
	
	/// <summary>
	/// Gives the functionality of the sisap's queries commands. With enhanced capabilities
	/// </summary>
	public class Commands
	{
		/// <summary>
		/// Parse a single string into tokens (command line style)
		/// </summary>
		public static IEnumerable<string> TokenizeLine (string line)
		{
			MatchCollection C = Regex.Matches(line, @"(\S+)");
			var L = new List<string>();
			foreach (Match c in C) {
				L.Add(c.Value);
			}
			return L;
		}

		/// <summary>
		/// Search shell (not interactive at this level)
		/// </summary>

		public static void Search (Index index, IEnumerable<CommandQuery> qReader, ShellSearchOptions searchOps)
		{
			var summary = new ResultSummary () {
				ResultName = searchOps.ResultName,
				IndexName = searchOps.IndexName,
				QueriesName = searchOps.QueryName
			};
			int qid = 0;
			long totaltime = 0;
			SearchCost totalCost = new SearchCost (0, 0);
			foreach (CommandQuery qItem in qReader) {
				long tstart = DateTime.Now.Ticks;
				SearchCost startCost = index.Cost;
				IResult res;
				var qobj = qItem.QObj;
				if (qobj == null) {
					qobj = index.DB.Parse (qItem.QRaw);
				}
				if (qItem.QTypeIsRange) {
					res = index.SearchRange (qobj, qItem.QArg);
				} else {
					res = index.SearchKNN (qobj, (int)qItem.QArg);
				}
				var qraw = qItem.QRaw;
				SearchCost finalCost = index.Cost;
				finalCost.Internal -= startCost.Internal;
				finalCost.Total -= startCost.Total;
				totalCost.Internal += finalCost.Internal;
				totalCost.Total += finalCost.Total;
				long time = DateTime.Now.Ticks - tstart;
				totaltime += time;
				var query = new Query () {
					QueryID = qid,
					QueryType = qItem.EncodeQTypeQArgInSign(),
					QueryRaw =  qraw,
					SearchCostTotal = finalCost.Total,
					SearchCostInternal = finalCost.Internal,
					SearchTime = (new TimeSpan(time)).TotalSeconds,
					Result = new List<ItemPair>(res)
				};
				Console.WriteLine ("-----  QueryID: {0}, QueryType: {1}  -----", query.QueryID, query.QueryType);
				if (res.Count == 0) {
					Console.WriteLine ("- results> empty result set");
				} else {
					Console.WriteLine ("- results> count: {0}, first-dist: {1}, last-dist: {2}", res.Count, res.First.Dist, res.CoveringRadius);
				}
				Console.WriteLine ("- search-time: {0}, cost-internal-distances: {1}, cost-total-distances: {2}", query.SearchTime, query.SearchCostInternal, query.SearchCostTotal);
				Console.WriteLine ("- index: {0}, db: {1}, result: {2}", index,
				                   Path.GetFileName(index.DB.Name),
				                   Path.GetFileName(searchOps.ResultName));
				summary.Add (query);
				qid++;
			}
			summary.ComputeSummary ();
			if (searchOps.ResultName != null) {
				File.WriteAllText (searchOps.ResultName, JsonConvert.SerializeObject (summary, Formatting.Indented));
			}
			Console.WriteLine ("Number queries: {0}", qid);
			Console.WriteLine ("Average total-numdists: {0}", (totalCost.Total + 0.0) / qid);
            Console.WriteLine ("Average internal-distances: {0}", (totalCost.Internal + 0.0) / qid);
            Console.WriteLine ("Average external-distances: {0}", (totalCost.Total - totalCost.Internal + 0.0) / qid);
			Console.WriteLine ("Total search time: {0}", (new TimeSpan (totaltime)).TotalSeconds);
			Console.WriteLine ("Average search time: {0}", (new TimeSpan (totaltime / qid)).TotalSeconds);
		}
		
		/// <summary>
		/// Method performing the check command
		/// </summary>
		/// <param name="argsList">
		/// Command line like style
		/// </param>
		public static void Check (IEnumerable<string> argsList)
		{
			string outname = null;
			bool help = false;
			var op = new OptionSet() {
				{"check", "Command name, consumes token", v => int.Parse("0")},
				{"save|outname=", "Output of the tabulation filename", v => outname = v},
				{"help|h", "Shows this help message", v => help = true}
			};

			List<string> checkList = op.Parse(argsList);
			if (1 > checkList.Count || help) {
				Console.WriteLine ("Usage --check [--options] res-basis res-list...");
				op.WriteOptionDescriptions(Console.Out);
				return;
			}
			var list = new List<ResultSummary> ();
			var B = JsonConvert.DeserializeObject<ResultSummary> (File.ReadAllText (checkList [0]));
			foreach(string arg in checkList) {
				var R = JsonConvert.DeserializeObject<ResultSummary> (File.ReadAllText (arg));
				R.Parametrize (B);
				R.ResultName = arg;
				R.QueryList = null;
				list.Add (R);
				// Console.WriteLine (JsonConvert.SerializeObject(R, Formatting.Indented));
			}

			var s = JsonConvert.SerializeObject (list, Formatting.Indented);
			Console.WriteLine (s);
			if (outname != null) {
				File.WriteAllText(outname, s);
			}	
		}		
	}
}
