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
//   Original filename: natix/CompactDS/Bitmaps/IRankSelect.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// The interface describing an indexed bitmap
	/// </summary>
	public interface IRankSelect : ILoadSave
	{		
		/// <summary>
		/// The number of bits in the bitmap
		/// </summary>
		int Count {
			get;
		}
		/// <summary>
		/// The number of enabled bits in the bitmap. It's equivalent to Rank1(Count - 1),
		/// but many structures can solve efficiently this request
		/// </summary>
		int Count1 {
			get;
		}
		/// <summary>
		/// Returns the number of 0's until the given position
		/// </summary>
		int Rank0(int pos);
		/// <summary>
		/// Returns the number of 1's until the given position
		/// </summary>
		int Rank1 (int pos);
		/// <summary>
		/// Returns the position of the rank1-th enabled bit
		/// </summary>
		int Select1 (int rank1);
		/// <summary>
		/// Returns the position of the rank0-th enabled bit
		/// </summary>
		int Select0 (int rank0);
		/// <summary>
		/// Returns the bit (true=1 or false=0) at the given position
		/// </summary>
		bool Access (int pos);
		/// <summary>
		/// Asserts the equality.
		/// </summary>
		void AssertEquality (IRankSelect other);
	}
}
