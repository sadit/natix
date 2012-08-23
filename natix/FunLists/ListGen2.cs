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
//   Original filename: natix/FunLists/ListGen2.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	///  A read/write, not caching, list generator
	/// </summary>
	public class ListGen2<T> : ListGen<T>
	{
		public ListGen2 (Func<int, T> get_item, Func<int, T, T> set_item, int len) : base(get_item, set_item, len)
		{
		}
		
		public override void SetItem (int index, T u)
		{
			if (this.set_item != null) {
				this.set_item (index, u);
			}
		}
	}
}

