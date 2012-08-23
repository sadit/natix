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
//   Original filename: natix/SortingSearching/Searching/BlockSearch.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	/// <summary>
	/// 	It's not optimized, right now it focus on the reduction of the comparisons
	/// </summary>

	public class BlockSearch<T> : ISearchAlgorithm<T> where T: IComparable
	{
		double BlockRatio;
		Random rand = null;
		public ISearchAlgorithm<T> SSample;
		public ISearchAlgorithm<T> SBlock;
		
		public int CompCounter {
			get {
				return this.SSample.CompCounter + this.SBlock.CompCounter;
			}
		}
		
		public BlockSearch (double blockRatio) : this(blockRatio, new DoublingSearch<T>(), new BinarySearch<T>())
		{
		}
		
		public BlockSearch (double blockRatio, ISearchAlgorithm<T> ssample, ISearchAlgorithm<T> sblock)
		{
			if (blockRatio < 0) {
				this.rand = new Random ();
			}
			this.BlockRatio = blockRatio;
			this.SSample = ssample;
			this.SBlock = sblock;
		}
		
		public bool Search (T data, IList<T> L, out int occPosition, int min, int max)
		{
			int N = max - min;
			if (N < 1) {
				occPosition = max;
				return false;
			}
			if (N < 16) {
				// avoiding small arrays (in fact, this check should be for a too much larger N)
				// right now it's just a work around for the log(1) definition
				var r = this.SBlock.Search (data, L, out occPosition, min, max);
				return r;
			}
			if (this.rand != null) {
				this.BlockRatio = this.rand.NextDouble ();
			}
			int blockSize = (int)(N * this.BlockRatio + 1);
			int Nsample = (int)Math.Ceiling (((double)N) / blockSize);
			// int Nsample = N / blockSize;
			max--;
			ListGen<T> Lsample = new ListGen<T> (
				(int iS) => L[Math.Min ((iS + 1) * blockSize - 1 + min, max)],
				Nsample);
			int blockId;
			bool found = this.SSample.Search (data, Lsample, out blockId, 0, Nsample);
			max++;
			if (blockId == Nsample) {
				// out of range
				occPosition = max;
				return false;
			}
			// start position
			blockId *= blockSize;
			blockId += min;
			// end position
			N = Math.Min (blockId + blockSize, max);
			//Console.WriteLine ("L.Count: {0}, blockBegin: {1}, blockEnd: {2}",
			//	L.Count, blockId, N);
			found = this.SBlock.Search (data, L, out occPosition, blockId, N);
			// Console.WriteLine ("Found: {0}, occPosition: {1}", r, occPosition);
			return found;
		}
	}
}
