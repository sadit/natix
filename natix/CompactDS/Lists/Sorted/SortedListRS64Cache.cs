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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class SortedListRS64Cache : ListGenerator<long>
	{
		public IRankSelect64 B;
		public int prev_index = -1;
		public long prev_value = -1;
		public int count = -1;
		public int shift;

		public SortedListRS64Cache (IRankSelect64 rsbitmap, int shift = 0)
		{
			this.B = rsbitmap;
			this.shift = shift;
		}
		
		public override int Count {
			get {
				if (this.count == -1) {
					this.count = (int)this.B.Count1;
				}
				return this.count;
			}
		}
		
		public override long GetItem (int index)
		{
			if (index == this.prev_index) {
				return this.prev_value;
			}
			this.prev_index = index;
			this.prev_value = this.B.Select1 (index + 1) + shift;
			return this.prev_value;
		}
		
		public override void SetItem (int index, long u)
		{
			throw new NotSupportedException ();
		}
		
	}
}
