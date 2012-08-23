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
//   Original filename: natix/SimilaritySearch/Result/ResultInfo.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace natix.SimilaritySearch
{
	/// <summary>
	///  The result info. For checking purposes
	/// </summary>
	public struct ResultInfo
	{
		/// <summary>
		/// The query id
		/// </summary>
		public int qid;
		/// <summary>
		/// query type
		/// </summary>
		public double qtype;
		/// <summary>
		///  The raw string representing the object
		/// </summary>
		public string qraw;
		/// <summary>
		///  The Cost of the search
		/// </summary>
		public SearchCost cost;
		/// <summary>
		///  Search's time
		/// </summary>
		public TimeSpan time;
		/// <summary>
		///  The result set
		/// </summary>
		public List<ResultPair> result;
		
		/*public ResultInfo ()
		{
			
		}*/
		
		public void Load (BinaryReader Input)
		{
			this.qid = Input.ReadInt32 ();
			this.qtype = Input.ReadDouble ();
			this.qraw = Input.ReadString ();
			this.cost.External = Input.ReadInt32 ();
			this.cost.Internal = Input.ReadInt32 ();
			this.time = new TimeSpan (Input.ReadInt64 ());
			this.result = new List<ResultPair> ();
			int n = Input.ReadInt32 ();
			for (int i = 0; i < n; i++) {
				var docid = Input.ReadInt32 ();
				var dist = Input.ReadDouble ();
				this.result.Add (new ResultPair (docid, dist));
			}
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.qid);
			Output.Write ((double)this.qtype);
			Output.Write (this.qraw);
			Output.Write ((int)this.cost.External);
			Output.Write ((int)this.cost.Internal);
			Output.Write ((long)this.time.Ticks);
			int n = this.result.Count;
			Output.Write ((int)this.result.Count);
			for (int i = 0; i < n; i++) {
				var p = this.result [i];
				Output.Write (p.docid);
				Output.Write (p.dist);
			}
		}
		/// <summary>
		/// Constructor
		/// </summary>
		public ResultInfo (int qid, double qtype, string qraw, SearchCost cost, TimeSpan time, IResult res)
		{
			this.qid = qid;
			this.qtype = qtype;
			this.qraw = qraw;
			this.cost = new SearchCost (0, 0);
			this.time = TimeSpan.FromTicks (0);
			this.result = new List<ResultPair> ();
			this.Extend (res, cost, time);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="res">
		/// A <see cref="ResultInfo"/>
		/// </param>
		public void Extend (ResultInfo res)
		{
			this.cost.External += res.cost.External;
			this.cost.Internal += res.cost.Internal;
			this.time = this.time.Add (res.time);
			foreach (ResultPair p in res.result) {
				this.result.Add (p);
			}
		}
		/// <summary>
		/// Extends the current result set with a new result (increasing time and search's cost)
		/// </summary>

		public void Extend (IResult res, SearchCost cost, TimeSpan time)
		{
			this.cost.External += cost.External;
			this.cost.Internal += cost.Internal;
			this.time = this.time.Add (time);
			foreach (ResultPair p in res) {
				this.result.Add (p);
			}
		}
		/// <summary>
		/// An string representing the object
		/// </summary>
		/// <param name="showmaxres">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string ToString (int showmaxres)
		{
			return this.ToString (showmaxres, null);
		}
		/// <summary>
		///   An string representing the object, resolving the docid as strings in names
		/// </summary>
		/// <param name="showmaxres">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="names">
		/// A <see cref="System.String[]"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string ToString (int showmaxres, string[] names)
		{
			StringWriter ofile = new System.IO.StringWriter ();
			ofile.WriteLine ("======================");
			ofile.WriteLine ("qraw: {0}", qraw);
			ofile.WriteLine ("-----");
			ofile.WriteLine ("qid: {0}, qtype: {1}", qid, qtype);
			ofile.WriteLine ("time: {0}", time.TotalSeconds);
			ofile.WriteLine ("internalCost: {0}", this.cost.Internal);
			ofile.WriteLine ("externalCost: {0}", this.cost.External);
			showmaxres = Math.Min (showmaxres, result.Count);
			if (names == null) {
				for (int i = 0; i < showmaxres; i++) {
					ofile.Write ("(dist: {0}, docid: {1})", result[i].dist, result[i].docid);
					if (i + 1 < showmaxres) {
						ofile.Write (", ");
					}
				}
			} else {
				for (int i = 0; i < showmaxres; i++) {
					ofile.Write ("(dist: {0}, docid: {1}, name: {2})", result[i].dist, result[i].docid, names[result[i].docid]);
					if (i + 1 < showmaxres) {
						ofile.Write (", ");
					}
				}
			}
			if (showmaxres < result.Count) {
				ofile.WriteLine ("<{0} more results>", result.Count - showmaxres);
			} else {
				ofile.WriteLine ("<TheEnd>");
			}
			return ofile.ToString ();
		}
		/// <summary>
		/// A representation of the instance
		/// </summary>
		public override string ToString ()
		{
			return this.ToString (this.result.Count);
		}
		
		/// <summary>
		/// Cardinality of the unions
		/// </summary>
		public int UnionCardinality (ResultInfo n, int basisSize, int resultSize)
		{
			HashSet<int> ts = new HashSet<int> ();
			HashSet<int> ns = new HashSet<int> ();
			int i = 0;
			foreach (ResultPair p in this.result) {
				ts.Add (p.docid);
				i++;
				if (i >= resultSize) {
					break;
				}
			}
			i = 0;
			foreach (ResultPair p in n.result) {
				ns.Add (p.docid);
				i++;
				if (i >= basisSize) {
					break;
				}
			}
			ts.UnionWith (ns);
			return ts.Count;
		}
		
		/// <summary>
		///  Cardinality of the intersection
		/// </summary>

		public int IntersectCardinality (ResultInfo n, int basisSize, int resultSize)
		{
			HashSet<int> ts = new HashSet<int> ();
			HashSet<int> ns = new HashSet<int> ();
			int i = 0;
			foreach (ResultPair p in this.result) {
				ts.Add (p.docid);
				i++;
				if (i >= resultSize) {
					break;
				}
			}
			i = 0;
			foreach (ResultPair p in n.result) {
				ns.Add (p.docid);
				i++;
				if (i >= basisSize) {
					break;
				}

			}
			ts.IntersectWith (ns);
			return ts.Count;
		}
				/// <summary>
		/// Queries for the cardinality of the unions
		/// </summary>

		public double Recall (ResultInfo basis, int basisSize, int resultSize)
		{
			if (resultSize == 0) {
				return 0;
			}
			var intersection = this.IntersectCardinality (basis, basisSize, resultSize);
			if (intersection == 0) {
				return 0;
			}
			double r = intersection * 1.0 / basisSize;
			if (r > 1) {
				throw new ArgumentException (String.Format ("recall: {0}, basis: {1}, result: {2}", r, basisSize, resultSize));
			}
			return r;
		}
		
		private string NormalizeName (string name)
		{
			name = name.ToLower ();
			while (true) {
				string prev = name;
				name = Path.GetFileNameWithoutExtension (name);
				if (name == prev) {
					break;
				}
			}
			var re = new Regex(@"^ped\d*");
			name = re.Replace(name, "");
			// Console.WriteLine("===> normalize: {0}", name);
			return name;
		}
		/// <summary>
		/// Special for filename based comparisons, the names are normalized
		/// </summary>
		/// <remarks>
		/// Special for filename based comparisons, like audio and audio excerpts where we know exactly how to check the results
		/// and results can be correct for different docids.
		/// 
		/// The normalizations drops the directory name (if any), changes to lower case,
		/// and a prefix Ped\d\d and every extensions (if any).
		/// TODO: The FoundName of the recall-by-name functionality should be changed for future versions to a custom check function
		/// </remarks>
		/// <param name="resultSize">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="dbnames">
		/// A <see cref="System.String[]"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool FoundName (int resultSize, string[] dbnames)
		{
			if (dbnames == null) {
				return false;
			}
			int i = 0;
			foreach (ResultPair p in this.result) {
				string normqraw = this.NormalizeName (qraw);
				string normname = this.NormalizeName (dbnames[p.docid]);
				
				if (normqraw == normname) {
					//Console.WriteLine ("qraw: {0}, dbname: {1}, docid: {2}", normqraw, normname, p.docid);
					return true;
				}
				//Console.WriteLine ("NOT EQUAL: qraw: {0}, dbname: {1}, docid: {2}", normqraw, normname, p.docid);
				i++;
				if (i >= resultSize) {
					break;
				}
			}
			return false;
		}
	}
}
