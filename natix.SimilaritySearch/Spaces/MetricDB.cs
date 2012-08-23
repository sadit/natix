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
//   Original filename: natix/SimilaritySearch/Spaces/Space.cs
// 
using natix;
using System;
using System.IO;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
//	/// <summary>
//	/// Should be used as a distance prototype for methods and classes accepting external distance's functions
//	/// </summary>
//	public delegate double Distance<T>(T a, T b);
	/// <summary>
	/// Exposes the basic methods to provide the space functionality.
	/// </summary>
	/// <remarks> There exists two versions of an space
	/// The complete version. It must know things about the underlying object datatypes, and can access to single items
	/// Restricted version. It should be used for tasks that doesn't need to know things about the object types.
	/// </remarks>
	public interface MetricDB : ILoadSave
	{
		/// <summary>
		/// The number of objects in the space, useful for iterating over the space using the indexer facilities.
		/// For simplicity and the randomness nature of the spaces, this methods should be prefeared instead any
		/// IEnumerable implementation.
		/// 
		/// This should be thread-safe (specially important for spaces with delete capabilities)
		/// </summary>
		int Count {
			get;
		}
		/// <summary>
		///  The number of distances computed at the accessing time, this is an monotonic function.
		/// This can be non-safe for multithread environments, it's useful for experimental tests.
		/// </summary>
		int NumberDistances {
			get;
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		string Name {
			get; set;
		}

		/// <summary>
		/// Creates an empty result set 
		/// </summary>
		IResult CreateResult (int K, bool ceiling);

		/// <summary>
		/// Returns the object numerated with docid
		/// </summary>
		object this[int docid] {
			get;
		}
		/// <summary>
		/// Distance between the specified a and b.
		/// </summary>
		double Dist(object a, object b);
		/// <summary>
		/// Obtains a valid object from the specified s.
		/// </summary>
		object Parse(string s, bool isquery);
	}
}
