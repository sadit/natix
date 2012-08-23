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
//   Original filename: natix/CompactDS/Sequences/UnraveledSymbolGolynskiRL.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// Unraveled symbol for GolynskiRL sequence index
	/// </summary>
	public class UnraveledSymbolGolynskiRL : RankSelectBase
	{
		GolynskiListRL2Seq seqindex;
		int symbol;
		BitStreamCtx ctx = new BitStreamCtx(-1);
		int run_len = 0;
		int count1 = -1;
		int prev_select_arg = int.MinValue;
		int prev_select_value = int.MinValue;
		ListSDiffCoderRL list = null;

		#region RANKSELECTBASEMETHODS
		/// <summary>
		/// Asserts the equality.
		/// </summary>
		public override void AssertEquality (IRankSelect other)
		{
			throw new NotSupportedException ();
		}
		
		/// <summary>
		/// Load
		/// </summary>
		public override void Load (BinaryReader br)
		{
			throw new NotSupportedException ();
		}
		
		/// <summary>
		/// Saves
		/// </summary>
		public override void Save (BinaryWriter bw)
		{
			throw new NotSupportedException ();
		}
		
			
		public override int Count {
			get {
				return this.seqindex.Count;
			}
		}
		
		public override int Count1 {
			get {
				if (this.count1 == -1) {
					this.count1 = this.seqindex.Rank (this.symbol, this.Count - 1);
				}
				return this.count1;
			}
		}
		
		public override bool this[int pos]
		{
			get {
				return this.symbol == this.seqindex.Access (pos);
			}
		}
	
#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.CompactDS.UnraveledSymbolGolynskiRL"/> class.
		/// </summary>
		public UnraveledSymbolGolynskiRL (GolynskiListRL2Seq _seqindex, int _symbol)
		{
			this.seqindex = _seqindex;
			this.symbol = _symbol;
			this.ctx = new BitStreamCtx ();
			this.list = this.seqindex.GetPERM ().GetListRL2 ().Diffs;
		}
		
		/// <summary>
		/// Rank1
		/// </summary>
		public override int Rank1 (int pos)
		{
			return this.seqindex.Rank (this.symbol, pos);
		}
		
		// select0 is a little bit harder, solving using Rank1 and binary search (base class)
		/// <summary>
		/// Select1
		/// </summary>

		public override int Select1 (int rank)
		{
			// Console.WriteLine ("A SELECT rank {0}, rl: {1}, ctx: {2}, symbol: {3}", rank, run_len, ctx.Offset, symbol);
			// useful for union/intersection/t-threshold algorithms
			if (this.prev_select_arg == rank) {
				return this.prev_select_value;
			}
			// Console.WriteLine ("B SELECT rank {0}, rl: {1}, ctx: {2}, symbol: {3}", rank, run_len, ctx.Offset, symbol);
			// useful with selects emulating an iteration of lists
			if (this.ctx.Offset >= 0 && this.prev_select_arg + 1 == rank) {
				this.prev_select_arg = rank;
				this.prev_select_value += this.list.GetNext (ctx, ref this.run_len);
				// Console.WriteLine ("C SELECT rank {0}, rl: {1}, ctx: {2}, symbol: {3}", rank, run_len, ctx.Offset, symbol);
			} else {
				this.ctx.Offset = -1;
				this.run_len = 0;
				this.prev_select_arg = rank;
				this.prev_select_value = this.seqindex.SelectRL (this.symbol, rank, this.ctx, ref this.run_len);
				// Console.WriteLine ("D SELECT rank {0}, rl: {1}, ctx: {2}, symbol: {3}", rank, run_len, ctx.Offset, symbol);
			}
			return this.prev_select_value;
		}
	}
}

