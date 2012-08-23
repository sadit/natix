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
//   Original filename: natix/CompactDS/Permutations/PlainPerms.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class PlainPerms : ListGenerator<int>, IPermutation
	{
		IList<int> P;
		IList<int> Inv;
		
		public PlainPerms ()
		{
			
		}
		
		public override int GetItem (int i)
		{
			return this.P [i];
		}

		public override void SetItem (int i, int v)
		{
			throw new NotSupportedException ();
		}
		
		public override int Count {
			get {
				return this.P.Count;
			}
		}
		
		public void Build (IList<int> perm)
		{
			this.P = perm;
			this.Inv = new int[this.P.Count];
			for (int i = 0; i < this.P.Count; i++) {
				this.Inv [this.P [i]] = i;
			}
		}

		public int Inverse (int i)
		{
			return this.Inv [i];
		}
		
		public void Load (BinaryReader Input)
		{
			int n = Input.ReadInt32 ();
			this.P = new int[ n ];
			this.Inv = new int[ n];
			PrimitiveIO<int>.ReadFromFile (Input, n, this.P);
			PrimitiveIO<int>.ReadFromFile (Input, n, this.Inv);
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)P.Count);
			PrimitiveIO<int>.WriteVector (Output, this.P);
			PrimitiveIO<int>.WriteVector (Output, this.Inv);
		}
	}
}

