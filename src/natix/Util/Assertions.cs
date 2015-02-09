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
//   Original filename: natix/Util/Assertions.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	/// Common assertions (for debugging purposes)
	/// </summary>
	public class Assertions
	{
		/// <summary>
		/// Assert equality of two lists
		/// </summary>
		public static void AssertIList<T> (IList<T> a, IList<T> b, string msg)
		{
			if (a.Count != b.Count) {
				throw new ArgumentException (String.Format ("{0} inequality in Count, a.Count: {1}, b.Count: {2}", msg,
						a.Count, b.Count));
			}
			for (int i = 0; i < a.Count; i++) {
				if (!a[i].Equals (b[i])) {
					throw new ArgumentException(String.Format ("{0} inequality in the index: {0}", i));
				}
			}
		}
	}
}

