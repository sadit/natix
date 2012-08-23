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
//   Original filename: natix/CompactDS/Bitmaps/RankSelectBase.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public abstract class RankSelectBase : IRankSelect
	{
		public abstract int Count {
			get;
		}
		
		public virtual int Count1 {
			get {
				return this.Rank1 (this.Count-1);
			}
		}
		
		public abstract void AssertEquality (IRankSelect other);
		
		public virtual bool this[int pos]
		{
			get {
				return this.Access (pos);
			}
		}
		
		public virtual bool Access (int pos)
		{
			if (pos > 0) {
				return this.Rank1 (pos) != this.Rank1 (pos - 1);
			} else {
				return this.Rank1 (pos) == 1;
			}
		}
		
		public abstract void Save (BinaryWriter bw);
		public abstract void Load (BinaryReader br);
		
		public virtual int Select0 (int rank)
		{
			if (rank <= 0) {
				return -1;
			}
			var G = new ListGen<int> ((int i) => this.Rank0 (i), this.Count);
			int pos = GenericSearch.FindFirst<int> (rank, G);
			return pos;
		}

		public virtual int Select1 (int rank)
		{
			if (rank <= 0) {
				return -1;
			}
			var G = new ListGen<int> ((int i) => this.Rank1 (i), this.Count);
			return GenericSearch.FindFirst<int> (rank, G);
		}

		public virtual int Rank0 (int pos)
		{
			// pos + 1 = Rank0(pos) + Rank1(pos)
			return pos + 1 - this.Rank1 (pos);
		}

		public virtual int Rank1 (int pos)
		{
			return pos + 1 - this.Rank0 (pos);
		}
	}
}
