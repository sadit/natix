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
		public IList<LC_RNN> LC_LIST;
		public IUnionIntersection UI_ALG;

		public PolyIndexLC ()
		{
		}
		
		public IList<LC_RNN> GetIndexList ()
		{
			return this.LC_LIST;
		}

		public void Build (IList<LC_RNN> indexlist, int max_instances=int.MaxValue)
		{
			this.LC_LIST = new List<LC_RNN> ();
			max_instances = Math.Min(max_instances, indexlist.Count);
			for (int i = 0; i < max_instances; ++i) {
				this.LC_LIST.Add(indexlist[i]);
			}
		}

		public override MetricDB DB {
			get {
				return this.LC_LIST[0].DB;
			}
			set {
			}
		}
		public override void Save (BinaryWriter Output)
		{
			Output.Write((int) this.LC_LIST.Count);
			for (int i = 0; i < this.LC_LIST.Count; ++i) {
				IndexGenericIO.Save(Output, this.LC_LIST[i]);
			}
		}

		public override void Load (BinaryReader Input)
		{
			var count = Input.ReadInt32 ();
			this.LC_LIST = new LC_RNN [count];
			for (int i = 0; i < count; ++i) {
				this.LC_LIST[i] = (LC_RNN)IndexGenericIO.Load(Input);
			}
			this.UI_ALG = new FastUIArray8 (this.DB.Count);
		}

		public override IResult SearchRange (object q, double radius)
		{
			IList<IList<IRankSelect>> M = new List<IList<IRankSelect>> ();
			IResult R = this.DB.CreateResult (this.DB.Count, false);
			var cache = new Dictionary<int,double> ();
			foreach (var I in this.LC_LIST) {
				var L = I.PartialSearchRange (q, radius, R, cache);
				M.Add (L);
			}
			var C = this.UI_ALG.ComputeUI (M);
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
			foreach (var I in this.LC_LIST) {
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
