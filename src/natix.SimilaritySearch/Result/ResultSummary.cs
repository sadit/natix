//
// Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class ResultSummary
	{
		public string ResultName { get; set; }
		public string IndexName { get; set; }
		public string QueriesName { get; set; }

		public double SearchTime { get; set; }
		public double SearchCostInternal { get; set; }
		public double SearchCostTotal { get; set; }
		public double CoveringRadius { get; set; }
		public double EmptyResults { get; set; }
		public List<Query> QueryList { get; set; }
		public Dictionary<string, object> Parametrized { get; set; }

		public ResultSummary ()
		{
			this.QueryList = new List<Query> ();
			this.Parametrized = new Dictionary<string, object> ();
		}

		public void Add(Query q)
		{
			this.SearchTime += q.SearchTime;
			this.SearchCostTotal += q.SearchCostTotal;
			this.SearchCostInternal += q.SearchCostInternal;
			if (q.Result.Count > 0) {
				this.CoveringRadius += q.Result [q.Result.Count - 1].Dist;
			} else {
				++this.EmptyResults;
			}
			this.QueryList.Add (q);
		}

		public void ComputeSummary()
		{
			var c = this.QueryList.Count;
			this.SearchTime /= c;
			this.SearchCostTotal /= c;
			this.SearchCostInternal /= c; 
			this.CoveringRadius /= c; // possible weird data
		}

		/// <summary>
		/// Compute statistics against basis
		/// </summary>
		public void Parametrize (ResultSummary groundThruth)
		{
			if ((this.QueryList.Count != groundThruth.QueryList.Count) || this.QueriesName != groundThruth.QueriesName) {
				throw new ArgumentException ("ERROR: QUERY SOURCES DOESN'T MATCH!!");
			}

			double count = this.QueryList.Count;
			var recall = 0.0;
			for (int i = 0; i < count; ++i) {
				recall += Query.Recall (groundThruth.QueryList [i].Result, this.QueryList [i].Result);
			}
			this.Parametrized ["Recall"] = recall / count;
			this.Parametrized ["Speedup"] = groundThruth.SearchTime / this.SearchTime;
			if (groundThruth.SearchCostInternal > 0) {
				this.Parametrized ["SearchCostInternal"] = this.SearchCostInternal / groundThruth.SearchCostInternal;
			} else {
				this.Parametrized ["SearchCostInternal"] = 0.0;
			}
			if (groundThruth.SearchCostTotal > 0) {
				this.Parametrized ["SearchCostTotal"] = this.SearchCostTotal / groundThruth.SearchCostTotal;
			} else {
				this.Parametrized ["SearchCostTotal"] = 0.0;
			}
			this.Parametrized ["ProximityRatio"] = this.CoveringRadius / groundThruth.CoveringRadius;
			this.Parametrized ["Memory"] = 0;
			this.Parametrized ["ConstructionCostTime"] = 0;
			this.Parametrized ["ConstructionCostDistances"] = 0;

			if (File.Exists (this.IndexName)) {
				using (var f = File.OpenRead(this.IndexName)) {
					this.Parametrized ["Memory"] = f.Length;
				}
			}
			if (File.Exists (this.IndexName + ".construction-time.json")) {
				var txt = File.ReadAllText (this.IndexName + ".construction-time.json");
				try {
					var msg = JsonConvert.DeserializeObject<Dictionary<string, object>> (txt);
					this.Parametrized ["ConstructionCostTime"] = msg["Seconds"];
					this.Parametrized ["ConstructionCostDistances"] = msg["Distances"];
				} catch (Newtonsoft.Json.JsonReaderException) {
					Console.WriteLine ("Ignoring construction-time information. Old construction-time format?");
				}
			}
		}

	}
}

