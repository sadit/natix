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
//   Original filename: natix/CompactDS/Lists/SortedListRS64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class SortedListRS64 : ListGenerator<long>
	{ 
		public IRankSelect64 rs;

		public SortedListRS64 (IRankSelect64 rs)
		{
			this.rs = rs;
		}
				
		public override int Count {
			get {
				return (int)this.rs.Count1;
			}
		}
		
		public override long GetItem (int index)
		{
			return this.rs.Select1 (index + 1);
		}
		
		public override void SetItem (int index, long u)
		{
			throw new NotSupportedException ();
		}
	}
}
