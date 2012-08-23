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
//   Original filename: natix/FunLists/ListIntegersShiftValues.cs
// 
using natix;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	public class ListIntegersShiftValues : ListGenerator<int>
	{
		int shift;
		IList<int> list;
		public ListIntegersShiftValues (IList<int> list, int shift)
		{
			this.list = list;
			this.shift = shift;
		}
		
		public override int GetItem (int index)
		{
			return this.list[index] + this.shift;
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}
		
		public override int Count {
			get {
				return this.list.Count;
			}
		}
	}
}
