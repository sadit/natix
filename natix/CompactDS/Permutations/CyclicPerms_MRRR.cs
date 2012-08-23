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
//   Original filename: natix/CompactDS/Permutations/CyclicPerms_MRRR.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class CyclicPerms_MRRR : ListGenerator<int>, IPermutation
	{
		protected IList<int> PERM; // permutation
		protected IList<int> BACK;
		protected IRankSelect has_back; // bitmap marking if each items has a back pointer
		
		public CyclicPerms_MRRR () : base()
		{
		}

		public CyclicPerms_MRRR (IList<int> perm, int t)
		{
			this.Build (perm, t, null);
		}
		
		public void Build (IList<int> perm, int t, object arg_finish_build = null)
		{
			t = Math.Max (t, 2);
			this.PERM = perm;
			int n = this.PERM.Count;
			var visited = new BitStream32 ();
			visited.Write (false, n);
			this.BACK = new List<int> ();
			var back_indexes = new List<int> ();
			for (int i = 0; i < n; i++) {
				if (visited [i]) {
					continue;
				}
				int c = 0;
				int prev_j_mod_t = -1;
				// int prev_j = -1;
				for (int j = i; visited[j] == false; c++) {
					visited [j] = true;
					if (c % t == 0) {
						if (c > 0) {
							this.BACK.Add (prev_j_mod_t);
							back_indexes.Add (j);
						}
						prev_j_mod_t = j;
					}
					// prev_j = j;
					j = this.PERM [j];
				}
				if (c >= t) {
					int j = i;
					while (true) {
						// Console.WriteLine ("c: {0}, t: {1}, j: {2}, i: {3}", c, t, j, i);
						if (c % t == 0) {
							this.BACK.Add (prev_j_mod_t);
							back_indexes.Add (j);
							break;
						}
						c++;
						// prev_j = j;
						j = this.PERM [j];
					}
				}
				/*
				Console.WriteLine ("#backs: {0}", this.BACK.Count);
				Console.WriteLine ("visited: {0}", visited);
				for (int x = 0; x < this.BACK.Count; x++) {
					Console.WriteLine ("back: {0}, current: {1}", this.BACK [x], back_indexes [x]);
				}
				Console.WriteLine ();*/				
			}
			Sorting.Sort<int,int> (back_indexes, this.BACK);
			visited.Clear ();
			visited.Write (false, n);
			foreach (int i in back_indexes) {
				visited [i] = true;
			}
			var bitmap = new GGMN ();
			bitmap.Build (visited, 20);
			this.has_back = bitmap;
			/*for (int i = 0; i < n; i++) {
				Console.Write ("{0}, ", this.PERM [i]);
			}
			Console.WriteLine ("<END>");
			for (int i = 0; i < this.BACK.Count; i++) {
				Console.Write ("{0}, ", this.BACK [i]);
			}
			Console.WriteLine ("<END>");
			Console.WriteLine ("HAS_BACK: " + visited.ToString());
			*/
			this.FinishBuild (arg_finish_build);
		}
	
		protected virtual void FinishBuild (object arg)
		{
			// To be used by derived classes
		}

		public virtual void Save (BinaryWriter Output)
		{
			ListIGenericIO.Save (Output, this.PERM);
			ListIGenericIO.Save (Output, this.BACK);
			/*Output.Write ((int)this.PERM.Count);
			PrimitiveIO<int>.WriteVector (Output, this.PERM);
			Output.Write ((int)this.BACK.Count);
			PrimitiveIO<int>.WriteVector (Output, this.BACK);*/
			RankSelectGenericIO.Save (Output, this.has_back);
		}
		
		public virtual void Load (BinaryReader Input)
		{
			this.PERM = ListIGenericIO.Load (Input);
			this.BACK = ListIGenericIO.Load (Input);
			/*var n = Input.ReadInt32 ();
			this.PERM = new int[n];
			PrimitiveIO<int>.ReadFromFile (Input, n, this.PERM);
			var b = Input.ReadInt32 ();
			this.BACK = new int[b];
			PrimitiveIO<int>.ReadFromFile (Input, b, this.BACK);*/
			this.has_back = RankSelectGenericIO.Load (Input);
		}
		
		public override int Count {
			get {
				return this.PERM.Count;
			}
		}
		
		public override int GetItem (int index)
		{
			return this.PERM [index];
		}
		
		public override void SetItem (int index, int u)
		{
			throw new NotSupportedException ("SetItem");
		}
		
		public int Inverse (int i)
		{
			int j = i;
			bool first_step = true;
			while (i != this.PERM[j]) {
				// Console.WriteLine ("i: {0}, j: {1}, perm(j): {2}", i, j, this.PERM[j]);
				if (first_step && this.has_back.Access (j)) {
					j = this.BACK [this.has_back.Rank1 (j) - 1];
					first_step = false;
				} else {
					j = this.PERM [j];
				}
			}
			return j;
		}
	}
}

