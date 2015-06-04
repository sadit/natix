//
//   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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

using System;
using System.IO;
using System.Collections.Generic;
using NDesk.Options;

using natix;
using natix.SimilaritySearch;

namespace ExactIndexes
{
	class MainClass
	{
		public static void ExecuteMain(string nick, IndexArgumentSetup setup, Action prepare_db)
		{
			var dbname = String.Format ("DB.{0}", Path.GetFileName(setup.DATABASE));
			setup.BINARY_DATABASE = dbname;

			prepare_db ();

			SpaceGenericIO.Load (setup.BINARY_DATABASE); 
			var arglist = new System.Collections.Concurrent.ConcurrentQueue<String> ();
			arglist.Enqueue ("--save");
			arglist.Enqueue (String.Format ("Tab.ExactIndexes.{0}.{1}.qarg={2}.json", nick, Path.GetFileName (setup.QUERIES), setup.QARG));

			arglist.Enqueue (Indexes.ExecuteSeq (setup, nick));
			var actionlist = new List<Action> ();


			if (setup.ExecuteParameterless) {
				actionlist.Add (() => {
					var resname = Indexes.ExecuteVPT (setup, nick);
					arglist.Enqueue(resname);
				});

				actionlist.Add (() => {
					var resname = Indexes.ExecuteSAT (setup, nick);
					arglist.Enqueue(resname);
				});
				actionlist.Add (() => {
					var resname = Indexes.ExecuteSATDistal (setup, nick);
					arglist.Enqueue(resname);
				});
				actionlist.Add (() => {
					var resname = Indexes.ExecuteSATRandom (setup, nick);
					arglist.Enqueue(resname);
				});
				actionlist.Add (() => {
					var resname = Indexes.ExecuteNANNI(setup, nick);
					arglist.Enqueue(resname);
				});
				actionlist.Add (() => {
					var resname = Indexes.ExecuteTNANNI(setup, nick);
					arglist.Enqueue(resname);
				});
				// arglist.Add (Indexes.ExecuteVPTX (setup, nick));
				//arglist.Add (Indexes.ExecuteMILCv3 (setup, nick));
			}

			foreach (var bsize in setup.LC) {
				var _bsize = bsize;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteLC (setup, nick, _bsize);
					arglist.Enqueue(resname);
				});
			}

			foreach (var numGroups in setup.MILC) {
				var _numGroups = numGroups;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteMANNI (setup, nick, _numGroups);
					arglist.Enqueue(resname);
				});
				actionlist.Add (() => {
					var resname = Indexes.ExecuteTMANNI (setup, nick, _numGroups);
					arglist.Enqueue(resname);
				});
				actionlist.Add (() => {
					var resname = Indexes.ExecuteDMANNI (setup, nick, _numGroups);
					arglist.Enqueue(resname);
				});
				//arglist.Add (Indexes.ExecuteMILCv2 (setup, nick, numGroups));
			}

			foreach (var numGroups in setup.EPT) {
				var _numGroups = numGroups;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteEPTA (setup, nick, _numGroups);
					arglist.Enqueue (resname);
				});
				// arglist.Add (Indexes.ExecuteEPT (setup, nick, numGroups));
			}

			foreach (var numPivs in setup.BNC) {
				var _numPivs = numPivs;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteBNCInc (setup, nick, _numPivs);
					arglist.Enqueue (resname);
				});
			}

			foreach (var numPivs in setup.KVP) {
				var _numPivs = numPivs;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteKVP (setup, nick, _numPivs, setup.KVP_Available);
					arglist.Enqueue (resname);
				});
			}

			foreach (var numPivs in setup.SPA) {
				var _numPivs = numPivs;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteSpaghetti (setup, nick, _numPivs);
					arglist.Enqueue (resname);
				});
			}

			foreach (var numPivs in setup.LAESA) {
				var _numPivs = numPivs;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteLAESA (setup, nick, _numPivs);
					arglist.Enqueue (resname);
				});
			}

			foreach (var alpha in setup.SSS) {
				var _alpha = alpha;
				actionlist.Add (() => {
					var resname = Indexes.ExecuteSSS (setup, nick, _alpha, setup.SSS_max);
					arglist.Enqueue (resname);
				});
			}


			if (setup.CORES == 1) {
				foreach (var action in actionlist) {
					action.Invoke ();
				}
			} else {
				LongParallel.ForEach (actionlist, (a) => a.Invoke (), setup.CORES);
			}
			if (setup.ExecuteSearch) {
				Commands.Check (arglist);
			}
		}

		public static void MainSEQED (IndexArgumentSetup setup)
		{
			var basename = Path.GetFileName (setup.DATABASE);
			var nick = String.Format("{0}{1}", setup.PREFIX, basename);

			if (!Directory.Exists (nick)) {
				Directory.CreateDirectory (nick);
			}

			ExecuteMain (nick, setup, () => {
				SeqLevenshteinSpace<int> sp;
				if (!File.Exists(setup.BINARY_DATABASE)) {
					sp = new SeqLevenshteinSpace<int>();
					sp.Build(setup.DATABASE);
					SpaceGenericIO.Save(setup.BINARY_DATABASE, sp);
				}
			});
		}

		public static void MainSTRED(IndexArgumentSetup setup)
		{
			var basename = Path.GetFileName (setup.DATABASE);
			var nick = String.Format("{0}{1}", setup.PREFIX, basename);

			if (!Directory.Exists (nick)) {
				Directory.CreateDirectory (nick);
			}

			ExecuteMain (nick, setup, () => {
				if (!File.Exists(setup.BINARY_DATABASE)) {
					var sp = new StringLevenshteinSpace();
					sp.Build(setup.DATABASE);
					SpaceGenericIO.Save(setup.BINARY_DATABASE, sp);
				}
			});
		}

		public static void MainWIKTIONARY(IndexArgumentSetup setup)
		{
			var basename = Path.GetFileName (setup.DATABASE);
			var nick = String.Format("{0}{1}", setup.PREFIX, basename);

			if (!Directory.Exists (nick)) {
				Directory.CreateDirectory (nick);
			}

			ExecuteMain (nick, setup, () => {
				if (!File.Exists(setup.BINARY_DATABASE)) {
					Wiktionary sp = new Wiktionary();
					sp.Build(setup.DATABASE);
					SpaceGenericIO.Save(setup.BINARY_DATABASE, sp);
				}
			});
		}

		public static void MainCOPHIR(IndexArgumentSetup setup)
		{
			var basename = Path.GetFileName (setup.DATABASE);
			var nick = String.Format("{0}{1}", setup.PREFIX, basename);

			if (!Directory.Exists (nick)) {
				Directory.CreateDirectory (nick);
			}

			ExecuteMain (nick, setup, () => {
				if (!File.Exists(setup.BINARY_DATABASE)) {
				    throw new Exception("The CoPhIR space doesn't support an automatic compilation");
				}
			});
		}

		public static void MainDOC (IndexArgumentSetup setup)
		{
			var basename = Path.GetFileName (setup.DATABASE);
			var nick = String.Format("{0}{1}", setup.PREFIX, basename);

			if (!Directory.Exists (nick)) {
				Directory.CreateDirectory (nick);
			}

			ExecuteMain (nick, setup, () => {
				DocumentDB sp;
				if (!File.Exists(setup.BINARY_DATABASE)) {
					sp = new DocumentDB();
					var list = new List<string>(Directory.EnumerateFiles(setup.DATABASE));
					list.Sort();
					sp.Build(setup.DATABASE, list);
					SpaceGenericIO.Save(setup.BINARY_DATABASE, sp);
				}
			});
		}

		public static void MainVEC (IndexArgumentSetup setup, string stype)
		{
			var basename = Path.GetFileName (setup.DATABASE);
			var nick = String.Format("{0}{1}", setup.PREFIX, basename);

			if (!Directory.Exists (nick)) {
				Directory.CreateDirectory (nick);
			}

			switch (stype) {
			case "VEC":
				ExecuteMain (nick, setup, () => {
					if (!File.Exists (setup.BINARY_DATABASE)) {
						MemMinkowskiVectorDB<float> db = new MemMinkowskiVectorDB<float> ();
						db.Build (setup.DATABASE);
						SpaceGenericIO.Save (setup.BINARY_DATABASE, db);
					}
				});
				break;
			case "VEC_UInt8":
				ExecuteMain (nick, setup, () => {
					if (!File.Exists (setup.BINARY_DATABASE)) {
						MemMinkowskiVectorDB<byte> db = new MemMinkowskiVectorDB<byte> ();
						db.Build (setup.DATABASE);
						SpaceGenericIO.Save (setup.BINARY_DATABASE, db);
					}
				});
				break;
			
			case "VEC_Int8":
				ExecuteMain (nick, setup, () => {
					if (!File.Exists (setup.BINARY_DATABASE)) {
						MemMinkowskiVectorDB<sbyte> db = new MemMinkowskiVectorDB<sbyte> ();
						db.Build (setup.DATABASE);
						SpaceGenericIO.Save (setup.BINARY_DATABASE, db);
					}
				});
				break;
	
			case "VEC_Int16":
				ExecuteMain (nick, setup, () => {
					if (!File.Exists (setup.BINARY_DATABASE)) {
						MemMinkowskiVectorDB<short> db = new MemMinkowskiVectorDB<short> ();
						db.Build (setup.DATABASE);
						SpaceGenericIO.Save (setup.BINARY_DATABASE, db);
					}
				});
				break;

			case "VEC_UInt16":
				ExecuteMain (nick, setup, () => {
					if (!File.Exists (setup.BINARY_DATABASE)) {
						MemMinkowskiVectorDB<ushort> db = new MemMinkowskiVectorDB<ushort> ();
						db.Build (setup.DATABASE);
						SpaceGenericIO.Save (setup.BINARY_DATABASE, db);
					}
				});
				break;
			default:
				throw new ArgumentException ("Error unknown vector subtype " + stype);
			}
		}

		public static void Main (string[] args)
		{
			OptionSet ops = null;
			var setup = new IndexArgumentSetup ();
			string stype = null;

			ops = new OptionSet() {
				{"database=", "Database in its ascii format. It will create a file DB.dbname in the current directory", v => setup.DATABASE = v },
				{"queries=", "Queries in its ascii format", v => setup.QUERIES = v },
				{"stype=", "The type of the metric space. Valid names VEC, VEC_Int16, VEC_UInt16, VEC_Int8, VEC_UInt8, DOC, SEQ-ED, STR-ED, WIKTIONARY, COPHIR282", v => stype = v},
				{"qtype=", "Type of query, negative values should be integers and means for near neighbor search, positive values means for range search. Defaults to -30 (30NN)", v => setup.QARG = Double.Parse(v)},
				{"prefix=", "Experiment's prefix", v => setup.PREFIX = v},
				{"laesa=", "Run LAESA with the given pivot sizes (comma sep.)",	v => LoadList(v, setup.LAESA)},
				{"spa=", "Run Spaghetti with the given pivot sizes (comma sep.)",	v => LoadList(v, setup.SPA)},
				{"bnc=", "Run BNC-Inc with the given pivot sizes (comma sep.)",	v => LoadList(v, setup.BNC)},
				{"kvp=", "Run KVP with the given pivot sizes (comma sep.)",	v => LoadList(v, setup.KVP)},
				{"kvp-available=", "Run KVP with the given available pivots (defaults to 1024; zero means sqrt(n))", v => setup.KVP_Available = int.Parse(v)},
				{"ept=", "Run EPT with the given group sizes (comma sep.)", v => LoadList(v, setup.EPT)},
				{"milc=", "Run MILC* with the given number of indexes (comma sep.)", v => LoadList(v, setup.MILC)},
				{"lc=", "Run LC with the given block sizes (comma sep.)", v => LoadList(v, setup.LC)},
				{"sss=", "Run SSS with the given alpha list (comma sep.)", v => LoadList(v, setup.SSS)},
				{"sss-max=", "Run SSS with the given alpha list (comma sep.)", v => setup.SSS_max = int.Parse(v)},
				{"parameterless", "Enable parameterless indexes", v => setup.ExecuteParameterless = true},
				{"help|h", "Shows this help message", v => ops.WriteOptionDescriptions(Console.Out)},
				{"skip-search", v => setup.ExecuteSearch = false},
				{"cores=", v => setup.CORES = int.Parse(v)},
			};

			ops.Parse(args);
			if (setup.DATABASE == null) {
				throw new ArgumentNullException ("The database argument is mandatory");
			}
			if (setup.QUERIES == null) {
				throw new ArgumentNullException ("The queries argument is mandatory");
			}
			if (stype == null) {
				throw new ArgumentNullException ("The stype argument is mandatory");
			}
			if (stype.StartsWith("VEC")) {
				MainVEC (setup, stype);
			} else if (stype == "DOC") {
				MainDOC (setup);
			} else if (stype == "SEQ-ED") {
				MainSEQED (setup);
			} else if (stype == "STR-ED") {
				MainSTRED (setup);
			} else if (stype == "WIKTIONARY") {
				MainWIKTIONARY (setup);
			} else if (stype == "COPHIR282") {
			    MainCOPHIR(setup);
			} else {
				throw new ArgumentException (String.Format("Unknown space type {0}", stype));
			}
		}

		static void LoadList(string v, List<int> list) {
			foreach (var item in v.Split(',')) {
				list.Add (int.Parse (item));
			}
		}

		static void LoadList(string v, List<double> list) {
			foreach (var item in v.Split(',')) {
				list.Add (double.Parse (item));
			}
		}

	}
}

