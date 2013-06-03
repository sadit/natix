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
//   Original filename: natix/CompactDS/Sequences/UnraveledSymbol.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// Unraveled symbol 
	/// </summary>
	public class UnraveledSymbol : Bitmap
	{
		public Sequence seqindex;
		public int symbol;
	
		/// <summary>
		/// Asserts the equality.
		/// </summary>
		public override void AssertEquality (Bitmap other)
		{
			throw new NotSupportedException ();
		}
		
		/// <summary>
		/// Load from "Input"
		/// </summary>
		public override void Load (BinaryReader Input)
		{
			throw new NotSupportedException ();
		}
		
		/// <summary>
		/// Saves to "Output"
		/// </summary>

		public override void Save (BinaryWriter Output)
		{
			throw new NotSupportedException ();
		}
		
		/// <summary>
		/// Creates an unraveled symbol using "_symbol" over "_seqindex"
		/// </summary>

		public UnraveledSymbol (Sequence _seqindex, int _symbol)
		{
			this.seqindex = _seqindex;
			this.symbol = _symbol;
		}
		
		/// <summary>
		/// The number of bits in this bitmap
		/// </summary>
		public override int Count {
			get {
				return this.seqindex.Count;
			}
		}
		
		/// <summary>
		/// The number of enabled bits in this bitmap
		/// </summary>
		public override int Count1 {
			get {
				return this.seqindex.Rank (this.symbol, this.Count - 1);
			}
		}
		
		/// <summary>
		/// Access to the bit at position "pos"
		/// </summary>
		public override bool Access(int pos)
		{
			return this.symbol == this.seqindex.Access (pos);
		}
		
		
		/// <summary>
		/// Rank1
		/// </summary>
		public override int Rank1 (int pos)
		{
			return this.seqindex.Rank (this.symbol, pos);
		}
		
		/// <summary>
		/// Select1 
		/// </summary>
		public override int Select1 (int rank)
		{
			return this.seqindex.Select (this.symbol, rank);
		}

	}
}

