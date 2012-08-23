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
//   Original filename: natix/FunLists/ListCast2.cs
// 
using System;
using System.IO;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	/// List cast2. Cast an integer list to a list of type T
	/// </summary>
	public class ListCastFromInt<T> : ListGenerator<T>
	{
		static INumeric<T> num = (INumeric<T>)Numeric.Get (typeof(T));
		IList<int> list;
		
		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name='list'>
		/// The list containing items to be casted
		/// </param
		public ListCastFromInt (IList<int> list)
		{
			this.list = list;
		}
		
		/// <summary>
		/// Gets the index+1 th item of the list
		/// </summary>

		public override T GetItem (int index)
		{
			return num.FromInt (this.list [index]);
		}
		
		/// <summary>
		/// Sets the index+1 th item  to u
		/// </summary>

		public override void SetItem (int index, T u)
		{
			this.list [index] = num.ToInt (u);
		}
		
		/// <summary>
		/// Returns the length of the list
		/// </summary>
		/// <value>
		/// The count.
		/// </value>
		public override int Count {
			get {
				return this.list.Count;
			}
		}
	}
}

