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
//   Original filename: natix/FunLists/ListShiftIndex.cs
// 
using natix;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	public class ListShiftIndex<T> : ListGenerator<T>
	{
		public IList<T> L;
		public int startIndex;
		public int count;
		
		public ListShiftIndex (IList<T> L, int startIndex, int count)
		{
			this.L = L;
			this.startIndex = startIndex;
			this.count = count;
		}
		
		public void Shift (int shift)
		{
			this.startIndex += shift;
			this.count -= shift;
		}

		public override int Count {
			get {
				return this.count;
			}
		}
		
		public override T GetItem (int index)
		{
			return this.L[index + this.startIndex];
		}
		
		public override void SetItem (int index, T u)
		{
			this.L[index + this.startIndex] = u;
		}
	}
}
