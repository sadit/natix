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
//   Original filename: natix/CompactDS/PermutationBuilders.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public delegate IPermutation PermutationBuilder (IList<int> perm);
	
	public class PermutationBuilders
	{
		public static PermutationBuilder GetCyclicPerms (int t, ListIBuilder listperm_builder = null,
		                                                 ListIBuilder listback_builder = null)
		{
			return delegate (IList<int> perm) {
				var P = new CyclicPerms_MRRR ();
				P.Build (perm, t, listperm_builder, listback_builder);
				return P;
			};
		}

		public static PermutationBuilder GetCyclicPermsListIFS (int t)
		{
			return delegate (IList<int> perm) {
				var P = new CyclicPerms_MRRR ();
				var builder = ListIBuilders.GetListIFS();
				P.Build (perm, t, builder, builder);
				return P;
			};
		}

		public static PermutationBuilder GetCyclicPermsListRL (int t)
		{
			return delegate (IList<int> perm) {
				var P = new CyclicPerms_MRRR ();
				P.Build (perm, t, ListIBuilders.GetListRL(), ListIBuilders.GetListIFS());
				return P;
			};
		}

		public static PermutationBuilder GetCyclicPermsListIRS64 (int t, BitmapFromList64 bb = null)
		{
			return delegate (IList<int> perm) {
				var P = new CyclicPerms_MRRR ();
				P.Build (perm, t, ListIBuilders.GetListIRS64(bb), ListIBuilders.GetListIFS());
				return P;
			};
		}

		public static PermutationBuilder GetCyclicPermsListIDiffs (int t, short bsize,
		                                                           BitmapFromBitStream marks_builder = null,
		                                                           IIEncoder32 encoder = null)
		{
			return delegate (IList<int> perm) {
				var P = new CyclicPerms_MRRR ();
				var permbuilder = ListIBuilders.GetListIDiffs(bsize, marks_builder, encoder);
				var backbuilder = ListIBuilders.GetListIFS();
				P.Build (perm, t, permbuilder, backbuilder);
				return P;
			};
		}

//		public static PermutationBuilder GetSuccCyclicPerms (int t)
//		{
//			return delegate (IList<int> perm) {
//				var P = new SuccCyclicPerms_MRRR ();
//				P.Build (perm, t);
//				return P;
//			};
//		}
//		
//		public static PermutationBuilder GetRLCyclicPerms (int t)
//		{
//			return delegate (IList<int> perm) {
//				var P = new RLCyclicPerms_MRRR ();
//				P.Build (perm, t);
//				return P;
//			};
//		}
//		
//		public static PermutationBuilder GetSuccRLCyclicPerms (int t)
//		{
//			return delegate (IList<int> perm) {
//				var P = new SuccRLCyclicPerms_MRRR ();
//				P.Build (perm, t);
//				return P;
//			};
//		}
//
//		public static PermutationBuilder GetSuccRL2CyclicPerms (int t)
//		{
//			return delegate (IList<int> perm) {
//				var P = new SuccRL2CyclicPerms_MRRR ();
//				P.Build (perm, t, new SuccRL2CyclicPerms_MRRR.BuildParams ());
//				return P;
//			};
//		}
//		
//		public static PermutationBuilder GetSuccRL2CyclicPerms (int t, IIEncoder32 coder, short block_size)
//		{
//			return delegate (IList<int> perm) {
//				var P = new SuccRL2CyclicPerms_MRRR ();
//				P.Build (perm, t, new SuccRL2CyclicPerms_MRRR.BuildParams (coder, block_size));
//				return P;
//			};
//		}

		public static PermutationBuilder GetPlainPerms ()
		{
			return delegate (IList<int> perm) {
				var P = new PlainPerms ();
				P.Build (perm);
				return P;
			};
		}
		
	}
}

