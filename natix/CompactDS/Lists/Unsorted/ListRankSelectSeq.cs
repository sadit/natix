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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListRL2.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// Encodes an array or permutation using a compact representation. Specially useful for lists that
	/// exhibit large runs. It does not support neither consecutive equal items nor negative items.
	/// </summary>
	public class ListRankSelectSeq : ListGenerator<int>, ILoadSave
	{
		public IRankSelectSeq S;

		public ListRankSelectSeq()
		{
		}

		public override int Count {
			get {
				return this.S.Count;
			}
		}

		public virtual void Build (IList<int> inlist, int _maxvalue, SequenceBuilder seq_builder = null)
		{
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetWTM(4);
			}
			this.S = seq_builder(inlist, _maxvalue + 1);

		}

		public virtual void Load (BinaryReader Input)
		{
			this.S = RankSelectSeqGenericIO.Load(Input);
		}

		public virtual void Save (BinaryWriter Output)
		{
			RankSelectSeqGenericIO.Save(Output, this.S);
		}

		public override int GetItem (int index)
		{
			this.S.Access(index);
		}

		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}	
	}
}
