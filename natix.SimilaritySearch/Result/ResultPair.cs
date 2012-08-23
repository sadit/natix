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
//   Original filename: natix/SimilaritySearch/Result/ResultPair.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	// TODO: Implement Result for discrete distances with IndexedSortedArray
	/// <summary>
	/// A single pair of results
	/// </summary>
	public struct ResultPair : IComparable< ResultPair >
	{
		/// <summary>
		/// object identifier
		/// </summary>
		public int docid;
		/// <summary>
		/// Distance to the query
		/// </summary>
		public double dist;
		/// <summary>
		/// Result pair
		/// </summary>
		public ResultPair(int _docid, double _d) {
			this.dist = _d;
			this.docid = _docid;
		}
		/// <summary>
		/// An string representing the object
		/// </summary>
		public override string ToString ()
		{
			return "(dist: " + this.dist.ToString() + ", docid: " + this.docid.ToString() + ")"; 
		}
		/// <summary>
		/// Compare two result pairs (by distance)
		/// </summary>
		public int CompareTo (ResultPair o)
		{
			double c = this.dist - o.dist;
			if (c == 0) {
				return this.docid - o.docid;
			} else {
				return Math.Sign (c);
			}
		}
		
	}
}