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
//   Original filename: natix/CompactDS/Sequences/IRankSelectSeq.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// The protocol of an index of sequences
	/// </summary>
	public interface IRankSelectSeq : ILoadSave
	{		
		/// <summary>
		/// The length of the sequence
		/// </summary>
		int Count {
			get;
		}
		/// <summary>
		/// The size of the alphabet
		/// </summary>
		int Sigma {
			get;
		}
		/// <summary>
		/// Returns the rank of the symbol at the given position
		/// </summary>
		int Rank(int symbol, int pos);
		/// <summary>
		/// Returns the position where the given symbol has the given rank
		/// </summary>
		int Select (int symbol, int rank);
		/// <summary>
		/// Returns the symbol at the given position
		/// </summary>
		int Access (int pos);
		/// <summary>
		/// Returns a rank-select-access bitmap representation of the requested symbol.
		/// </summary>
		IRankSelect Unravel (int symbol);
	}
}
