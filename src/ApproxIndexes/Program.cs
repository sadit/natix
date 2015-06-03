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
using System.Collections;
using System.Collections.Concurrent;
using NDesk.Options;

using natix;
using natix.SimilaritySearch;

namespace ApproxIndexes
{
	class MainClass
	{
		public static void ExecuteMain(string nick, IndexArgumentSetup setup, Action prepare_db)
		{
			var dbname = String.Format ("DB.{0}", Path.GetFileName(setup.DATABASE));
			setup.BINARY_DATABASE = dbname;

			prepare_db ();

			// It is required to be already on memory at this point. The reason is to avoid the loading of several instances
			// of the same database
			SpaceGenericIO.Load (setup.BINARY_DATABASE); 

			var arglist = new System.Collections.Concurrent.ConcurrentQueue<String> ();
			arglist.Enqueue ("--save");
			arglist.Enqueue (String.Format ("Tab.ApproxIndexes.{0}.{1}.qarg={2}.json", nick, Path.GetFileName (setup.QUERIES), setup.QARG));
				/*var arglist = new List<string> () {
				"--save",
				String.Format("Tab.{0}.{1}.qarg={2}.json", nick, Path.GetFileName(setup.QUERIES), setup.QARG)
				*/

			arglist.Enqueue (Indexes.ExecuteSeq (setup, nick));
			var actionlist = new List<Action> ();
			// arglist.Add (Indexes.ExecuteSATApprox (setup, nick));
			// arglist.Add (Indexes.ExecuteSATForest (setup, nick));

			foreach (var max_instances in setup.NeighborhoodHash_MaxInstances) {
				foreach (var expected_recall in setup.NeighborhoodHash_ExpectedRecall) {
					var _max_instances = max_instances;
					var _expected_recall = expected_recall;
					actionlist.Add (() => {
						var reslist = Indexes.ExecuteMultiNeighborhoodHash (setup, nick, _expected_recall, _max_instances);
						foreach (var res in reslist) {
							arglist.Enqueue(res);
						}
					});
				}
			}

			foreach (var numrefs in setup.KNR_NUMREFS) {
				foreach (var k in setup.KNR_KBUILD) {
					foreach (var maxcand_ratio in setup.KNR_MAXCANDRATIO) {
						var _numrefs = numrefs;
						var _k = k;
						var _maxcand_ratio = maxcand_ratio;
						actionlist.Add (() => {
							var reslist = Indexes.ExecuteKNRSEQ (setup, nick, _numrefs, _k, _maxcand_ratio);
							foreach (var res in reslist) {
								arglist.Enqueue(res);
							}
						});
					}
				}
			}

//			actionlist.Add (() => {
//				var resname = Indexes.ExecuteAPG_OptTabuSatNeighborhood (setup, nick);
//				arglist.Enqueue(resname);
//			});
//
//			actionlist.Add (() => {
//				var resname = Indexes.ExecuteAPG_OptTabuSatNeighborhoodMontecarloStart(setup, nick);
//				arglist.Enqueue(resname);
//			});


			foreach (var neighbors in setup.OPTSEARCH_NEIGHBORS) {
				// arglist.Add (Indexes.ExecuteLocalSearchRestarts (setup, nick, dbname, setup.QUERIES, neighbors));
				// arglist.Add (Indexes.ExecuteLocalSearchBestFirst (setup, nick, dbname, setup.QUERIES, neighbors));
				var _neighbors = neighbors;
			
				actionlist.Add (() => {
					var resname = Indexes.ExecuteApproxGraphOptRestartsIS(setup, nick, _neighbors);
					arglist.Enqueue(resname);
				});

				actionlist.Add (() => {
					var resname = Indexes.ExecuteApproxGraphOptRandomRestarts(setup, nick, _neighbors);
					arglist.Enqueue(resname);
				});

//				actionlist.Add (() => {
//					var resname = Indexes.ExecuteApproxGraphOptSimplerOptRandomRestarts(setup, nick, _neighbors);
//					arglist.Enqueue(resname);
//				});

				actionlist.Add (() => {
					var resname = Indexes.ExecuteMetricGraphGreedy(setup, nick, _neighbors);
					arglist.Enqueue(resname);
				});

				foreach (var restarts in setup.OPTSEARCH_RESTARTS) {
					var _restarts = restarts;
					actionlist.Add (() => {
						var resname = Indexes.ExecuteApproxGraphIS(setup, nick, _neighbors, _restarts);
						arglist.Enqueue(resname);
					});
//					actionlist.Add (() => {
//						var resname = Indexes.ExecuteApproxGraph(setup, nick, _neighbors, _restarts);
//						arglist.Enqueue(resname);
//					});
				}

				actionlist.Add (() => {
					var resname = Indexes.ExecuteLocalSearchGallopingBeam(setup, nick, _neighbors);
					arglist.Enqueue(resname);
				});

				foreach (var beamsize in setup.OPTSEARCH_BEAMSIZE) {
					var _beamsize = beamsize;
					actionlist.Add (() => {
						var resname = Indexes.ExecuteLocalSearchBeam(setup, nick, _beamsize, _neighbors);
						arglist.Enqueue(resname);
					});

//					actionlist.Add (() => {
//						var resname = Indexes.ExecuteLocalSearchMontecarloBeam(setup, nick, _beamsize, _neighbors);
//						arglist.Enqueue(resname);
//					});
				}
			}

			foreach (var numInstances in setup.LSHFloatVector_INDEXES) {
				foreach (var numSamples in setup.LSHFloatVector_SAMPLES) {
					var _numInstances = numInstances;
					var _numSamples = numSamples;
					actionlist.Add (() => {
						var resname = Indexes.ExecuteLSHFloatVector (setup, nick, _numInstances, _numSamples);
						arglist.Enqueue(resname);
					});
				}
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
				{"neighborhoodhash-instances=", "Run NeighborhoodHash with the given maximum number of hashes", v => LoadList(v, setup.NeighborhoodHash_MaxInstances)},
				{"neighborhoodhash-recall=", "Create NeighborhoodHash to achieve the given recalls", v => LoadList(v, setup.NeighborhoodHash_ExpectedRecall)},

				{"optsearch-beamsize=", "Run LOCALSEARCH with the given list of beam sizes", v => LoadList(v, setup.OPTSEARCH_BEAMSIZE)},
				{"optsearch-restarts=", "Run LOCALSEARCH/APG with the given list of restarts", v => LoadList(v, setup.OPTSEARCH_RESTARTS)},
				{"optsearch-neighbors=", "Run LOCALSEARCH/APG with the given list of outgoing degrees (neighbors)", v => LoadList(v, setup.OPTSEARCH_NEIGHBORS)},

				{"knr-numrefs=", "Run KnrSeq methods with the given list of # refs (comma sep.)",	v => LoadList(v, setup.KNR_NUMREFS)},
				{"knr-kbuild=", "Create KnrSeq indexes with the given list of near references (comma sep.)",	v => LoadList(v, setup.KNR_KBUILD)},
				{"napp-ksearch=", "Search KnrSeq indexes with the given list of near references (comma sep.)",	v => LoadList(v, setup.KNR_KSEARCH)},
				{"knr-maxcand=", "Run KnrSeq methods with the given list of maxcand (comma sep., ratio)",	v => LoadList(v, setup.KNR_MAXCANDRATIO)},
				{"lsh-instances=", "A list of instances to for LSH_FloatVectors (comma sep.)", v=> LoadList(v, setup.LSHFloatVector_INDEXES)},
				{"lsh-width=", "A list of widths for LSH_FloatVectors (comma sep.)", v=> LoadList(v, setup.LSHFloatVector_SAMPLES)},
				//{"parameterless", "Enable parameterless indexes", v => setup.ExecuteParameterless = true},
				{"skip-search", v => setup.ExecuteSearch = false},
				{"cores=", v => setup.CORES = int.Parse(v)},
				{"help|h", "Shows this help message", v => {
						ops.WriteOptionDescriptions(Console.Out);
						System.Environment.Exit(0);
					}
				}
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
