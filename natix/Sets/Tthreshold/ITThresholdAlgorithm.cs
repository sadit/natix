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
//   Original filename: natix/Sets/Tthreshold/ITThresholdAlgorithm.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.Sets
{
	public interface ITThresholdAlgorithm
	{
		int CompCounter {
			get;
		}
		/// <summary>
		/// Searches for sets with at least MinT items in common.
		/// </summary>
		/// <param name="PostingLists">
		/// Sorted arrays of integers
		/// </param>
		/// <param name="MinT">
		/// Minimum T (how many times an item appears in the posting lists)
		/// </param>
		/// <param name="docs">
		/// The output set
		/// </param>
		/// <param name="cardinalities">
		/// The cardinalities of items in docs, possible null value for algorithms not computing this value
		/// </param>
		void SearchTThreshold (IList<IList<int>> PostingLists, int MinT, out IList<int> docs, out IList<short> cardinalities);
	}
}

