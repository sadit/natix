﻿//
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

using natix;
using natix.SimilaritySearch;
using Newtonsoft.Json;

namespace ExactIndexes
{
	class Indexes
	{
		public static void PerformSearch(string resname, Index idx, string idxname, IndexArgumentSetup setup)
		{
			if (setup.ExecuteSearch) {
				Console.WriteLine ("======= Searching {0}", resname);
				var ops = new ShellSearchOptions (setup.QUERIES, idxname, resname);
				var qstream = new QueryStream (setup.QUERIES, setup.QARG);
				Commands.Search (idx, qstream.Iterate (), ops);
				GC.Collect ();
			}
		}

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

		public static ANNI CreateANNI(string dbname, int expected_k)
		{
			MetricDB db = SpaceGenericIO.Load (dbname);
			ANNI ilc = new ANNI ();
			var annisetup = new ANNISetup (db.Count, expected_k);
			ilc.Build (db, annisetup);
			return ilc;
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
			var resname = GetResultName (nick, idxname, setup.QUERIES, setup.QARG, "");
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
			PerformSearch (resname, idx, idxname, setup);
			return resname;
		}

		public static List<string> ExecuteNANNI(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.NANNI", nick);
			var reslist = new List<string> ();
			var resname = Execute (setup, nick, idxname, (db) => {
				var nilc = new NANNI ();
				nilc.Build (CreateANNI (setup.BINARY_DATABASE, (int)Math.Abs (setup.QARG)));
				return nilc;
			});

			reslist.Add (resname);
//			resname = Execute (setup, nick, idxname + "S", (db) => {
//				var nilc = IndexGenericIO.Load(idxname);
//				return nilc;
//			});
//			reslist.Add (resname);
			return reslist;
		}

		public static string ExecuteTNANNI(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.TNANNI", nick);
			var resname = Execute (setup, nick, idxname, (db) => {
				var nilc = new XNANNI ();
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				// var annisetup = new ANNISetup(new PivotSelectorRandom(db.Count), (int)Math.Abs(setup.QARG), 0.001, 128, 64);
				nilc.Build (db, annisetup, false);
				return nilc;
			});

			return resname;
		}

		public static string ExecuteDNANNI(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.DNANNI", nick);
			var resname = Execute (setup, nick, idxname, (db) => {
				var nilc = new XNANNI ();
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				// var annisetup = new ANNISetup(new PivotSelectorRandom(db.Count), (int)Math.Abs(setup.QARG), 0.001, 128, 64);
				nilc.Build (db, annisetup, true);
				return nilc;
			});

			return resname;
		}

		public static string ExecuteFANNI(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.FANNI", nick);
			var resname = Execute (setup, nick, idxname, (db) => {
				var nilc = new XNANNI ();
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				// var annisetup = new ANNISetup(new PivotSelectorRandom(db.Count), (int)Math.Abs(setup.QARG), 0.001, 128, 64);
				nilc.Build (db, annisetup, false);
				return nilc;
			});

			return resname;
		}

		public static string ExecuteTMANNI(IndexArgumentSetup setup, string nick, int num_indexes)
		{
			var idxname = String.Format ("{0}/Index.TMANNI.{1}", nick, num_indexes);		
			return Execute (setup, nick, idxname, (db) => {
				var milc = new XMANNI ();
				// var annisetup = new ANNISetup(new PivotSelectorRandom(db.Count), (int)Math.Abs(setup.QARG), 0.001, 512, 64);
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				milc.Build (db, annisetup, num_indexes, false);
				return milc;
			});
		}

		public static string ExecuteDMANNI(IndexArgumentSetup setup, string nick, int num_indexes)
		{
			var idxname = String.Format ("{0}/Index.DMANNI.{1}", nick, num_indexes);
			return Execute (setup, nick, idxname, (db) => {
				var milc = new XMANNI ();
				// var annisetup = new ANNISetup(new PivotSelectorRandom(db.Count), (int)Math.Abs(setup.QARG), 0.001, 512, 64);
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				milc.Build (db, annisetup, num_indexes, true);
				return milc;
			});
		}


		public static string ExecuteMANNI(IndexArgumentSetup setup, string nick, int num_indexes)
		{
			var idxname = String.Format ("{0}/Index.MANNI.{1}", nick, num_indexes);		
			return Execute (setup, nick, idxname, (db) => {
				var milc = new MANNI ();
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				milc.Build (db, annisetup, num_indexes, setup.CORES);
				return milc;
			});
		}

		public static string ExecuteMANNIv2(IndexArgumentSetup setup, string nick, int num_indexes)
		{
			var idxname = String.Format ("{0}/Index.MANNIv2.{1}", nick, num_indexes);		
			return Execute (setup, nick, idxname, (db) => {
				var milc = new MANNIv2 ();
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				milc.Build (db, annisetup, num_indexes, setup.CORES);
				return milc;
			});
		}

		public static string ExecuteMANNIv3(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.MANNIv3", nick);
			return Execute (setup, nick, idxname, (db) => {
				var milc = new FANNI ();
				var annisetup = new ANNISetup(db.Count, (int)Math.Abs(setup.QARG));
				milc.Build (db, annisetup);
				return milc;
			});
		}

		public static string ExecuteEPTA(IndexArgumentSetup setup, string nick, int numgroups)
		{
			var idxname = String.Format ("{0}/Index.EPTA.{1}", nick, numgroups);
			return Execute (setup, nick, idxname, (db) => {
				EPTable eptable = new EPTable ();
				eptable.Build (db, numgroups, (_db, rand) => new EPListOptimizedA (db, numgroups, rand), setup.CORES);
				return eptable;
			});
		}

		public static string ExecuteEPTB(IndexArgumentSetup setup, string nick, int numgroups)
		{
			var idxname = String.Format ("{0}/Index.EPTB.{1}", nick, numgroups);
			return Execute (setup, nick, idxname, (db) => {
				EPTable eptable = new EPTable ();
				eptable.Build (db, numgroups, (_db, rand) => new EPListOptimizedB (db, numgroups, rand), setup.CORES);
				return eptable;
			});
		}

		public static string ExecuteEPT(IndexArgumentSetup setup, string nick, int numgroups)
		{
			var idxname = String.Format ("{0}/Index.EPT.{1}", nick, numgroups);
			return Execute (setup, nick, idxname, (db) => {
				EPTable eptable = new EPTable ();
				double beta = 0.8;
				eptable.Build (db, numgroups,
				               (_db, rand) => new EPListOptimized (db, numgroups, rand, 3000, beta), setup.CORES);
				return eptable;
			});
		}

		public static string ExecuteLAESA(IndexArgumentSetup setup, string nick, int numpivs)
		{
			var idxname = String.Format ("{0}/Index.LAESA.{1}", nick, numpivs);
			return Execute (setup, nick, idxname, (db) => {
				LAESA laesa = new LAESA ();
				laesa.Build (db, numpivs, setup.CORES);
				return laesa;
			});
		}

		public static string ExecuteSpaghetti(IndexArgumentSetup setup, string nick, int numpivs)
		{
			var idxname = String.Format ("{0}/Index.Spaghetti.{1}", nick, numpivs);
			return Execute (setup, nick, idxname, (db) => {
				Spaghetti spa = new Spaghetti ();
				spa.Build (db, numpivs);
				return spa;
			});
		}

		public static string ExecuteKVP(IndexArgumentSetup setup, string nick, int k, int available_pivs)
		{
			MetricDB db = SpaceGenericIO.Load (setup.BINARY_DATABASE);
			if (available_pivs == 0) {
				available_pivs = (int)Math.Sqrt (db.Count);
			}
			var idxname = String.Format ("{0}/Index.KVP.{1}-{2}", nick, k, available_pivs);

			return Execute (setup, nick, idxname, (_db) => {
				KVP kvp = new KVP ();
				kvp.Build (db, k, available_pivs);
				return kvp;
			});
		}

		public static string ExecuteSSS(IndexArgumentSetup setup, string nick, double alpha, int maxPivs)
		{
			var idxname = String.Format ("{0}/Index.SSS.{1}", nick, alpha);
			return Execute (setup, nick, idxname, (db) => {
				SSS sss = new SSS ();
				sss.Build (db, alpha, maxPivs, setup.CORES);
				return sss;
			});
		}

		public static string ExecuteBNCInc(IndexArgumentSetup setup, string nick, int numPivs)
		{
			var idxname = String.Format ("{0}/Index.BNCInc.{1}", nick, numPivs);
			return Execute (setup, nick, idxname, (db) => {
				BNCInc bncinc = new BNCInc ();
				bncinc.Build (db, numPivs);
				return bncinc;
			});
		}

		public static string ExecuteSATRandom(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.SAT-Random", nick);
			return Execute (setup, nick, idxname, (db) => {
				SAT_Random sat = new SAT_Random ();
				sat.Build (db, RandomSets.GetRandom());
				return sat;
			});
		}

		public static string ExecuteSATDistal(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.SAT-Distal", nick);
			return Execute (setup, nick, idxname, (db) => {
				var sat = new SAT_Distal ();
				sat.Build (db, RandomSets.GetRandom());
				return sat;
			});
		}

		public static string ExecuteSAT(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.SAT-Legacy", nick);
			return Execute (setup, nick, idxname, (db) => {
				var sat = new SAT ();
				sat.Build (db, RandomSets.GetRandom());
				return sat;
			});
		}

		public static string ExecuteLC(IndexArgumentSetup setup, string nick, int bsize)
		{
			var idxname = String.Format ("{0}/Index.LC-{1}", nick, bsize);
			return Execute (setup, nick, idxname, (db) => {
				var lc = new LC ();
				lc.Build (db, bsize, RandomSets.GetRandom());
				return lc;
			});
		}

		public static string ExecuteVPT(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.VPT", nick);
			return Execute (setup, nick, idxname, (db) => {
				var vpt = new VPT ();
				vpt.Build (db, RandomSets.GetRandom());
				return vpt;
			});
		}

		public static string ExecuteVPTX(IndexArgumentSetup setup, string nick)
		{
			var idxname = String.Format ("{0}/Index.VPTX", nick);
			return Execute (setup, nick, idxname, (db) => {
				var vpt = new VPTX ();
				vpt.Build (db, RandomSets.GetRandom());
				return vpt;
			});
		}

		public static string ExecuteSeq(IndexArgumentSetup setup, string nick)
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

