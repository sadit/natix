//
//  Copyright 2014  Luis Guillermo Ruiz Velázquez
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
using System.Collections.Generic;
using natix;
using natix.SimilaritySearch;
//using UtilsBasicIndex;
using System.IO;

namespace VPForest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Uso: VPForest db_file queries_file db_name dim tau
			string db_file="DB.colors";
			//string db_file="/home/memo/Descargas/db/colors/DB-colors.save";
			string queries_file="colors.queries";
			double querie_arg=.07;
			string query_type="Range";
			string dbname="colors";		
			int dim=112;
			double tau=.07;
			//IList<float[]> queries=new List<float[]>();

			if (args.Length!=0 )
			{
				db_file=args[0];
				queries_file=args[1];
				querie_arg = Convert.ToDouble(args[2]);
				query_type = args [3];
				dbname = args [4];
				dim = Convert.ToInt32 (args [5]);
				if (args.Length == 7)
					tau = Convert.ToDouble (args [6]);

			}

			// Leer DB
			if (!File.Exists (db_file)) {
				MemMinkowskiVectorDB<float> _db = new MemMinkowskiVectorDB<float> ();
				_db.Build (dbname+".ascii.header");
				SpaceGenericIO.Save (db_file, _db);
			}
			MetricDB DB;
			DB=SpaceGenericIO.Load(db_file,true);
			Console.WriteLine("DB Loaded size:{0}",DB.Count);



			int[] J={1,2,4,8,16}; // groups
			int [] I={1}; // not used
			foreach (int i in I)
			{
				foreach (int j in J)
				{
					int pivspergrp=0;

					// Crear índice VP-forest
					//Console.WriteLine("Building Forest m:{0}",i/10d);
					string VPF_file = "VP-Forest-"+dbname+"-Tau-" + tau + ".idx";
					VP_Forest VPF_Search;
					if (!File.Exists (VPF_file)) {
						Chronos chr_time = new Chronos ();
						chr_time.Start ();
						VPF_Search = new VP_Forest (DB, _tau: tau);
						chr_time.End ();
						File.AppendAllText("index-"+dbname+"-construction-speed-VP-Forest.csv", string.Format("{0} {1}{2}",tau,chr_time.AccTime,Environment.NewLine));
						VPF_Search.Save (new BinaryWriter (File.OpenWrite (VPF_file)));
					} else {
						VPF_Search = new VP_Forest ();
						VPF_Search.Load (new BinaryReader(File.OpenRead(VPF_file)));
					}
				

					// indice secuencial
					Sequential Seq=new Sequential();
					Seq.Build(DB);

					// índices EPT
					EPTable eptable_rnd400=new EPTable();	// 400 pivots / group
					EPTable eptable_rnd100=new EPTable(); 	// 100 pivots / group
					EPTable eptable_rnd8=new EPTable();		// 8 pivots / group
					EPTable eptable_rnd32=new EPTable();	// 32 pivots / group
					EPTable eptable_opt=new EPTable();

					// Construye los índices EPT
					Chronos chr_ept;
					string ept_file = "ept-opt-" + dbname + "-grps-" + j + ".idx";
					if (!File.Exists (ept_file)) {
						chr_ept = new Chronos ();
						chr_ept.Start ();
						eptable_opt.Build (DB, j, (MetricDB _db, Random seed) => new EPListOptimized (DB, j,seed, 1000, .8), 1);
						chr_ept.End ();
						File.AppendAllText ("index-" + dbname + "-construction-speed-ept.csv", string.Format ("EPT-opt {0} {1}{2}", j, chr_ept.AccTime, Environment.NewLine));
						eptable_opt.Save (new BinaryWriter (File.OpenWrite (ept_file)));
					} else {
						eptable_opt.Load (new BinaryReader (File.OpenRead (ept_file)));
					}

					ept_file = "ept-rnd100-" + dbname + "-grps-" + j + ".idx";
					if (!File.Exists (ept_file)) {
						chr_ept = new Chronos ();
						chr_ept.Start ();
						eptable_rnd100.Build (DB, j);
						chr_ept.End ();
						File.AppendAllText ("index-" + dbname + "-construction-speed-ept.csv", string.Format ("EPT-rnd100 {0} {1}{2}", j, chr_ept.AccTime, Environment.NewLine));
						eptable_rnd100.Save (new BinaryWriter (File.OpenWrite (ept_file)));
					} else {
						eptable_rnd100.Load (new BinaryReader (File.OpenRead (ept_file)));
					}

					ept_file = "ept-rnd8-" + dbname + "-grps-" + j + ".idx";
					if (!File.Exists (ept_file)) {
						chr_ept = new Chronos ();
						chr_ept.Start ();
						eptable_rnd8.Build (DB, j, (MetricDB _db, Random seed) => new EPListRandomPivots (DB, 8,seed), 1);
						chr_ept.End ();
						File.AppendAllText ("index-" + dbname + "-construction-speed-ept.csv", string.Format ("EPT-rnd8 {0} {1}{2}", j, chr_ept.AccTime, Environment.NewLine));
						eptable_rnd8.Save (new BinaryWriter (File.OpenWrite (ept_file)));
					} else {
						eptable_rnd8.Load (new BinaryReader (File.OpenRead (ept_file)));							
					}

					ept_file = "ept-rnd32-" + dbname + "-grps-" + j + ".idx";
					if (!File.Exists (ept_file)) {
						chr_ept = new Chronos ();
						chr_ept.Start ();
						eptable_rnd32.Build (DB, j, (MetricDB _db, Random seed) => new EPListRandomPivots (DB,32, seed), 1);
						chr_ept.End ();
						File.AppendAllText ("index-" + dbname + "-construction-speed-ept.csv", string.Format ("EPT-rnd32 {0} {1}{2}", j, chr_ept.AccTime, Environment.NewLine));
						eptable_rnd32.Save (new BinaryWriter (File.OpenWrite (ept_file)));
					} else {
						eptable_rnd32.Load (new BinaryReader (File.OpenRead (ept_file)));
					}

					ept_file = "ept-rnd400-" + dbname + "-grps-" + j + ".idx";
					if (!File.Exists (ept_file)) {
						chr_ept = new Chronos ();
						chr_ept.Start ();
						eptable_rnd400.Build (DB, j, (MetricDB _db, Random seed) => new EPListRandomPivots (DB,400, seed), 1);
						chr_ept.End ();
						File.AppendAllText ("index-" + dbname + "-construction-speed-ept.csv", string.Format ("EPT-rnd400 {0} {1}{2}", j, chr_ept.AccTime, Environment.NewLine));
						eptable_rnd400.Save (new BinaryWriter (File.OpenWrite (ept_file)));
					} else {
						eptable_rnd400.Load (new BinaryReader (File.OpenRead (ept_file)));
					}


					// generar queries
					var qstream=new QueryStream(queries_file,querie_arg);
					List<string> reslist=new List<string>();

					// ======================= Búsquedas ===============================0000

					string out_file=string.Format("res-{0}-dim[{2}]-dbsize[{1}]-{3}-",dbname,DB.Count,dim,query_type);
					string complete_out_file;
					// Sequential
					complete_out_file=out_file+"Seq.dat";
					Commands.Search(Seq,qstream.Iterate(),new ShellSearchOptions(queries_file,"Sequential",complete_out_file));
					reslist.Add(complete_out_file);
					// VPForest
					complete_out_file=out_file+string.Format("tau[{0}]-VPForest.dat",VPF_Search.Tau);
					Commands.Search(VPF_Search,qstream.Iterate(),new ShellSearchOptions(queries_file,"VP-Forest",complete_out_file));
					reslist.Add(complete_out_file);
					// EPTable_rnd-8
					complete_out_file=out_file+"EPTable_rnd-numgroups["+j+"]-pivspergrp[8].dat";
					Commands.Search(eptable_rnd8,qstream.Iterate(),new ShellSearchOptions(queries_file,"EPTable-rnd-8",complete_out_file));
					reslist.Add(complete_out_file);
					// EPTable_rnd-32
					complete_out_file=out_file+"EPTable_rnd-numgroups["+j+"]-pivspergrp[32].dat";
					Commands.Search(eptable_rnd32,qstream.Iterate(),new ShellSearchOptions(queries_file,"EPTable-rnd-32",complete_out_file));
					reslist.Add(complete_out_file);
					// EPTable_rnd-100
					complete_out_file=out_file+"EPTable_rnd-numgroups["+j+"]-pivspergrp[100].dat";
					Commands.Search(eptable_rnd100,qstream.Iterate(),new ShellSearchOptions(queries_file,"EPTable-rnd-100",complete_out_file));
					reslist.Add(complete_out_file);
					// EPTable_rnd-400
					complete_out_file=out_file+"EPTable_rnd-numgroups["+j+"]-pivspergrp[400].dat";
					Commands.Search(eptable_rnd400,qstream.Iterate(),new ShellSearchOptions(queries_file,"EPTable-rnd-400",complete_out_file));
					reslist.Add(complete_out_file);
					// EPTable_Opt
					complete_out_file=out_file+"EPTable_Opt-numgroups["+j+"].dat";
					Commands.Search(eptable_opt,qstream.Iterate(),new ShellSearchOptions(queries_file,"EPTable_Opt",complete_out_file));
					reslist.Add(complete_out_file);
					/**/

					// Parámetros para guardar los resultados
					reslist.Add("--horizontal");

					reslist.Add(string.Format("--save=res-{0}-check-out-dim[{3}]-dbsize[{1}]-{5}-VPF-Tau[{2}]-EPT-gps[{4}]",
					                          dbname,DB.Count,tau,dim,j,query_type) );
					Commands.Check(reslist);

				}
			}

		}
		/*
		public static void LoadQueries<T>(out IList<T[]> queries,string filename) where T: struct
		{
			BinaryReader br=new BinaryReader(File.OpenRead(filename));
			queries=new List<T[]>();
			int	count=br.ReadInt32();
			int dim=br.ReadInt32();
			for (int i=0;i<count;i++)
			{
				queries.Add(new T[dim]);
				PrimitiveIO<T>.LoadVector(br,dim,queries[i]);
			}
		}

		public static void LoadQueries<T>(out IList<T[]> queries,string filename,int count,int dim) where T: struct
		{
			StreamReader br=new StreamReader(filename);
			queries=new List<T[]>();
			List<T> q=new List<T>(dim);
			string line="";
			for (int i=0;i<count;i++)
			{
				line=br.ReadLine();
				Console.WriteLine("Read: {0}",line);
				queries.Add(new T[dim]);

				PrimitiveIO<T>.LoadVector(line,q);
				queries[i]=q.ToArray();
			}
		}
		*/
	}
}
