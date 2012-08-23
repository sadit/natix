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
//   Original filename: natix/CompactDS/Sequences/RankSelectFromRS64.cs
// using System;

namespace natix.CompactDS
{
	public class RankSelectFromRS64 : IRankSelect
	{
		IRankSelect64 rs;
		public RankSelectFromRS64 (IRankSelect64 rs)
		{
			this.rs = rs;
		}
		
		public int Count {
			get {
				return (int)this.rs.Count;
			}
		}
		
		public int Count1 {
			get {
				return (int)this.rs.Count1;
			}
		}
		
		public void AssertEquality (IRankSelect other)
		{
			throw new System.NotImplementedException ();
		}
		
		public void Load (System.IO.BinaryReader Input)
		{
			throw new System.NotImplementedException ();
		}
		
		public void Save (System.IO.BinaryWriter Output)
		{
			throw new System.NotImplementedException ();
		}
		
		public int Select0 (int rank0)
		{
			return (int)this.rs.Select0 (rank0);
		}
		
		public int Select1 (int rank1)
		{
			return (int)this.rs.Select1 (rank1);
		}
		
		public int Rank1 (int pos)
		{
			return (int)this.rs.Rank1 (pos);
		}
		
		public int Rank0 (int pos)
		{
			return (int)this.rs.Rank0 (pos);
		}
		
		public bool Access (int pos)
		{
			return this.rs.Access (pos);
		}
	}
}

