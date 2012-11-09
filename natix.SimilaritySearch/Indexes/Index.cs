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
//   Original filename: natix/SimilaritySearch/Indexes/Index.cs
// 
using System;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Simple implementation of two values to measure an index performance. Internal and external cost
	/// </summary>
	/// <remarks>
	/// We can distinguish over distance computations and computations need to the index.
	/// </remarks>
	public struct SearchCost
	{
		/// <summary>
		/// Total cost
		/// </summary>
		public int Total;
		/// <summary>
		/// Internal
		/// </summary>
		public int Internal;
		/// <summary>
		/// Constructor
		/// </summary>
		public SearchCost (int Total, int Internal) //, int External)
		{
			this.Total = Total;
            this.Internal = Internal;
		}
	}
	
	/// <summary>
	/// An index without access to the individual objects, useful load,save, build and configure tasks 
	/// </summary>
	public interface Index : ILoadSave
	{
		/// <summary>
		/// Access to the accumulated performed work of the index
		/// </summary>
		SearchCost Cost {
			get;
		}

		/// <summary>
		/// Access to the main space (the indexed space)
		/// </summary>
		MetricDB DB {
			get;
			set;
		}
		/// <summary>
		/// Search by range
		/// </summary>
		IResult SearchRange(object q, double radius);
		/// <summary>
		/// Searching K Nearest Neighbors
		/// </summary>
		IResult SearchKNN (object q, int k);
		/// <summary>
		/// Searches for KNN but using res as *suggested* ouput, it could returns another res object, so it must be updated (if necessary)
		/// </summary>
		IResult SearchKNN (object q, int k, IResult res);

	}	
}
