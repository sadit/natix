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
//   Original filename: natix/test-simsearch/Main.cs
// 
using System;
using System.IO;
using natix;
using natix.CompactDS;
using natix.SimilaritySearch;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;
using Microsoft.CSharp;

namespace testsimsearch
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var main = new MainClass();
			main.TestColors();
			main.TestNasa();
			main.TestEnglish();
			main.TestLSH_Hamming();
			main.TestLSH_Audio();
		}

		public void TestColors ()
		{
			var name = "DB.colors.float";
			var sourcename = "dbs/vectors/colors/colors.ascii.header";
			MetricDB original = null;
			if (!File.Exists (name)) {
				Console.WriteLine ("compiling the database '{0}' -> '{1}'", sourcename, name);
				var db0 = new MinkowskiVectorSpace<float> ();
				db0.Build (sourcename);
				SpaceGenericIO.Save (name, db0, false);
				original = db0;
			}
			Console.WriteLine ("loading compiled version of the database '{0}'", name);
			var db1 = SpaceGenericIO.Load (name);
			if (original != null) {
				this.AssertEqualityDB (original, db1);
			}
			this.Test(name, db1, "queries-colors-256", 300, 32, 512);
		}

		public void TestNasa ()
		{
			var name = "DB.nasa.float";
			var sourcename = "dbs/vectors/nasa/nasa.ascii.header";
			MetricDB original = null;
			if (!File.Exists (name)) {
				Console.WriteLine ("compiling the database '{0}' -> '{1}'", sourcename, name);
				var db0 = new MinkowskiVectorSpace<float> ();
				db0.Build (sourcename);
				SpaceGenericIO.Save (name, db0, false);
				original = db0;
			}
			Console.WriteLine ("loading compiled version of the database '{0}'", name);
			var db1 = SpaceGenericIO.Load (name);
			if (original != null) {
				this.AssertEqualityDB (original, db1);
			}
			this.Test(name, db1, "queries-nasa-256", 300, 32, 512);
		}

		public void TestEnglish ()
		{
			var name = "DB.english";
			var sourcename = "dbs/strings/dictionaries/English.dic";
			MetricDB original = null;
			if (!File.Exists (name)) {
				Console.WriteLine ("compiling the database '{0}' -> '{1}'", sourcename, name);
				var db0 = new StringLevenshteinSpace<char>();
				// we need to read sequences manually since we are not reading sequences of numbers
				db0.StringParser = (string s) => s.ToCharArray();
				db0.Build (sourcename);
				SpaceGenericIO.Save (name, db0, false);
				original = db0;
			}
			Console.WriteLine ("loading compiled version of the database '{0}'", name);
			var db1 = (StringLevenshteinSpace<char>)SpaceGenericIO.Load (name);
			if (original != null) {
				this.AssertEqualityDB (original, db1);
			}
			db1.StringParser = (string s) => s.ToCharArray();
			this.Test(name, db1, "queries-english-256", 300, 32, 512);
		}

		public void AssertEqualityDB (MetricDB db0, MetricDB db1)
		{
			Console.WriteLine("Checking equality between original and saved databases");
			for (int i = 0; i < db0.Count; ++i) {
				var d = db0.Dist(db0[i], db1[i]);
				if (d != 0) {
					throw new Exception("=== ASSERTION ERROR: databases are not identical");
				}
			}
			Console.WriteLine("OK");
		}

		public void Test (string nick, MetricDB db, string queries, int num_centers, int num_perms, int num_refs)
		{
			var qstream = new QueryStream (queries);
			var reslist = new List<string> ();
			// Exhaustive search
			{
				Sequential seq = new Sequential ();
				seq.Build (db);
				var idxname = "Index.Sequential." + nick;
				IndexGenericIO.Save (idxname, seq);
				var resname = "Res." + idxname + "." + queries;
				if (!File.Exists (resname)) {
					Commands.Search (seq, qstream.Iterate (), new ShellSearchOptions (queries, idxname, resname));
				}
				reslist.Add (resname);
			}

			///
			/// The List of Clusters and variants
			/// 

			// LC_RNN
			reslist.Add (this.TestLC ("Index.LC_RNN." + nick, db, num_centers, new LC_RNN (), queries, qstream));
			// LC
			reslist.Add (this.TestLC ("Index.LC." + nick, db, num_centers, new LC (), queries, qstream));
			// LC_IRNN
			reslist.Add (this.TestLC ("Index.LC_IRNN." + nick, db, num_centers, new LC_IRNN (), queries, qstream));
			// LC_PRNN
			reslist.Add (this.TestLC ("Index.LC_PRNN." + nick, db, num_centers, new LC_PRNN (), queries, qstream));
			// LC_ParallelBuild
			reslist.Add (this.TestLC ("Index.LC_ParallelBuild." + nick, db, num_centers, new LC_ParallelBuild (), queries, qstream));

			/// 
			/// Permutation Based Indexes
			///

			// Permutations
			reslist.Add (this.TestPI ("Index.Perms." + nick, db, num_perms, new Perms (), queries, qstream));
			// Brief Index
			reslist.Add (this.TestPI ("Index.BinPerms." + nick, db, num_perms, new BinPerms (), queries, qstream));
			// BinPermsTwoBits
			reslist.Add (this.TestPI ("Index.BinPermsTwoBits." + nick, db, num_perms, new BinPermsTwoBit (), queries, qstream));
			///
			/// KNR
			///

			{
				KnrSeqSearch idx;
				var idxname = "Index.KnrSeqSearch." + nick;
				if (File.Exists (idxname)) {
					idx = (KnrSeqSearch)IndexGenericIO.Load (idxname);
				} else {
					Console.WriteLine ("** Starting construction of '{0}'", idxname);
					var knr = new KnrSeqSearch ();
					var sample = RandomSets.GetRandomSubSet (num_refs, db.Count);
					var refsdb = new SampleSpace ("", db, sample);
					var refsidx = new LC ();
					refsidx.Build (refsdb, refsdb.Count / 10, RandomSets.GetRandom());
					knr.Build (db, refsidx, 7);
					IndexGenericIO.Save (idxname, knr);
					idx = knr;
				}
				idx.MAXCAND = 1024;
				this.TestKNR(idx, idxname, queries, num_refs, reslist, (I) => I);
				Console.WriteLine ("==== Working on a permuted space");
				idxname = idxname + ".proximity-sorted";
				if (!File.Exists(idxname)) {
					idx = idx.GetSortedByPrefix();
					idx.MAXCAND = 1024;
					IndexGenericIO.Save(idxname, idx);
				} else {
					idx = (KnrSeqSearch)IndexGenericIO.Load(idxname);
				}
				this.TestKNR(idx, idxname, queries, num_refs, reslist, (I) => new PermutedIndex(I));
			}
			reslist.Add("--horizontal");
			Commands.Check(reslist);
		}

		public void TestKNR(KnrSeqSearch idx, string idxname, string queries, int num_refs, IList<string> reslist, Func<Index,Index> map)
		{
			// KnrSeqSearch
			var qstream = new QueryStream(queries);
			// PP-Index
			var resname = "Res." + idxname + "." + queries + ".PPIndex";
			var searchops = new ShellSearchOptions (queries, idxname, resname);
			if (!File.Exists (resname)) {
				Commands.Search (map(idx), qstream.Iterate (), searchops);
			}
			reslist.Add (resname);

			// Spearman Footrule
			resname = "Res." + idxname + "." + queries + ".SF";
			if (!File.Exists (resname)) {
				searchops = new ShellSearchOptions (queries, idxname, resname);
				Commands.Search (map(new KnrSeqSearchFootrule(idx)), qstream.Iterate (), searchops);
			}
			reslist.Add (resname);

			// Spearman Rho
			resname = "Res." + idxname + "." + queries + ".SR";
			if (!File.Exists (resname)) {
				searchops = new ShellSearchOptions (queries, idxname, resname);
				Commands.Search (map(new KnrSeqSearchSpearmanRho(idx)), qstream.Iterate (), searchops);
			}
			reslist.Add (resname);

			// Jaccard
			resname = "Res." + idxname + "." + queries + ".Jaccard";
			if (!File.Exists (resname)) {
				searchops = new ShellSearchOptions (queries, idxname, resname);
				Commands.Search (map(new KnrSeqSearchJaccard(idx)), qstream.Iterate (), searchops);
			}
			reslist.Add (resname);

			// RelMatches
			resname = "Res." + idxname + "." + queries + ".RelMatches";
				if (!File.Exists (resname)) {
				searchops = new ShellSearchOptions (queries, idxname, resname);
				Commands.Search (map(new KnrSeqSearchRelMatches(idx)), qstream.Iterate (), searchops);
			}
			reslist.Add (resname);

			// CNAPP
			reslist.Add(_Test("Index.CNAPP." + idxname, idx.DB, () => {
				var cnapp = new CNAPP();
				// cnapp.Build(idx, idx.K-2);
				cnapp.Build(idx, 1);
				return map(cnapp);
			}, queries));
		}

		public string TestPI(string idxname, MetricDB db, int num_perms, dynamic pi, string queries, QueryStream qstream)
		{
			var dbperms = RandomSets.GetRandomSubSet (num_perms, db.Count);
			dynamic idx;
			if (File.Exists(idxname)) {
				idx = IndexGenericIO.Load(idxname);
			} else {
				Console.WriteLine ("** Starting construction of '{0}'", idxname);
				pi.Build (db, new SampleSpace ("", db, dbperms));
				IndexGenericIO.Save (idxname, pi);
				idx = pi;
			}
			idx.MAXCAND = 1024;
			var resname = "Res." + idxname + "." + queries;
			if (!File.Exists (resname)) {
				Commands.Search (idx, qstream.Iterate (), new ShellSearchOptions (queries, idxname, resname));
			}
			return resname;
		}

		public string TestLC(string idxname, MetricDB db, int num_centers, dynamic lc, string queries, QueryStream qstream)
		{
			Index idx;
			if (File.Exists(idxname)) {
				idx = IndexGenericIO.Load(idxname);
			} else {
				Console.WriteLine ("** Starting construction of '{0}'", idxname);
				lc.Build (db, num_centers, SequenceBuilders.GetSeqXLB_SArray64(16));
				IndexGenericIO.Save (idxname, lc);
				idx = lc;
			}
			var resname = "Res." + idxname + "." + queries;
			if (!File.Exists (resname)) {
				Commands.Search (idx, qstream.Iterate (), new ShellSearchOptions (queries, idxname, resname));
			}
			return resname;
		}

		public void TestLSH_Hamming ()
		{
			var name = "hamming-files.list.bin";
			var source = "hamming-files.list";
			BinH8Space db0 = null;
			if (!File.Exists (name)) {
				db0 = new BinH8Space ();
				db0.Build (source);
				SpaceGenericIO.Save (name, db0, false);
			}
			var db = SpaceGenericIO.Load (name, false);
			if (db0 != null) {
				this.AssertEqualityDB (db0, db);
				db0 = null;
			}
			this.TestLSC_H8(db, name, "queries-hamming");
		}

		public void TestLSH_Audio ()
		{
			var name = "hamming-files.list.audio.bin";
			var source = "hamming-files.list";
			AudioSpace db0 = null;
			if (!File.Exists (name)) {
				db0 = new AudioSpace();
				db0.Build(source, 30, 3);
				SpaceGenericIO.Save (name, db0, false);
			}
			var db = SpaceGenericIO.Load (name, false);
			if (db0 != null) {
				this.AssertEqualityDB (db0, db);
				db0 = null;
			}
			this.TestLSC_H8(db, name, "queries-hamming");
		}

		public void TestLSC_H8(MetricDB db, string name, string queries)
		{
			var reslist = new List<string> ();
			// LSC_H1
			reslist.Add(_Test("Index.LSC_H8." + name, db, () => {
				var idx = new LSC_H8();
				idx.Build(db, 20);
				return idx;
			}, queries));
			// MLSC_H8
			reslist.Add(_Test("Index.MLSC_H8." + name, db, () => {
				var idx = new MLSC_H8();
				idx.Build(db, 20, 4);
				return idx;
			}, queries));
			// LSC_Cyclic8
			reslist.Add(_Test("Index.LSC_Cyclic8." + name, db, () => {
				var idx = new LSC_CyclicH8();
				idx.Build(db, 20);
				return idx;
			}, queries));
			// MLSC_Cyclic8
			reslist.Add(_Test("Index.MLSC_Cyclic8." + name, db, () => {
				var idx = new MLSC_CyclicH8();
				idx.Build(db, 20, 4);
				return idx;
			}, queries));
			reslist.Add("--horizontal");
			Commands.Check(reslist);
		}

		string _Test(string idxname, MetricDB db, Func<Index> new_index, string queries)
		{
			QueryStream qstream = new QueryStream(queries);
			Index idx;
			if (File.Exists(idxname)) {
				idx = IndexGenericIO.Load(idxname);
			} else {
				Console.WriteLine ("** Starting construction of '{0}'", idxname);
				idx = new_index();
				IndexGenericIO.Save (idxname, idx);
			}
			var resname = "Res." + idxname + "." + queries;
			if (!File.Exists (resname)) {
				Commands.Search (idx, qstream.Iterate (), new ShellSearchOptions (queries, idxname, resname));
			}
			return resname;
		}
	}
}
