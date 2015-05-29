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
	public class Query
	{
		public long QueryID { get; set; }
		public double QueryType { get; set; }
		public string QueryRaw { get; set; }
		public double SearchCostInternal { get; set; }
		public double SearchCostTotal { get; set; }
		public double SearchTime { get; set; }
		public List<ItemPair> Result = new List<ItemPair>();

		public Query ()
		{
			this.Result = new List<ItemPair> ();
		}

		public static double RecallByDistance (IResult basis, IResult current)
		{
			return RecallByDistance (new List<ItemPair>(basis), new List<ItemPair>(current));
		}

		public static double RecallByDistance (List<ItemPair> basis, List<ItemPair> current)
		{
			// fixed by memo
			if (basis.Count == 0 && current.Count == 0) {
				return 1.0;
			}
			int i = 0;
			int j = 0;
			int matches = 0;
			while (i < basis.Count && j < current.Count) {
				if (basis [i].Dist == current [j].Dist) {
					++matches;
					++i;
					++j;
				} else if (basis [i].Dist < current [j].Dist) {
					++i;
				} else {
					++j;
				}
			}
			if (matches == 0) {
				return 0;
			}
			return matches * 1.0 / basis.Count;
		}

		public static double RecallByObjID (IResult basis, IResult current)
		{
			return RecallByObjID (new List<ItemPair>(basis), new List<ItemPair>(current));
		}

		public static double RecallByObjID (List<ItemPair> basis, List<ItemPair> current)
		{
			var H = new HashSet<int>();
			foreach (var p in basis) {
				H.Add (p.ObjID);
			}
			return RecallByObjID (H, current);
		}

		public static double RecallByObjID (HashSet<int> H, List<ItemPair> current)
		{
			// fixed by memo
			if (H.Count == 0 && current.Count == 0) {
				return 1.0;
			}

			int matches = 0;
			foreach (var p in current) {
				if (H.Contains (p.ObjID)) {
					++matches;
				}
			}

			if (matches == 0) {
				return 0;
			}
			return matches * 1.0 / H.Count;
		}

		public static double Recall (IResult basis, IResult current)
		{
			return Recall (new List<ItemPair>(basis), new List<ItemPair>(current));
		}

		public static double Recall (List<ItemPair> basis, List<ItemPair> current)
		{
			return Math.Max (RecallByDistance(basis, current),
			                 RecallByObjID(basis, current));
		}
	}
}
