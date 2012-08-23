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
//   Original filename: natix/SimilaritySearch/Indexes/PolyIndexLC.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;
using natix.Sets;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	public class PolyIndexLC : BasicIndex
	{
		protected IList<LC_RNN> lc_list;
		protected IUnionIntersection ui_alg;
		
		public PolyIndexLC ()
		{
		}
		
		public IList<LC_RNN> GetIndexList ()
		{
			return this.lc_list;
		}
				
		public  void Build (string[] indexlist)
		{
			this.lc_list = new List<LC_RNN> (indexlist.Length);
			foreach (var name in indexlist) {
				this.lc_list.Add((LC_RNN)IndexGenericIO.Load(name));
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write((int) this.lc_list.Count);
			for (int i = 0; i < this.lc_list.Count; ++i) {
				IndexGenericIO.Save(Output, this.lc_list[i]);
			}
		}
		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.ui_alg = new FastUIArray8 (this.DB.Count);
			var count = Input.ReadInt32 ();
			this.lc_list = new LC_RNN [count];
			for (int i = 0; i < count; ++i) {
				this.lc_list[i] = (LC_RNN)IndexGenericIO.Load(Input);
			}
		}

		public override IResult SearchRange (object q, double radius)
		{
			IList<IList<IRankSelect>> M = new List<IList<IRankSelect>> ();
			IResult R = this.DB.CreateResult (this.DB.Count, false);
			var cache = new Dictionary<int,double> ();
			foreach (var I in this.lc_list) {
				var L = I.PartialSearchRange (q, radius, R, cache);
				M.Add (L);
			}
			var C = this.ui_alg.ComputeUI (M);
			foreach (int docid in C) {
				var dist = this.DB.Dist (q, this.DB [docid]);
				if (dist <= radius) {
					R.Push (docid, dist);
				}
			}
			return R;
		}
		
		public override IResult SearchKNN (object q, int K, IResult R)
		{
			byte[] A = new byte[ this.DB.Count ];
			var queue = new Queue<IEnumerator<IRankSelect>> ();
			var cache = new Dictionary<int,double> ();
			foreach (var I in this.lc_list) {
				var L = I.PartialSearchKNN (q, K, R, cache).GetEnumerator ();
				if (L.MoveNext ()) {				
					queue.Enqueue (L);
				}
			}
			int max = queue.Count;
			while (queue.Count > 0) {
				var L = queue.Dequeue ();
				// foreach (var item in L.Current) {
				var rs = L.Current;
				var count1 = rs.Count1;
				for (int i = 1; i <= count1; ++i) {
					var item = rs.Select1 (i);
					A [item]++;
					if (A [item] == max) {
						var dist = this.DB.Dist (q, this.DB [item]);
						R.Push (item, dist);
					}
				}
				if (L.MoveNext ()) {
					queue.Enqueue (L);
				}
			}
			return R;			
		}
	}
}
