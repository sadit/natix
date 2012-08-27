// 
//  Copyright 2012  sadit
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.IO;
using System.Collections.Generic;
using natix.CompactDS;
using natix.Sets;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class PermutedIndex : BasicIndex
	{
		public Index IDX;

		public override void Save (BinaryWriter Output)
		{
			IndexGenericIO.Save(Output, this.IDX);
		}

		public override void Load (BinaryReader Input)
		{
			this.IDX = IndexGenericIO.Load(Input);
			this.DB = (this.IDX.DB as PermutedSpace).DB;
		}

		public PermutedIndex () : base()
		{
		}

		public PermutedIndex (Index idx) : this()
		{
			this.Build(idx);
		}

		public void Build (Index idx)
		{
			if ((idx.DB as PermutedSpace) == null) {
				throw new ArgumentException("PermutedIndex idx.DB should be a PermutedSpace instance");
			}
			this.IDX = idx;
			this.DB = (idx.DB as PermutedSpace).DB;
		}


		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var R = this.IDX.SearchKNN (q, knn, new Result (res.K, res.Ceiling));
			var P = (this.IDX.DB as PermutedSpace);
			foreach (var p in R) {
				// res.Push(P.PERM.Inverse(p.docid), p.dist);
				res.Push(P.PERM[p.docid], p.dist);
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
			var res = new Result(int.MaxValue, false);
			var R = this.IDX.SearchRange (q, radius);
			var P = (this.IDX.DB as PermutedSpace);
			foreach (var p in R) {
				// res.Push(P.PERM.Inverse(p.docid), p.dist);
				res.Push(P.PERM[p.docid], p.dist);
			}
			// IDX 1 2 3 4 
			// DIR 4 2 1 3  (3,4) => (1,3) => (3,4)
			// INV 3 2 4 1
			return res;
		}
	}
}