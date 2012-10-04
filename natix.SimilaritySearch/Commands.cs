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
		/// Show maximum number of results in the standard output, defaults to 128
		/// </summary>
		public int ShowMaxResult = 128;
		/// <summary>
		/// If true show the histogram of distances of each result, defaults to true
		/// </summary>
		public bool ShowHist = true;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public ShellSearchOptions (string queryname, string indexname, string resultname = null, int showmaxres=128, bool showhist = true)
		{
			this.QueryName = queryname;
			this.IndexName = indexname;
			this.ResultName = resultname;
			this.ShowMaxResult = showmaxres;
			this.ShowHist = showhist;
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
			BinaryWriter ResultOutput = null;
			if (searchOps.ResultName != null) {
				ResultOutput = new BinaryWriter (File.Create (searchOps.ResultName + ".tmp"));
			}
			SortedDictionary<double, int> avg_hist = new SortedDictionary<double, int> ();
			int qid = 0;
			long totaltime = 0;
			SearchCost totalCost = new SearchCost (0, 0);
			if (ResultOutput != null) {
				var reslist = new ResultList (searchOps.IndexName, searchOps.QueryName);
				// Dirty.SerializeBinary (Output, reslist);
				reslist.Save (ResultOutput);
			}
			foreach (CommandQuery qItem in qReader) {
				long tstart = DateTime.Now.Ticks;
				SearchCost startCost = index.Cost;
				IResult res;
				if (qItem.QTypeIsRange) {
					res = index.SearchRange (index.DB.Parse(qItem.QRaw, true), qItem.QArg);
				} else {
					res = index.SearchKNN (index.DB.Parse(qItem.QRaw, true), (int)qItem.QArg);
				}
				var qraw = qItem.QRaw;
				if (qraw.Length > 1024) {
					qraw = "<very-large-qraw>";
				}
				SearchCost finalCost = index.Cost;
				finalCost.Internal -= startCost.Internal;
				finalCost.External -= startCost.External;
				totalCost.Internal += finalCost.Internal;
				totalCost.External += finalCost.External;
				long time = DateTime.Now.Ticks - tstart;
				totaltime += time;
				SortedDictionary<double, int> hist = new SortedDictionary<double, int> ();
				if (searchOps.ShowHist) {
					foreach (ResultPair p in res) {
						if (hist.ContainsKey (p.dist)) {
							hist [p.dist]++;
						} else {
							hist [p.dist] = 1;
						}
					}
					foreach (var p in hist) {
						if (avg_hist.ContainsKey (p.Key)) {
							avg_hist [p.Key] += p.Value;
						} else {
							avg_hist [p.Key] = p.Value;
						}
					}
					if (avg_hist.Count > 1000) {
						searchOps.ShowHist = false;
						Console.WriteLine ("WARNING: Histogram of distances was disabled because there are too many bins");
					}
				}
				ResultInfo info = new ResultInfo (qid, qItem.EncodeQTypeQArgInSign(), qraw, finalCost, new TimeSpan (time), res);
				if (ResultOutput != null) {
					// Dirty.SerializeBinary (ResultOutput, info);
					info.Save (ResultOutput);
				}
				Console.WriteLine (info.ToString (searchOps.ShowMaxResult, null));
				if (searchOps.ShowHist) {
					Console.WriteLine ("Distance histogram (dist => counter)");
					foreach (KeyValuePair<double, int> xp in hist) {
						Console.Write ("({0} => {1}), ", xp.Key, xp.Value);
					}
					Console.WriteLine ("<TheEnd>");
				}
				Console.WriteLine ("Number Results: {0}", res.Count);
				qid++;
			}
			if (searchOps.ShowHist) {
				Console.WriteLine ("Average Distance histogram (dist => counter)");
				foreach (KeyValuePair<double, int> xp in avg_hist) {
					Console.Write ("({0} => {1}), ", xp.Key, ((double)xp.Value) / qid);
				}
				Console.WriteLine ("<TheEnd>");
			}
			if (ResultOutput != null) {
				ResultOutput.Close ();
				if (File.Exists (searchOps.ResultName)) {
					File.Delete (searchOps.ResultName);
				}
				File.Move (searchOps.ResultName + ".tmp", searchOps.ResultName);
			}
			Console.WriteLine ("Number queries: {0}", qid);
			Console.WriteLine ("Average numdists: {0}", (totalCost.Internal + totalCost.External + 0.0) / qid);
			Console.WriteLine ("Total search time: {0}", (new TimeSpan (totaltime)).TotalSeconds);
			Console.WriteLine ("Average search time: {0}", (new TimeSpan (totaltime / qid)).TotalSeconds);
		}
		
		/// <summary>
		/// Print histogram of distances of a result
		/// </summary>
		public static void Hist (IEnumerable<string> argsList)
		{
			string scriptname = null;
			int maxresult = int.MaxValue;
			int round = 3;
			// bool plot = false;
			bool help = false;
			var op = new OptionSet() {	
				{"maxresult=", "Number of items to take into account", v => maxresult = int.Parse(v)},
				{"round|r=", String.Format("Number of digits to round. Default {0}", round), v => round = int.Parse(v)},
				// {"plot|p", "Plot the output", v => plot = true},
				{"scriptname|save=", "Output of the histogram script", v => scriptname = v},
				{"help|h", "Shows this help message", v => help = true}
			};
			List<string> checkList = op.Parse(argsList);
			if (checkList.Count == 0 || help) {
				Console.WriteLine ("Usage --hist [--options] result-file...");
				op.WriteOptionDescriptions(Console.Out);
				return;
			}
			StringWriter w = new StringWriter();
			foreach (var arg in checkList) {
				ResultList R = ResultList.FromFile(arg);
				Dictionary<double, int> H = new Dictionary<double, int>();
				int Qcount = 0;
				foreach (var resultInfo in R) {
					Qcount++;
					int m = 0;
					foreach (var result in resultInfo.result) {
						double k = Math.Round(result.dist, round);
						if (H.ContainsKey(k)) {
							H[k] = H[k] + 1;
						} else {
							H[k] = 1;
						}
						if (m >= maxresult) {
							break;
						}
						m++;
					}
				}
				List<double> keyList = new List<double>(H.Keys);
				keyList.Sort();
				w.WriteLine("# Histogram for {0}", arg);
				w.WriteLine("set terminal postscript rounded font \"Helvetica\" 18");
				w.WriteLine("set output 'Plot.Hist.{0}.ps'", Path.GetFileName(arg).Replace("=", "."));
				w.WriteLine("set key off");
				w.WriteLine("set xlabel \"Distance\"");
				w.WriteLine("set ylabel \"Average frequency ({0} queries)\"", Qcount);
				w.WriteLine("set size 0.6,0.6");
				w.WriteLine("set title \"Histogram of distances\\n{0}\"", arg);
				w.WriteLine("set boxwidth 0.88 relative");
				w.WriteLine("plot '-' using 1:2 with boxes fs solid 0.7");
				foreach (double k in keyList) {
					w.WriteLine("{0} {1}", k, H[k]);
				}
				w.WriteLine("e");
			}
			if (scriptname != null) {
				File.WriteAllText(scriptname, w.ToString());
			}
			Console.WriteLine(w.ToString());
		}
		/// <summary>
		/// Method performing the check command
		/// </summary>
		/// <param name="argsList">
		/// Command line like style
		/// </param>
		public static void Check (IEnumerable<string> argsList)
		{
			int maxbasissize = int.MaxValue;
			int maxresultsize = int.MaxValue;
			string names = null;
			string outname = null;
			bool vertical = true;
			bool help = false;
			bool join = false;
			var op = new OptionSet() {
				{"check", "Command name, consumes token", v => int.Parse("0")},
				{"names=", "The docid to name listing, this can be used to check by name (e.g. audio). A name per line text file (docid = linenumber starting with 0).", v => names = v},
				{"join|j", "Joins the results files creating a single result to be checked against the basis (the first result file). The resources (time and distances) are presented as the sum of all the joined results", v => join = true},
				{"maxbasis=", "Compute statistics for a maximum number of results (e.g. change number of candidates)", v => maxbasissize = int.Parse(v)},
				{"maxresult=", "Compute statistics for a different number in the basis (e.g. cut knn results)", v => maxresultsize = int.Parse(v)},
				{"horizontal|H", "Print information horizontally", v => vertical = false},
				{"save|outname=", "Output of the tabulation filename", v => outname = v},
				{"help|h", "Shows this help message", v => help = true}
			};

			List<string> checkList = op.Parse(argsList);
			if (1 > checkList.Count || help) {
				Console.WriteLine ("Usage --check [--options] res-basis res-list...");
				op.WriteOptionDescriptions(Console.Out);
				return;
			}
			string[] dbnames = null;
			if (names != null) {
				dbnames = File.ReadAllLines(names);
			}
			StringWriter sw = new StringWriter();
			if (join) {
				var B = ResultList.FromFile(checkList[0]);
				var R = ResultList.FromFile(checkList[1]);
				for(int i = 2; i < checkList.Count; i++) {
					var _R = ResultList.FromFile(checkList[i]);
					R.Extend(_R);
				}
				R.Parametrize (B, maxbasissize, maxresultsize, dbnames);
				R.Name = "joined";
				sw.WriteLine( R.ToString (vertical, true) );
			} else {
				var B = ResultList.FromFile(checkList[0]);
				bool isfirst = true;
				foreach(string arg in checkList) {
					var R = ResultList.FromFile(arg);
					R.Parametrize (B, maxbasissize, maxresultsize, dbnames);
					R.Name = arg;
					sw.WriteLine (R.ToString (vertical, isfirst));
					isfirst = false;
				}
			}
			string _s = sw.ToString();
			sw.Close();
			Console.WriteLine(_s);
			if (outname != null) {
				File.WriteAllText(outname, _s);
			}	
		}		
	}
}
