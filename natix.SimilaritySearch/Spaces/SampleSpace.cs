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
	/// A Generic permuted space
	/// </summary>
	public class SampleSpace : MetricDB
	{
		MetricDB db;
		IList<int> sample;
		public void Save (BinaryWriter O)
		{
			O.Write(this.Name);
			O.Write((int)this.sample.Count);
			PrimitiveIO<int>.WriteVector(O, this.sample);
			O.Write(this.db.Name);
		}

		public void Load (BinaryReader I)
		{
			this.Name = I.ReadString ();
			var count = I.ReadInt32 ();
			this.sample = new int[count];
			PrimitiveIO<int>.ReadFromFile(I, count, this.sample);
			var name = I.ReadString();
			this.db = SpaceGenericIO.Load(name, true);
		}

		public SampleSpace ()
		{
		}

		public SampleSpace (string name, MetricDB db, IList<int> sample)
		{
			this.Name = name;
			this.db = db;
			this.sample = sample;
		}
		
		public int Count {
			get {
				return this.sample.Count;
			}
		}
		
		public IResult CreateResult (int K, bool ceiling)
		{
			return this.db.CreateResult (K, ceiling);
		}

		public object Parse (string s, bool isquery)
		{
			return this.db.Parse (s, isquery);
		}
		
		public double Dist (object a, object b)
		{
			return this.db.Dist (a, b);
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
				var p = this.sample [docid];
				return this.db [p];
			}
		}
	}
}
