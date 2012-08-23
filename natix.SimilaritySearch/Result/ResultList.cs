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
//   Original filename: natix/SimilaritySearch/Result/ResultList.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace natix.SimilaritySearch
{

	/// <summary>
	/// Statistics for results
	/// </summary>
	public class ResultList: List<ResultInfo>
	{
		/// <summary>
		/// The name of the index
		/// </summary>
		public string indexname;
		/// <summary>
		/// The query file
		/// </summary>
		public string queryfile;
		/// <summary>
		/// The average recall
		/// </summary>
		public double avg_recall = -1;
		/// <summary>
		///  The average time in seconds
		/// </summary>
		public double avg_seconds = -1;
		/// <summary>
		/// Average covering radius
		/// </summary>
		public double avg_covering_radius = 0;
		/// <summary>
		/// Average covering radius (basis)
		/// </summary>
		public double avg_covering_radius_basis = 0;
		/// <summary>
		/// The empty results count
		/// </summary>
		public int empty_results;
		/// <summary>
		///  The average cost
		/// </summary>
		public SearchCost avg_cost = new SearchCost(0, 0);
		/// <summary>
		/// The result filename
		/// </summary>
		public string Name = "";
		int avg_result_size = 0;
		int avg_basis_size = 0;
		double avg_recall_by_name = 0;
		
		public ResultList () : base()
		{
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.indexname);
			Output.Write (this.queryfile);
			Output.Write (this.Name);
			// the other variables are not necessary since they are computed
			foreach (ResultInfo info in this) {
				info.Save (Output);
			}
		}
		
		public void Load (BinaryReader Input)
		{
			this.indexname = Input.ReadString ();
			this.queryfile = Input.ReadString ();
			this.Name = Input.ReadString ();
			while (Input.BaseStream.Position < Input.BaseStream.Length) {
				var info = new ResultInfo ();
				info.Load (Input);
				this.Add (info);
			}

		}
		
		
		/// <summary>
		/// Constructor
		/// </summary>
		public ResultList (string indexname, string queryfile) : base()
		{
			this.indexname = indexname;
			this.queryfile = queryfile;
		}
		
		/// <summary>
		/// Extend with an external ResultList
		/// </summary>
		/// <param name="reslist">
		/// A <see cref="ResultList"/>
		/// </param>
		public void Extend (ResultList reslist)
		{
			if (this.Count != reslist.Count) {
				throw new ArgumentException ("ResultLists should have the same length to be joined");
			}
			for (int i = 0; i < reslist.Count; i++) {
				this[i].Extend (reslist[i]);
			}
		}
		
		/// <summary>
		/// Load from a serialized file
		/// </summary>
		public static ResultList FromFile (string arg)
		{
			/*
			FileStream stream = new FileStream (arg, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
			ResultList R = (ResultList)Dirty.DeserializeBinary (stream);
			//Console.WriteLine ("# R resultname: {0}, queryfile: {1}", arg, R.queryfile);
			while (stream.Position < stream.Length) {
				R.Add ((ResultInfo)Dirty.DeserializeBinary (stream));
			}
			stream.Close ();
			return R;
			*/
			ResultList R = new ResultList ();
			using (BinaryReader Input = new BinaryReader (File.OpenRead (arg))) {
				R.Load (Input);
			}
			return R;
		}


		/// <summary>
		/// Compute statistics against basis
		/// </summary>
		public void Parametrize (ResultList basis, int maxBasisSize, int maxResultSize, string[] dbnames)
		{
			if (dbnames == null && (this.Count != basis.Count || this.queryfile != basis.queryfile)) {
				//throw new ArgumentException ("The basis must be a resultlist from the same query set");
				Console.WriteLine ("WARNING: QUERY SOURCES DOESN'T MATCH!!");
			}
			double recall = 0;
			double sec = 0;
			this.avg_result_size = 0;
			this.avg_basis_size = 0;
			this.avg_recall_by_name = 0;
			this.avg_covering_radius = 0;
			double size = this.Count;
			double cost_internal = 0;
			double cost_external = 0;

			for (int i = 0; i < size; i++) {
				int basisSize = Math.Min (maxBasisSize, basis [i].result.Count);
				int resultSize = Math.Min (maxResultSize, this [i].result.Count);
				recall += this [i].Recall (basis [i], basisSize, resultSize);
				sec += this [i].time.TotalSeconds;
				if (this [i].FoundName (resultSize, dbnames)) {
					this.avg_recall_by_name += 1;
				}
				// cost internal and external are divided here to avoid overflows
				// (the costs can be very large)
				cost_internal += this [i].cost.Internal / size;
				cost_external += this [i].cost.External / size;
				this.avg_result_size += resultSize;
				this.avg_basis_size += basisSize;
				if (resultSize < 1 || basisSize < 1) {
					this.empty_results++;
				} else {
					this.avg_covering_radius += this [i].result [resultSize - 1].dist;
					this.avg_covering_radius_basis += basis [i].result [basisSize - 1].dist;
				}
			}
			if (this.empty_results != size) {
				this.avg_covering_radius /= (size - this.empty_results);
				this.avg_covering_radius_basis /= (size - this.empty_results);				
			}
			this.avg_result_size /= (int)size;
			this.avg_basis_size /= (int)size;
			this.avg_recall = recall / size;
			this.avg_seconds = sec / size;
			this.avg_cost.External = (int)cost_external;
			this.avg_cost.Internal = (int)cost_internal;
			this.avg_recall_by_name /= size;			
		}
		
		/// <summary>
		/// The representing string for this instance
		/// </summary>
		public string ToString (bool vertical, bool header)
		{
			StringWriter s = new StringWriter ();
			if (vertical) {
				s.WriteLine ("=========");
				s.WriteLine ("resultname: {0}", this.Name);
				s.WriteLine ("indexname: {0}", this.indexname);
				s.WriteLine ("queryfile: {0}", this.queryfile);
				s.WriteLine ("avg_basis_size: {0}", this.avg_basis_size);
				s.WriteLine ("avg_result_size: {0}", this.avg_result_size);
				s.WriteLine ("avg_recall: {0}", this.avg_recall);
				s.WriteLine ("avg_recall_by_name: {0}", this.avg_recall_by_name);
				s.WriteLine ("avg_seconds: {0}", this.avg_seconds);
				s.WriteLine ("avg_cost_internal: {0}", this.avg_cost.Internal);
				s.WriteLine ("avg_cost_external: {0}", this.avg_cost.External);
				s.WriteLine ("saved_results: {0}", this.Count);
				s.WriteLine ("avg_covering_radius_basis: {0}", this.avg_covering_radius_basis);
				s.WriteLine ("avg_covering_radius_result: {0}", this.avg_covering_radius);
				s.WriteLine ("empty_results_count: {0}", this.empty_results);
			} else {
				if (header) {
					s.WriteLine ("#filename: {0} - {1}", indexname, queryfile);
					s.WriteLine ("#columns: indexname, queryfile, avg_basis_size, avg_result_size, avg_recall, avg_recall_by_name, avg_seconds, avg_cost_internal, avg_cost_external, resname, avg_covering_radius_basis, avg_covering_radius_result, empty_results_count");
				}
				s.Write ("{0}", this.indexname);
				s.Write (" {0}", this.queryfile);
				s.Write (" {0}", this.avg_basis_size);
				s.Write (" {0}", this.avg_result_size);
				s.Write (" {0}", this.avg_recall);
				s.Write (" {0}", this.avg_recall_by_name);
				s.Write (" {0}", this.avg_seconds);
				s.Write (" {0}", this.avg_cost.Internal);
				s.Write (" {0}", this.avg_cost.External);
				s.Write (" {0}", this.Name);
				s.Write (" {0}", this.avg_covering_radius_basis);
				s.Write (" {0}", this.avg_covering_radius);
				s.Write (" {0}", this.empty_results);
			}
			return s.ToString ();
		}
		/// <summary>
		/// The representing string
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public override string ToString ()
		{
			return this.ToString (true, true);
		}
	}
}
