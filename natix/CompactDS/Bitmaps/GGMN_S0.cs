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
//   Original filename: natix/CompactDS/Bitmaps/GGMN_S0.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	/// <summary>
	/// GNBitmap with special implementation for select 0
	/// </summary>
	public class GGMN_S0 : GGMN
	{
		ListGen<uint> AbsComp;
		ListGen<uint> BitBlocksComp;

		public override int Select0 (int rank)
		{
			if (this.AbsComp == null) {
				int scale = this.B << 5;
				this.AbsComp = new ListGen<uint> ((int i) => ((uint)((i+1) * scale)) - this.Abs[i], this.Abs.Count);
				this.BitBlocksComp = new ListGen<uint> ((int u) => ~this.BitBlocks[u], this.BitBlocks.Count);
			}
			if (rank <= 0) {
				return -1;
			}
			int absindex = GenericSearch.FindFirst<uint> ((uint)rank, AbsComp);
			if (absindex >= 0 && AbsComp[absindex] == rank) {
				absindex--;
			}
			if (absindex < 0) {
				return BitAccess.Select1 (BitBlocksComp, 0, this.B, rank);
			} else {
				int startindex = (absindex + 1) * this.B;
				return ((startindex) << 5) +
					BitAccess.Select1 (BitBlocksComp, startindex, this.B, rank - (int)AbsComp[absindex]);
			}

		}
	}
}
