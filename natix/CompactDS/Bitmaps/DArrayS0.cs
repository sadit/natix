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
//   Original filename: natix/CompactDS/Bitmaps/DArrayS0.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class DArrayS0 : IRankSelect
	{
		DArray S0;
		DArray S1;
		
		public DArrayS0 ()
		{
		}
		
		public int Count {
			get {
				 return this.S1.Count;
			}
		}
		
		public int Count1 {
			get { return this.Rank1 (this.Count - 1); }
		}

		public bool Access(int i)
		{
			return this.S1.Access(i);
		}
		
		public void AssertEquality (IRankSelect obj)
		{
			var other = obj as DArrayS0;
			this.S0.AssertEquality (other.S0);
			this.S1.AssertEquality (other.S1);
		}
		
		public void Build (IBitStream bstream, short Brank, int Bselect)
		{
			this.BuildBackend (bstream.GetIList32 (), Brank, Bselect, (int)bstream.CountBits);
		}
		
		ListGen<uint> GetCompList (IList<uint> bitblocks)
		{
			return new ListGen<uint> ((int i) => ~bitblocks[i], bitblocks.Count);
		}
		
		public void BuildBackend (IList<uint> bitblocks, short Brank, int Bselect, int N)
		{
			this.S1 = new DArray ();
			this.S1.BuildBackend (bitblocks, Brank, Bselect, N);
			this.S0 = new DArray ();
			this.S0.BuildBackend (this.GetCompList (bitblocks), (short)(Brank * 12), Bselect, N);
		}
		
		public int Select1 (int rank)
		{
			return this.S1.Select1 (rank);
		}

		public int Rank0 (int I)
		{
			return this.S1.Rank0 (I);
		}

		public int Rank1 (int I)
		{
			return this.S1.Rank1 (I);
		}

		public int Select0 (int rank)
		{
			return this.S0.Select1 (rank);
		}
		
		public void Save (BinaryWriter bw)
		{
			this.S1.Save (bw);
			this.S0.Save (bw, false);
			
		}

		public void Load (BinaryReader br)
		{
			this.S1 = new DArray ();
			this.S1.Load (br);
			this.S0 = new DArray ();
			this.S0.Load (br, false);
			this.S0.SetBitBlocks (this.GetCompList (this.S1.GetBitBlocks ()));
		}
	}
}

