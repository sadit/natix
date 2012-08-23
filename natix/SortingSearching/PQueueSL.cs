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
//   Original filename: natix/SortingSearching/PQueueSL.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class PQueueSL<T>
	{
		SkipList2<T> L;
		SkipList2AdaptiveContext<T> ctx;
		
		public PQueueSL (Comparison<T> cmp_fun)
		{
			this.L = new SkipList2<T> (0.5, cmp_fun);
			this.ctx = new SkipList2AdaptiveContext<T> (true, this.L.FIRST);
			// this.ctx = null;
			// this.ctx = new SkipListAdaptiveContext<T> (false, null);
		}
		
		public int Count {
			get {
				return this.L.Count;
			}
		}
		
		public T Pop ()
		{
			if (this.Count < 1) {
				throw new ArgumentOutOfRangeException ("empty p-queue");
			}
			var r = this.L.RemoveFirstNode ();
			if (this.ctx != null && r == this.ctx.StartNode) {
				this.ctx.StartNode = this.L.FIRST;
			}
			return r.data;
		}
		
		public void Push (T data)
		{
			this.L.Add (data, this.ctx);
		}
		
		public T Top ()
		{
			if (this.Count < 1) {
				throw new ArgumentOutOfRangeException ("empty p-queue");
			}
			return this.L.GetItem (0);
		}
	}
}

