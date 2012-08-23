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
//   Original filename: natix/FunLists/ListCastNumberToInt.cs
// 
using System;
using System.IO;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	/// List cast. Cast a list of items of T to a list of integers
	/// </summary>
	public class ListCastToInt<T> : ListGenerator<int>
	{
		static INumeric<T> num = (INumeric<T>)Numeric.Get (typeof(T));
		IList<T> list;
		
		/// <summary>
		/// Initialization
		/// </summary>
		/// <param name='list'>
		/// The list containing items to be casted
		/// </param>
		public ListCastToInt (IList<T> list)
		{
			this.list = list;
		}
		
		/// <summary>
		/// Returns the index+1 item of the list
		/// </summary>
		public override int GetItem (int index)
		{
			return num.ToInt (this.list [index]);
		}
		
		/// <summary>
		/// Sets the index+1 item of the list
		/// </summary>
		public override void SetItem (int index, int u)
		{
			this.list [index] = num.FromInt (u);
		}
		
		/// <summary>
		/// Returns the number of items in the list
		/// </summary>
		public override int Count {
			get {
				return this.Count;
			}
		}
	}
}

