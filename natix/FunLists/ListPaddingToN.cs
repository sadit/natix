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
using natix;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	public class ListPaddingToN<T> : ListGenerator<T>
	{
		public IList<T> L;
		public int N;
		public T padding_value;

		public ListPaddingToN (IList<T> L, int N, T padding_value)
		{
			this.L = L;
			this.N = N;
			this.padding_value = padding_value;
		}
		
		public override int Count {
			get {
				return this.N;
			}
		}
		
		public override T GetItem (int index)
		{
			if (index < this.L.Count) {
				return this.L[index];
			}
			return this.padding_value;
		}
		
		public override void SetItem (int index, T u)
		{
			if (index < this.L.Count) {
				this.L[index] = u;
			}
		}
	}
}
