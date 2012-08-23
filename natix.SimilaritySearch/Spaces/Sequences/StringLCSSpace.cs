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

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;

namespace natix.SimilaritySearch
{

	/// <summary>
	/// String space
	/// </summary>
	public class StringLCSSpace<T> : StringSpace< T > where T : IComparable
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public StringLCSSpace () : base()
		{
		}


		/// <summary>
		/// Wrapper to the real string distance
		/// </summary>
		public override double Dist (object a, object b)
		{
			this.numdist++;
			return StringSpace<T>.LCS ((IList<T>)a, (IList<T>)b);
		}
	}
}
