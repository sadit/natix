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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Vector space
	/// </summary>
	public class MemMinkowskiVectorDB<T> : MemVectorDB<T> where T: struct
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public MemMinkowskiVectorDB () : base()
		{
		}

		/// <summary>
		/// Distance wrapper for any P-norm
		/// </summary>
		public override double Dist (object _a, object _b)
		{
			this.numdist++;
			var a = (T[])_a;
			var b = (T[])_b;
			if (this.P == 1) {
				return Num.DistL1 (a, b);
			}
			if (this.P == 2) {
				return Num.DistL2 (a, b);
			}
			if (this.P == -1) {
				return Num.DistLInf (a, b);
			}
			return Num.DistLP (a, b, this.P, true);
		}
	}
}
