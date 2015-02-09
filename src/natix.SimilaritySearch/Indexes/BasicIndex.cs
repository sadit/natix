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
//   Original filename: natix/SimilaritySearch/Indexes/BaseIndex.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
// using System.Linq;


namespace natix.SimilaritySearch
{
	/// <summary>
	/// The basic methods for an Index
	/// </summary>
	public abstract class BasicIndex : Index
	{
		/// <summary>
		/// space
		/// </summary>

		public virtual void Load (BinaryReader Input)
		{
			this.DB = SpaceGenericIO.SmartLoad(Input, true);
		}

		public virtual void Save (BinaryWriter Output)
		{
			SpaceGenericIO.SmartSave(Output, this.DB);
		}
		                        
		/// <summary>
		/// Constructor
		/// </summary>
		public BasicIndex ()
		{
		}

		/// <summary>
		/// Returns the main space
		/// </summary>
		public virtual MetricDB DB {
			get;
			set;
		}

		/// <summary>
		/// Search by range
		/// </summary>
		public virtual IResult SearchRange (object q, double radius)
		{
			var res = new ResultRange (radius, this.DB.Count);
			this.SearchKNN (q, this.DB.Count, res);
			return res;
		}
		
		/// <summary>
		/// Search by KNN
		/// </summary>
		public virtual IResult SearchKNN (object q, int K)
		{
			return this.SearchKNN (q, K, new Result (Math.Abs (K)));
		}
		
		/// <summary>
		/// Perform a KNN search.
		/// </summary>
		public abstract IResult SearchKNN (object q, int K, IResult res);

        protected long internal_numdists = 0;
		/// <summary>
		/// The current search cost object for the index
		/// </summary>
		public virtual SearchCost Cost {
			get {
				return new SearchCost (this.DB.NumberDistances, this.internal_numdists);
			}
		}

	}
}
