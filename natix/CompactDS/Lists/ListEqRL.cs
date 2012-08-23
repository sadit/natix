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
//   Original filename: natix/CompactDS/Lists/ListEqRL.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class ListEqRL : ListGenerator<int>, ILoadSave
	{
		IRankSelect runs;
		IList<int> heads;
		
		
		public IList<int> Heads {
			get {
				return this.heads;
			}
		}
		
		public IRankSelect Runs {
			get {
				return this.runs;
			}
		}
		
		public ListEqRL ()
		{	
		}
		

		public void Build (IList<int> seq, int maxvalue)
		{
			var _heads = new ListIFS ((int)Math.Ceiling (Math.Log (maxvalue + 1, 2)));
			//var _heads = new List<int> ();
			var _runs = new BitStream32 ();
			int n = seq.Count;
			int prevc = seq [0];
			_runs.Write (true);
			_heads.Add (prevc);
			for (int i = 1; i < n; i++) {
				int c = seq [i];
				if (prevc == c) {
					_runs.Write (false);
				} else {
					_runs.Write (true);
					_heads.Add (c);
				}
				prevc = c;
			}
			// an additional 1 at the end
			// _runs.Write (true);
			var b = new GGMN ();
			b.Build (_runs, 16);
			this.runs = b;
			this.heads = _heads;
		}
		
		public void Load (BinaryReader Input)
		{
			this.heads = ListIGenericIO.Load (Input);
			this.runs = RankSelectGenericIO.Load (Input);
		}
		
		public void Save (BinaryWriter Output)
		{
			ListIGenericIO.Save (Output, this.heads);
			RankSelectGenericIO.Save (Output, this.runs);
		}
		
		public override int Count {
			get {
				return this.runs.Count;
			}
		}
		
		public override int GetItem (int index)
		{
			// var c = this.lens.Access (index);
			var rank1 = this.runs.Rank1 (index);
			return this.heads [rank1 - 1];
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotSupportedException();
		}
	}
}
