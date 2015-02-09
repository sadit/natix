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
//   Original filename: natix/FunLists/ListGen64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	///  A read only, not caching, list generator
	/// </summary>
	public class ListGen64<T> : ListGenerator64<T>
	{
		/// <summary>
		/// The generator.
		/// </summary>
		public Func<long,T> get_item;
		public Func<long, T, T> set_item;
		/// <summary>
		/// The length.
		/// </summary>
		public long Length;
		/// <summary>
		/// A function called when the instance is destroyed
		/// </summary>
		public Func<int> FinalizeInstance = null;
		
		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name='_get_item'>
		/// The function generating items
		/// </param>
		/// <param name='len'>
		/// Length of the list
		/// </param>
		public ListGen64 (Func<long, T> _get_item, long len)
		{
			this.get_item = _get_item;
			this.Length = len;
		}
		
		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name='_get_item'>
		/// A function to generate items
		/// </param>
		/// <param name='_set_item'>
		/// A function to save items
		/// </param>
		/// <param name='len'>
		/// Length of the list
		/// </param>
		public ListGen64 (Func<long, T> _get_item, Func<long,T,T> _set_item, long len) : this(_get_item, len)
		{
			this.set_item = _set_item;
		}
		
		~ListGen64 ()
		{
			if (this.FinalizeInstance != null) {
				this.FinalizeInstance ();
			}
		}
		
		/// <summary>
		/// Gets the index-th item.
		/// </summary>
		public override T GetItem (long index)
		{
			return this.get_item (index);
		}
		
		/// <summary>
		/// Sets the item. 
		/// </summary>
		
		public override void SetItem (long index, T u)
		{
			if (this.set_item == null) {
				throw new NotSupportedException ();
			}
			this.set_item (index, u);			
		}
		
		/// <summary>
		/// Returns the length of the list
		/// </summary>
		public override long Count {
			get {
				return this.Length;
			}
		}
		public override void Add (T item)
		{
			// we suppose that Add only increments the Length.
			// the generator should handle the range
			++this.Length;
		}

		public override void Clear ()
		{
			this.Length = 0;
		}
	}
}
