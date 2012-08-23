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
//   Original filename: natix/CompactDS/Permutations/ListGen_MRRR.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	/// <summary>
	/// A CyclicPerms without saving the main permutation PERM, it will be assigned.
	/// </summary>
	public class ListGen_MRRR : CyclicPerms_MRRR
	{
		
		public ListGen_MRRR () : base()
		{
		}

		public ListGen_MRRR (IList<int> perm, int t) : base(perm, t)
		{
		}
		
		public void SetPERM (IList<int> perm)
		{
			this.PERM = perm;
		}
	
		protected override void FinishBuild (object arg)
		{
			// To be used by derived classes
		}

		public override void Save (BinaryWriter Output)
		{
			//ListIGenericIO.Save (Output, this.PERM);
			ListIGenericIO.Save (Output, this.BACK);
			RankSelectGenericIO.Save (Output, this.has_back);
		}
		
		public override void Load (BinaryReader Input)
		{
			//this.PERM = ListIGenericIO.Load (Input);
			this.BACK = ListIGenericIO.Load (Input);
			this.has_back = RankSelectGenericIO.Load (Input);
		}		
	}
}

