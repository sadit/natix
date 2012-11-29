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
//   Original filename: natix/SimilaritySearch/Result/IResult.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Result of interface
	/// </summary>
	public interface IResult : IEnumerable< ResultPair >
	{
		/// <summary>
		///  Returns the First (closer) result in the set
		/// </summary>
		ResultPair First
		{
			get;
		}
		/// <summary>
		///  Returns the Last (farthest) result in the set
		/// </summary>
		ResultPair Last
		{
			get;
		}
		/// <summary>
		/// Gets the covering radius of the result
		/// </summary>
		double CoveringRadius {
			get;
		}
				
		/// <summary>
		/// Pushes the specified docid and dist to the result. Returns true if docid was appended, and false otherwise.
		/// </summary>
		bool Push(int docid, double dist);
		
		int K {
			get;
		}
		
		bool Ceiling {
			get;
		}
		
		int Count {
			get;
		}
    }
}
