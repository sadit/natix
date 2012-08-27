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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// A permuted space
	/// </summary>
	public class PermutedSpace : MetricDB
	{
		public MetricDB DB;
		public IPermutation PERM;

		public void Save (BinaryWriter Output)
		{
			Output.Write(this.Name);
			PermutationGenericIO.Save(Output, this.PERM);
			SpaceGenericIO.SmartSave(Output, this.DB);
		}

		public void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			this.PERM = PermutationGenericIO.Load(Input);
			this.DB = SpaceGenericIO.SmartLoad(Input, true);
		}

		public PermutedSpace ()
		{
		}

		public PermutedSpace (string name, MetricDB db, IPermutation perm)
		{
			this.Name = name;
			this.DB = db;
			this.PERM = perm;
		}
		
		public int Count {
			get {
				return this.PERM.Count;
			}
		}
		
		public IResult CreateResult (int K, bool ceiling)
		{
			return this.DB.CreateResult (K, ceiling);
		}

		public object Parse (string s, bool isquery)
		{
			return this.DB.Parse (s, isquery);
		}
		
		public double Dist (object a, object b)
		{
			return this.DB.Dist (a, b);
		}

		public int NumberDistances {
			get;
			set;
		}
		
		public string Name {
			get;
			set;
		}
		
		public object this [int docid] {
			get {
				var p = this.PERM [docid];
				return this.DB [p];
			}
		}

	}
}
