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
//   Original filename: natix/SimilaritySearch/Result/Result.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The result set
	/// </summary>
	public class ResultRange : Result
	{
        double cov;

        public ResultRange(double cov, int k) : base(k)
        {
            this.cov = cov;
        }

        public ResultRange(double cov) : this(cov, int.MaxValue)
        {
        }

        public override double CoveringRadius {
            get {
                return this.cov;
            }
        }
    }
}
