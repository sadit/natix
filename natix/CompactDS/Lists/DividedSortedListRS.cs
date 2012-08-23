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
//   Original filename: natix/CompactDS/Lists/DividedSortedListRS.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class DividedSortedListRS : ListGenerator<int>
	{
		public IRankSelect B;
		public int divisor;
		
		public DividedSortedListRS (IRankSelect rsbitmap, int div)
		{
			this.B = rsbitmap;
			this.divisor = div;
		}
		
		public override int Count {
			get {
				// return this.B.Rank1 (this.B.Count - 1);
				return this.B.Count1 ;
			}
		}
		
		public override int GetItem (int index)
		{
			return this.B.Select1 (index + 1) / this.divisor;
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotSupportedException ();
		}
		
	}
}
