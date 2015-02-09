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
	/// A sample of an space
	/// </summary>
	public class SampleSpace : MetricDB
	{
		public MetricDB DB;
		public IList<int> SAMPLE;

		public IList<int> PERM {
			get {
				return this.SAMPLE;
			}
		}
		public virtual void Save (BinaryWriter Output)
		{
			Output.Write(this.Name);
			Output.Write((int)this.SAMPLE.Count);
			PrimitiveIO<int>.SaveVector(Output, this.SAMPLE);
			SpaceGenericIO.SmartSave(Output, this.DB);
		}

		public virtual void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			var count = Input.ReadInt32 ();
			this.SAMPLE = new int[count];
			PrimitiveIO<int>.LoadVector(Input, count, this.SAMPLE);
			this.DB = SpaceGenericIO.SmartLoad(Input, true);
		}

		public SampleSpace ()
		{
		}

		public SampleSpace (string name, MetricDB db, int samplesize) : this(name, db, samplesize, RandomSets.GetRandom(-1))
		{
		}

		public SampleSpace (string name, MetricDB db, int samplesize, Random rand)
		{
			this.Name = name;
			this.DB = db;
			this.SAMPLE = RandomSets.GetRandomSubSet (samplesize, db.Count, rand);
		}

		public SampleSpace (string name, MetricDB db, IList<int> sample)
		{
			this.Name = name;
			this.DB = db;
			this.SAMPLE = sample;
		}
		
		public int Count {
			get {
				return this.SAMPLE.Count;
			}
		}
		
//		public IResult CreateResult (int K, bool ceiling)
//		{
//			return this.DB.CreateResult (K, ceiling);
//		}

		public object Parse (string s)
		{
			return this.DB.Parse (s);
		}
		
		public double Dist (object a, object b)
		{
			return this.DB.Dist (a, b);
		}

		public long NumberDistances {
			get;
			set;
		}
		
		public string Name {
			get;
			set;
		}
		
		public object this [int docid] {
			get {
				var p = this.SAMPLE [docid];
				return this.DB [p];
			}
		}
	}
}
