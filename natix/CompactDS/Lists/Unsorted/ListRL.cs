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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListRL.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class ListRL : ListGenerator<int>, ILoadSave
	{
		protected ListIFS headers;
		protected IRankSelect runs;
		
		public IList<int> Headers {
			get {
				return this.headers;
			}
		}
		
		public IRankSelect Runs {
			get {
				return this.runs;
			}
		}
		
		public ListRL ()
		{
			this.BitmapBuilder = BitmapBuilders.GetGGMN_wt(12);
		}

		public BitmapFromBitStream BitmapBuilder {
			get;
			set;
		}

		public override int Count {
			get {
				return this.runs.Count;
			}
		}
		
		public override int GetItem (int index)
		{
			if (index >= this.Count) {
				throw new IndexOutOfRangeException ();
			}
			var runId = this.runs.Rank1 (index);
			var p = this.runs.Select1 (runId);
			return this.headers [runId - 1] + index - p;
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void Build (IList<int> list, int maxvalue)
		{
			int n = list.Count;
			var B_runs = new BitStream32 ();
			this.headers = new ListIFS ((int)Math.Ceiling (Math.Log (maxvalue + 1, 2)));
			if (list.Count > 0) {
				this.headers.Add (list [0]);
				B_runs.Write (true);
				for (int i = 1; i < n; i++) {
					if (list [i] == list [i - 1] + 1) {
						B_runs.Write (false);
					} else {
						this.headers.Add (list [i]);
						B_runs.Write (true);					
					}				
				}
			}
			var bb = new FakeBitmap ();
			bb.B = B_runs;
			/*
			Console.WriteLine ("%%%%%> ListRL B_runs: {0}", B_runs);
			Console.WriteLine ("%%%%%> ListRL Headers: ");
			foreach (var u in this.headers) {
				Console.Write (u.ToString () + ", ");
			}
			Console.WriteLine ("<end>");
			*/
			this.runs = this.BitmapBuilder (bb);
		}
		
		public virtual void Load (BinaryReader Input)
		{
			var L = new ListIFS ();
			L.Load (Input);
			this.headers = L;
			this.runs = RankSelectGenericIO.Load (Input);
		}

		public virtual void Save (BinaryWriter Output)
		{
			this.headers.Save (Output);
			RankSelectGenericIO.Save (Output, this.runs);
		}
	}
}
