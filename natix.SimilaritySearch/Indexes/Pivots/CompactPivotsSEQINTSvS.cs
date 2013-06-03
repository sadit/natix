//
//  Copyright 2012  Eric Sadit Tellez Avila
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class CompactPivotsSEQINTSvS : CompactPivotsSEQRANS
	{
		// public int SEARCHPIVS;

		public CompactPivotsSEQINTSvS () : base()
		{
		}

		public override void Build (LAESA idx, int num_pivs, int num_rings, SequenceBuilder seq_builder = null)
		{
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetIISeq(BitmapBuilders.GetSArray());
			}
			base.Build (idx, num_pivs, num_rings, seq_builder);
		}

		public override IResult SearchKNN (object q, int K, IResult res)
		{
			var m = this.PIVS.Count;
			//var max = Math.Min (this.SEARCHPIVS, m);
			var max = m;
			var P = new TopK<Tuple<double, float, float, Sequence>> (max);
			var A = new ushort[this.DB.Count];
			var _PIVS = (this.PIVS as SampleSpace).SAMPLE;
			for (int piv_id = 0; piv_id < m; ++piv_id) {
				var stddev = this.STDDEV [piv_id];
				var mean = this.MEAN [piv_id];
				var dqp = this.DB.Dist (q, this.PIVS [piv_id]);
                ++this.internal_numdists;
				var seq = this.SEQ [piv_id];
				A[_PIVS[piv_id]] = (ushort)max;
				res.Push(_PIVS[piv_id], dqp);
				var start_sym = this.Discretize (dqp, stddev, mean);
				var end_sym = this.Discretize (dqp, stddev, mean);
				var count = Math.Min(start_sym, Math.Abs(this.MAX_SYMBOL - end_sym));
				P.Push (count, Tuple.Create (dqp, stddev, mean, seq));
			}
			var queue = new Queue<IEnumerator<Bitmap>> ();
			foreach (var p in P.Items.Traverse()) {
				var tuple = p.Value;
				var it = this.IteratePartsKNN(res, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4).GetEnumerator();
				if (it.MoveNext()) {
					queue.Enqueue(it);
				}
			}
			int Isize = 0;
			while (queue.Count > 0) {
				var L = queue.Dequeue();
				var rs = L.Current;
				var count1 = rs.Count1;
				for (int i = 1; i <= count1; ++i) {
					var item = rs.Select1 (i);
					A [item]++;
					if (A [item] == max) {
						var dist = this.DB.Dist (q, this.DB [item]);
						res.Push (item, dist);
						++Isize;
					}
				}
			//	Console.WriteLine ("*** queue-count: {0}, count1: {1}, max: {2}, Isize: {3}", queue.Count, count1, max, Isize);
				if (L.MoveNext ()) {
					queue.Enqueue (L);
				}
			}
			return res;
		}

		public IEnumerable<Bitmap> IteratePartsKNN (IResult res, double dqp, float stddev, float mean, Sequence seq)
		{
			var sym = this.Discretize(dqp, stddev, mean);
			yield return seq.Unravel(sym);
			var left = sym - 1;
			var right = sym + 1;
			bool do_next = true;
			while (do_next) {
				do_next = false;
				var __left = this.Discretize(dqp - res.CoveringRadius, stddev, mean);
				if (0 <= left && __left <= left) {
					yield return seq.Unravel(left);
					--left;
					do_next = true;
				}
				var __right = this.Discretize(dqp + res.CoveringRadius, stddev, mean);
				if (right <= __right && right < seq.Sigma) {
					yield return seq.Unravel(right);
					++right;
					do_next = true;
				}
				/*Console.WriteLine ("left: {0}, right: {1}, __left: {2}, __right: {3}",
				                   left, right, __left, __right);*/
			}
		}

		public override IResult SearchRange (object q, double radius)
		{
			var m = this.PIVS.Count;
			var P = new TopK<Tuple<double, int, int, Sequence>> (m);
			for (int piv_id = 0; piv_id < m; ++piv_id) {
				var dqp = this.DB.Dist (q, this.PIVS [piv_id]);
                ++this.internal_numdists;
				var stddev = this.STDDEV [piv_id];
				var mean = this.MEAN [piv_id];
				var start_sym = this.Discretize (dqp - radius, stddev, mean);
				var seq = this.SEQ [piv_id];
				var end_sym = this.Discretize (dqp + radius, stddev, mean);
				var count = 0;
				var n = seq.Count;
				for (int s = start_sym; s <= end_sym; ++s) {
					count += seq.Rank (s, n - 1);
				}
				P.Push (count, Tuple.Create (dqp, start_sym, end_sym, seq));
			}
			HashSet<int> A = new HashSet<int>();
			HashSet<int> B = null;
			int I = 0;
			foreach (var p in P.Items.Traverse()) {
				var tuple = p.Value;
				// var dpq = tuple.Item1;
				var start_sym = tuple.Item2;
				var end_sym = tuple.Item3;
				var seq = tuple.Item4;
				for (int s = start_sym; s <= end_sym; ++s) {
					var rs = seq.Unravel(s);
					var count1 = rs.Count1;
					for (int i = 1; i <= count1; ++i) {
						if (B == null) {
							A.Add( rs.Select1(i) );
						} else {
							var pos = rs.Select1(i);
							if (A.Contains(pos)) {
								B.Add( pos );
							}
						}
					}
				}
				if (B == null) {
					B = new HashSet<int>();
				} else {
					A = B;
					B = new HashSet<int>();
				}
				++I;
			}
			// Console.WriteLine();
			B = null;
			var res = new Result(this.DB.Count, false);
			foreach (var docid in A) {
				var d = this.DB.Dist(this.DB[docid], q);
				if (d <= radius) {
					res.Push(docid, d);
				}
			}
			return res;
		}
	}
}

