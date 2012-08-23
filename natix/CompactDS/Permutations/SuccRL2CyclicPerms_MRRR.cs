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
//   Original filename: natix/CompactDS/Permutations/SuccRL2CyclicPerms_MRRR.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class SuccRL2CyclicPerms_MRRR : SuccCyclicPerms_MRRR 
	{
		public class BuildParams {
			public IIEncoder32 coder;
			public short block_size;
			public BuildParams (IIEncoder32 c, short b)
			{
				this.coder = c;
				this.block_size = b;
			}
			
			//public BuildParams () : this(new EliasDelta(), 127)
			public BuildParams () : this(new EliasGamma32(), 127)
			{
			}
		}
		
		public SuccRL2CyclicPerms_MRRR () : base()
		{
		}
		
		public SuccRL2CyclicPerms_MRRR (IList<int> perm, int t) : base(perm, t)
		{
		}
		
		public ListRL2 GetListRL2 ()
		{
			return (ListRL2) this.PERM;
		}

		protected override void FinishBuild (object args)
		{
			int maxvalue = this.PERM.Count - 1;
			var config = (BuildParams)args;
			this.PERM = this.BuildListRL2 (this.PERM, maxvalue, config);
			this.BACK = this.BuildSuccList (this.BACK, maxvalue);
		}
		
		protected IList<int> BuildListRL2 (IList<int> list, int maxvalue, BuildParams config)
		{
			var L = new ListRL2 ();
			// L.Build (list, maxvalue, new EliasGamma (), 63);
			L.Build (list, maxvalue, config.coder, config.block_size);
			return L;
		}
	}
}

