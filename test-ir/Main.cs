// 
//  Copyright 2012  sadit
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix;
using natix.SortingSearching;
using natix.Sets;
using natix.CompactDS;
using natix.InformationRetrieval;
using NDesk.Options;

namespace testir
{
	class MainClass
	{
		public static IEnumerable<string> GetFilenames (string dirname, string extension)
		{
			foreach (var e in Directory.GetFileSystemEntries (dirname)) {
				// foreach (var e in Directory.GetFiles (dirname))
				if (Directory.Exists (e)) {
					foreach (var d in GetFilenames (e, extension)) {
						yield return d;
					}
				} else {
					if (extension == null || e.EndsWith (extension)) {
						yield return e;
					}
				}
			}
		}

		public static void Main (string[] args)
		{
			string cmd = null;
			string basename = null;
			string inputdir = null;
			string listfiles = null;
			string extension = null;
			string searchalg = "galloping";
			string interalg = "in-order-tree";
			OptionSet op = new OptionSet () {
				{"help", "Show help", (v) => cmd = "help"},
				{"build", "Build an index", (v) => cmd = "build"},
				{"dir=", "Directory to scan", (v) => inputdir = v},
				{"list=", "List of filenames (one per line)", (v) => listfiles = v},
				{"ext=", "Valid extension", (v) => extension = v},
				{"search", "Search for a query in an index", (v) => cmd = "search"},
				{"index=", "Base name for the index (build or search)", (v) => basename = v},
				{"search-algorithm=", "Choose the search algorithm (sequential|binary-search|galloping|backward-galloping)",
					(v) => searchalg = v},
				{"intersection-algorithm=", "Choose an intersection algorithm (sequential|svs|small-adaptive|barbay-sequential|barbay-randomized|in-order-tree)",
					(v) => interalg = v}
			};

			var Largs = op.Parse (args);
			if (Largs.Count > 0) {
				Console.WriteLine ("Unknown arguments, valid options:");
				op.WriteOptionDescriptions (Console.Out);
				return;
			}
			var seq = new SeqTextIR ();
			switch (cmd) {
			case "build":
				if (inputdir == null && listfiles == null) {
					goto default;
				}
				if (basename == null) {
					goto default;
				}
				// var seq_builder = SequenceBuilders.GetGolynskiSucc (12);
				//var seq_builder = SequenceBuilders.GetSeqXLB_SArray64 (12);
				var seq_builder = SequenceBuilders.GetSeqXLB_DiffSetRL64 (12, 31, new EliasDelta64 ());
				//var seq_builder = SequenceBuilders.GetSeqXLB_DiffSetRL2_64 (12, 31, new EliasDelta64 ());
				Console.WriteLine ("*** building SeqTextIR instance over {0} | filter {1}", inputdir, extension);
                //var seq_container = new List<int>();
                using (var seq_container = new MemoryMappedList<int>(basename + ".memdata", 1<<12)) {
                    //var seq_container = new MemoryMappedList<int>("H", false); // false);
                    if (listfiles == null) {
                        seq.Build (GetFilenames (inputdir, extension), seq_builder, seq_container);
                    } else {
                        seq.Build (File.ReadAllLines (listfiles), seq_builder, seq_container);
                    }
                }
				Console.WriteLine ("*** saving");
				seq.Save (basename);
				/*{
					string filename;
					int sp;
					int len;
					Console.WriteLine ();
					File.WriteAllText ("out-test-docid-0", seq.GetFileData (0, out filename, out sp, out len));
					Console.WriteLine ("check file: {0}", filename);
				}*/	
				break;
			case "search":
				if (basename == null) {
					goto default;
				}
				ISearchAlgorithm<int> salg = null;
				switch (searchalg.ToLower ()) {
				case "galloping":
					salg = new DoublingSearch<int> ();
					break;
				//case "backward-galloping":
				//	salg = new BackwardDoublingSearch();
				//	break;	
				case "binary-search":
					salg = new BinarySearch<int> ();
					break;
				case "sequential":
					salg = new SequentialSearch<int> ();
					break;
				default:
					Console.WriteLine ("Unknown search algorithm: {0}", searchalg);
					ShowHelp (op, cmd);
					return;
				}
				IIntersection<int> ialg = null;
				switch (interalg.ToLower ()) {
				case "svs":
					ialg = new SvS<int> (salg);
					break;
				case "small-adaptive":
					ialg = new SmallAdaptive<int> (salg);
					break;
				case "barbay-sequential":
					ialg = new BarbaySequential<int> (salg);
					break;
				case "barbay-randomized":
					ialg = new BarbayRandomized<int> (salg);
					break;
				case "baeza-yates":
					ialg = new BaezaYatesIntersection<int> (salg);
					break;
				case "in-order-tree":
					ialg = new InOrderUnbalancedTreeIntersection<int> (0.5, salg);
					break;
				case "sequential":
					throw new NotImplementedException ("sequential intersection is not supported. Instead use svs + sequential search");
				default:
					ShowHelp (op, cmd);
					return;
				}
				seq.Load (basename);
				while (true) {
					Console.WriteLine ("query [enter]");
					string query = Console.ReadLine ();
					if (query == null || query == "") {
						break;
					}
					Search (seq, query, ialg);
				}
				break;
			case "help":
			default:
				ShowHelp (op, cmd);
				break;
			}
		}
		
		public static void ShowHelp (OptionSet op, string cmd)
		{
			Console.WriteLine ("Command: {0}", cmd);
			Console.WriteLine ("Valid options for search: index. Optional search-algorithm and intersection-algorithm");
			Console.WriteLine ("Valid options for build: index, dir, and ext");
			op.WriteOptionDescriptions (Console.Out);
		}
		
		public static void Search (SeqTextIR seq, string q, IIntersection<int> ialg)
		{
			Console.WriteLine ("Query: '{0}'", q);
			long itime = DateTime.Now.Ticks;
			var res = seq.SearchPhrase (q, ialg);
			int i = 0;
			int max_len_snippet = 128;
			foreach (var occ in res) {
				if (i >= 10) {
					Console.WriteLine ("Too many results skipping to the end");
					break;
				}
				i++;
				string uri;
				int docid;
				int sp;
				int len;
				string snippet = seq.GetSnippet (occ, max_len_snippet, out uri, out docid, out sp, out len);
				Console.WriteLine ();
				Console.WriteLine ("=========== ResultID {0}, occpos: {1}, uri: {2}", i, occ, uri);
				Console.WriteLine ("=========== Start snippet, snippet len: {0}, docid: {1}, offset_file: {2}", snippet.Length, docid, sp);
				Console.WriteLine (snippet);
				Console.WriteLine ("=========== End snippet");
			}
			Console.WriteLine ("Total results: {0}", i);
			var tspan = TimeSpan.FromTicks (DateTime.Now.Ticks - itime);
			Console.WriteLine ("query time: {0}", tspan);
		}
	}
}
