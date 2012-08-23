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
	public class NullSpace : MetricDB
	{
		public void Save (BinaryWriter O)
		{
		}

		public void Load (BinaryReader I)
		{
		}

		public NullSpace ()
		{
		}
		
		public int Count {
			get {
				return 0;
			}
		}
		
		public IResult CreateResult (int K, bool ceiling)
		{
			throw new NotSupportedException();
		}

		public object Parse (string s, bool isquery)
		{
			throw new NotSupportedException();
		}
		
		public double Dist (object a, object b)
		{
			throw new NotSupportedException();
		}

		public int NumberDistances {
			get {
				throw new NotSupportedException();
			}
		}
		
		public string Name {
			get { return "";}
			set {}
		}
		
		public object this [int docid] {
			get {
				throw new NotSupportedException();
			}
		}
	}
}
