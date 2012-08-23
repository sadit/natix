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
//   Original filename: natix/CompactDS/Bitmaps/RankSelectBase64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public abstract class RankSelectBase64 : IRankSelect64
	{
		public abstract long Count {
			get;
		}
		
		public virtual long Count1 {
			get {
				return this.Rank1 (this.Count-1);
			}
		}
		
		public abstract void AssertEquality (IRankSelect64 other);
		
		public virtual long Select1 (long rank, UnraveledSymbolXLB ctx)
		{
			return this.Select1 (rank);
		}
		
		public virtual long Rank1 (long pos, UnraveledSymbolXLB ctx)
		{
			return this.Rank1 (pos);
		}
		
		public virtual bool Access (long pos, UnraveledSymbolXLB ctx)
		{
			return this.Access(pos);
		}
		
		public virtual bool this[long pos]
		{
			get {
				return this.Access (pos);
			}
		}
		
		public virtual bool Access (long pos)
		{
			if (pos > 0L) {
				return this.Rank1 (pos) != this.Rank1 (pos - 1);
			} else {
				return this.Rank1 (pos) == 1;
			}
		}
		
		public abstract void Save (BinaryWriter bw);
		public abstract void Load (BinaryReader br);
		
		public virtual long Select0 (long rank)
		{
			if (rank <= 0L) {
				return -1L;
			}
			var G = new ListGen64<long> ((long i) => this.Rank0 (i), this.Count);
			long pos = GenericSearch64.FindFirst<long> (rank, G);
			return pos;
		}

		public virtual long Select1 (long rank)
		{
			if (rank <= 0L) {
				return -1L;
			}
			var G = new ListGen64<long> ((long i) => this.Rank1 (i), this.Count);
			return GenericSearch64.FindFirst<long> (rank, G);
		}

		public virtual long Rank0 (long pos)
		{
			// pos + 1 = Rank0(pos) + Rank1(pos)
			return pos + 1L - this.Rank1 (pos);
		}

		public virtual long Rank1 (long pos)
		{
			return pos + 1L - this.Rank0 (pos);
		}
	}
}
