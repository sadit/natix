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
using Newtonsoft.Json;

using natix;
using natix.SimilaritySearch;

namespace ApproxIndexes
{
	class Indexes
	{
		public static string GetResultName(string nick, string idxname, string queries, double qarg, string suffix)
		{
			if (suffix.Length > 0) {
				suffix = "." + suffix;
			}
			return String.Format(
				"{0}/Res.{1}.{2}.qarg={3}{4}.json",
				nick,
				Path.GetFileName(idxname),
				Path.GetFileName(queries),
				qarg,
				suffix
			);
		}

		public static void PerformSearch(string resname, Index idx, string nick, string idxname, string queries, double qarg)
		{
			var ops = new ShellSearchOptions(queries, idxname, resname);
			var qstream = new QueryStream (queries, qarg);
			Commands.Search (idx, qstream.Iterate(), ops);
			GC.Collect ();
		}

		public static void SaveConstructionTime(string idxname, long elapsed_ticks, long numdistances)
		{
			var outname = idxname + ".construction-time.json";
			var seconds = TimeSpan.FromTicks(elapsed_ticks).TotalSeconds;
			var msg = JsonConvert.SerializeObject (new Dictionary<string, object> () {
				{"Seconds", seconds},
				{"Distances", numdistances}
			}, Formatting.Indented);
			File.WriteAllText (outname, msg);
		}

		public static string Execute(IndexArgumentSetup setup, string nick, string idxname, Func<MetricDB, Index> create)
		{
			var resname = GetResultName (nick, idxname, setup.QUERIES,  setup.QARG, "");
			if (File.Exists (resname)) {
				return resname;
			}
			MetricDB db = SpaceGenericIO.Load (setup.BINARY_DATABASE);
			Index idx;

			if (!File.Exists (idxname)) {
				Console.WriteLine ("*** creating index {0}", idxname);
				var s = DateTime.Now.Ticks;
				var c = db.NumberDistances;
				idx = create (db);
				SaveConstructionTime (idxname, DateTime.Now.Ticks - s, db.NumberDistances - c);
				IndexGenericIO.Save (idxname, idx);
			} else {
				idx = IndexGenericIO.Load (idxname);
			}

			if (!File.Exists (resname)) {
				PerformSearch (resname, idx, nick, idxname, setup.QUERIES, setup.QARG);
			}
			return resname;
		}

		public static void ExecuteKNRSEQ(IndexArgumentSetup setup, string nick, int numrefs, int k, double maxcand_ratio, List<string> resnamelist)
		{
			var idxname = String.Format ("{0}/Index.knrseq-{1}-{2}", nick, numrefs, k);
			MetricDB db = SpaceGenericIO.Load (setup.BINARY_DATABASE);
			Index idx;
			var queries = setup.QUERIES;
			var suffix = "";

			if (!File.Exists (idxname)) {
				Console.WriteLine ("*** creating index {0}", idxname);
				var s = DateTime.Now.Ticks;
				var c = db.NumberDistances;
				var IDX = new KnrSeqSearch ();
				var refsDB = new SampleSpace("", db, numrefs);
				var refsIDX = new EPTable ();
				refsIDX.Build(refsDB, 4, (_db, _rand) => new EPListOptimizedA(_db, 4, _rand));
				if (k == 0) {
					k = KnrEstimateParameters.EstimateKnrEnsuringSharedNeighborhoods (db, refsIDX, (int)Math.Abs (setup.QARG));
					suffix = String.Format ("estimated-K={0}.", k);
				}
				IDX.Build (db, refsIDX, k, int.MaxValue);
				SaveConstructionTime (idxname, DateTime.Now.Ticks - s, db.NumberDistances - c);
				IndexGenericIO.Save (idxname, IDX);
				idx = IDX;
			} else {
				idx = IndexGenericIO.Load (idxname);
				if (k == 0) {
					var _idx = idx as KnrSeqSearch;
					suffix = String.Format ("estimated-K={0}.", _idx.K);
				}
			}
			string resname;
			// PPIndex
			resname = GetResultName (nick, idxname, queries, setup.QARG, String.Format(suffix + "maxcand={0}.PPI", maxcand_ratio));
			resnamelist.Add(resname);
			if (!File.Exists (resname)) {
				var knr = idx as KnrSeqSearch;
				knr.MAXCAND = (int)(idx.DB.Count * maxcand_ratio);
				PerformSearch (resname, knr, nick, idxname, queries, setup.QARG);
			}
			// KnrSeqSearchCosine
			resname = GetResultName (nick, idxname, queries, setup.QARG, String.Format(suffix + "maxcand={0}.COS", maxcand_ratio));
			resnamelist.Add(resname);
			if (!File.Exists (resname)) {
				var knr = new KnrSeqSearchCosine(idx as KnrSeqSearch);
				knr.MAXCAND = (int)(idx.DB.Count * maxcand_ratio);
				PerformSearch (resname, knr, nick, idxname, queries, setup.QARG);
			}
			// KnrSeqSearchFootrule
			resname = GetResultName (nick, idxname, queries, setup.QARG, String.Format(suffix + "maxcand={0}.FOOTRULE", maxcand_ratio));
			resnamelist.Add(resname);
			if (!File.Exists (resname)) {
				var knr = new KnrSeqSearchFootrule(idx as KnrSeqSearch);
				knr.MAXCAND = (int)(idx.DB.Count * maxcand_ratio);
				PerformSearch (resname, knr, nick, idxname, queries, setup.QARG);
			}
			// KnrSeqSearchJaccLCS
			resname = GetResultName (nick, idxname, queries, setup.QARG, String.Format(suffix + "maxcand={0}.JACCLCS", maxcand_ratio));
			resnamelist.Add(resname);
			if (!File.Exists (resname)) {
				var knr = new KnrSeqSearchJaccLCS(idx as KnrSeqSearch);
				knr.MAXCAND = (int)(idx.DB.Count * maxcand_ratio);
				PerformSearch (resname, knr, nick, idxname, queries, setup.QARG);
			}
			// KnrSeqSearchLCSv3
			resname = GetResultName (nick, idxname, queries, setup.QARG, String.Format(suffix + "maxcand={0}.LCSv3", maxcand_ratio));
			resnamelist.Add(resname);
			if (!File.Exists (resname)) {
				var knr = new KnrSeqSearchLCSv3(idx as KnrSeqSearch);
				knr.MAXCAND = (int)(idx.DB.Count * maxcand_ratio);
				PerformSearch (resname, knr, nick, idxname, queries, setup.QARG);
			}
			// NAPP
			foreach (var ksearch in setup.KNR_KSEARCH) {
				var knr = new NAPP(idx as KnrSeqSearch);
				knr.MAXCAND = (int)(idx.DB.Count * maxcand_ratio);
				resname = GetResultName (nick, idxname, queries, setup.QARG, String.Format(suffix + "maxcand={0}.NAPP.ksearch={1}", maxcand_ratio, ksearch));
				resnamelist.Add(resname);
				if (!File.Exists (resname)) {
					PerformSearch (resname, knr, nick, idxname, queries, setup.QARG);
				}
			}
		}

		// sisap 2012 version
		public static string ExecuteAPG_Greedy(IndexArgumentSetup setup, string nick, int neighbors, int restarts)
		{
			var idxname = String.Format ("{0}/Index.APG-Greedy.neighbors={1}-restarts={2}", nick, neighbors, restarts);
			return Execute (setup, nick, idxname, (db) => {
				var IDX = new ApproxGraph ();
				IDX.Build (db, (short)neighbors, (short)restarts);
				return IDX;
			});
		}

		public static string ExecuteLocalSearchBestFirst(IndexArgumentSetup setup, string nick, int neighbors, int restarts)
		{
			var idxname = String.Format ("{0}/Index.LocalSearchBestFirst.neighbors={1}-restarts={2}", nick, neighbors, restarts);
			return Execute (setup, nick, idxname, (db) => {
				var idx = new LocalSearchBestFirst ();
				idx.Build (db, neighbors, restarts);
				return idx;
			});
		}

		public static string ExecuteAPG_IS(IndexArgumentSetup setup, string nick, int neighbors, int restarts)
		{
			var idxname = String.Format ("{0}/Index.APG-IS.neighbors={1}-restarts={2}", nick, neighbors, restarts);
			return Execute (setup, nick, idxname, (db) => {
				var IDX = new APG_IS ();
				IDX.Build (db, neighbors, restarts);
				return IDX;
			});
		}

		public static string ExecuteAPG_OPT(IndexArgumentSetup setup, string nick, string queries, int neighbors)
		{
			var idxname = String.Format ("{0}/Index.APG-Tabu.neighbors={1}", nick, neighbors);
			return Execute (setup, nick, idxname, (db) => {
				var IDX = new APG_OPT ();
				IDX.Build (db, neighbors);
				return IDX;
			});
		}

		public static string ExecuteLocalSearchRestarts(IndexArgumentSetup setup, string nick, string dbname, string queries, int neighbors)
		{
			var idxname = String.Format ("{0}/Index.LocalSearchRestarts.neighbors={1}", nick, neighbors);
			return Execute (setup, nick, idxname, (db) => {
				var IDX = new LocalSearchRestarts ();
				IDX.Build (db, neighbors);
				return IDX;
			});
		}

		public static string ExecuteLocalSearchBeam(IndexArgumentSetup setup, string nick, int beamsize, int neighbors)
		{
			var idxname = String.Format ("{0}/Index.LocalSearchBeam.beamsize={1}-neighbors={2}", nick, beamsize, neighbors);
			return Execute (setup, nick, idxname, (db) => {
				var IDX = new LocalSearchBeam ();
				IDX.Build (db, neighbors, beamsize);
				return IDX;
			});
		}

		public static string ExecuteLocalSearchMontecarloBeam(IndexArgumentSetup setup, string nick, int beamsize, int neighbors)
		{
			var idxname = String.Format ("{0}/Index.LocalSearchMontecarloBeam.beamsize={1}-neighbors={2}", nick, beamsize, neighbors);
			return Execute (setup, nick, idxname, (db) => {
				var IDX = new LocalSearchMontecarloBeam ();
				IDX.Build (db, neighbors, beamsize);
				return IDX;
			});
		}

		public static void ExecuteMultiNeighborhoodHash(IndexArgumentSetup setup, string nick, double expected_recall, int max_instances, List<string> resnameList)
		{
			var idxname = String.Format ("{0}/Index.MultiNeighborhoodHash.max_instances={1}-qarg={2}-expected-recall={3}", nick, max_instances, setup.QARG, expected_recall);
			var resname = Execute (setup, nick, idxname, (db) => {
				var parameters = MultiNeighborhoodHash.EstimateParameters (db, max_instances, (int)Math.Abs (setup.QARG), expected_recall, 96);
				/*if (parameters.NumberOfInstances == 1) {
					idx = parameters.Index;
				} else {*/
				var IDX = new MultiNeighborhoodHash ();
				IDX.Build (db, parameters);
				return IDX;
			});
			resnameList.Add (resname);

			resname = GetResultName (nick, idxname, setup.QUERIES, setup.QARG, "Adaptive");
			resnameList.Add (resname);
	
			if (!File.Exists (resname)) {
				var idx = IndexGenericIO.Load (idxname);
				idx = new AdaptiveNeighborhoodHash(idx as MultiNeighborhoodHash);
				PerformSearch (resname, idx, nick, idxname, setup.QUERIES, setup.QARG);
			}
		}

		public static string ExecuteLSHFloatVector(IndexArgumentSetup setup, string nick, int num_indexes, int width)
		{
			var idxname = String.Format ("{0}/Index.LSH.{1}-{2}", nick, num_indexes, width);
			return Execute (setup, nick, idxname, (db) => {
				var lsh = new MLSH_FloatVectorL2 ();
				lsh.Build (db, width, num_indexes);			
				return lsh;
			});
		}

		public static string ExecuteSeq(IndexArgumentSetup setup, string nick, string dbname, string queries)
		{
			var idxname = String.Format ("{0}/Index.Seq", nick);
			return Execute (setup, nick, idxname, (db) => {
				var seq = new Sequential ();
				seq.Build (db);
				return seq;
			});
		}
	}
}