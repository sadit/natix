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
//   Original filename: natix/FunLists/ListGen.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	///  A read only, not caching, list generator
	/// </summary>
	public class ListGen<T> : ListGenerator<T>
	{
		/// <summary>
		/// The generator.
		/// </summary>
		public Func<int,T> get_item;
		
		/// <summary>
		/// The handler of SetItem
		/// </summary>
		public Func<int,T,T> set_item;
		/// <summary>
		/// The length.
		/// </summary>
		public int Length;
		/// <summary>
		/// A function called when the instance is destroyed
		/// </summary>
		public Func<int> FinalizeInstance = null;
		
		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name='gen'>
		/// The function generating items
		/// </param>
		/// <param name='len'>
		/// Length of the list
		/// </param>
		public ListGen (Func<int, T> gen, int len)
		{
			this.get_item = gen;
			this.Length = len;
		}

		public ListGen (Func<int, T> _get_item, Func<int, T, T> _set_item, int len) : this(_get_item, len)
		{
			this.set_item = _set_item;
		}
	
		~ListGen ()
		{
			if (this.FinalizeInstance != null) {
				this.FinalizeInstance ();
			}
		}
		
		/// <summary>
		/// Gets the index-th item.
		/// </summary>
		public override T GetItem (int index)
		{
			return this.get_item (index);
		}
		
		/// <summary>
		/// Sets the item. Unsupported
		/// </summary>
		public override void SetItem (int index, T u)
		{
			if (this.set_item == null) {
				throw new NotSupportedException ("Read only item");
			}
			this.set_item(index, u);
		}
	
		/// <summary>
		/// Returns the length of the list
		/// </summary>
		public override int Count {
			get {
				return this.Length;
			}
		}
		public override void Add (T item)
		{
			// we suppose that add only increments Length, Generate can handle the range
			this.Length++;
		}

		public override void Clear ()
		{
			this.Length = 0;
		}
	}
}

