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
//   Original filename: natix/SimilaritySearch/Indexes/APMILC.cs
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
	public class PolyIndexLC_Approx : PolyIndexLC
	{
		// TODO min_threshold should be a % of the number of indexes, or a maximum number of errors,
		// TODO min_threshold should not be a constant
		// int max_threshold_error = 1;
		int max_cand = 1024;
		// TODO max_extension should not be a constant
		// TODO max_extension should be a function of the query's radii
		int max_extension = 16;
		
		// TODO Set a maximum number of distance evaluations, alternative constraint to min_threshold
		
		// TODO decide if we shrink query balls or we expand query balls.
		// Right now we are shrinking for range and expanding for knn
		public PolyIndexLC_Approx ()
		{
		}
		
		protected IList<IRankSelect> CutRSList (IList<IRankSelect> L_input)
		{
			var L_output = new List<IRankSelect> (this.max_extension);
			foreach (var rs in L_input) {
				L_output.Add (rs);
				if (L_output.Count == this.max_extension) {
					break;
				}
			}
			return L_output;
		}
		
		
		public override IResult SearchRange (object q, double radius)
		{
			IList<IList<IRankSelect>> M = new List<IList<IRankSelect>> ();
			IResult R = this.DB.CreateResult (this.DB.Count, false);
			byte[] A = new byte[ this.DB.Count ];
			var cache = new Dictionary<int,double> ();
			// var threshold = this.GetIndexList().Count - this.max_threshold_error;
			foreach (var I in this.lc_list) {
				var L = I.PartialSearchRange (q, radius, R, cache);
				M.Add (this.CutRSList (L));
				foreach (var rs in L) {
					var count1 = rs.Count1;
					for (int i = 1; i <= count1; ++i) {
						var docid = rs.Select1 (i);
						A [docid]++;
						/* if (A [docid] == threshold) {
							var dist = this.MainSpace.Dist (q, this.MainSpace [docid]);
							if (dist <= radius) {
								R.Push (docid, dist);
							}
						}*/
					}
				}
			}
			Result _cand = new Result (this.max_cand, false);
			for (int i = 0; i < A.Length; ++i) {
				if (A [i] > 0) {
					_cand.Push (i, -A [i]);
				}
			}
			foreach (var pair in _cand) {
				var dist = this.DB.Dist (q, this.DB [pair.docid]);
				if (dist <= radius) {
					R.Push (pair.docid, dist);
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
			int num_iterations = this.GetIndexList ().Count * this.max_extension;
			// int max = queue.Count;
			// var threshold = this.GetIndexList().Count - this.max_threshold_error;

			while (queue.Count > 0) {
				var L = queue.Dequeue ();
				// foreach (var item in L.Current) {
				var rs = L.Current;
				var count1 = rs.Count1;
				for (int i = 1; i <= count1; ++i) {
					var item = rs.Select1 (i);
					A [item]++;
					/*if (A [item] == threshold) {
						var dist = this.MainSpace.Dist (q, this.MainSpace [item]);
						R.Push (item, dist);
					}*/
				}
				if (L.MoveNext ()) {
					queue.Enqueue (L);
				}
				if (num_iterations == 1) {
					break;
				}
				--num_iterations;
			}
			Result _cand = new Result (this.max_cand, false);
			for (int i = 0; i < A.Length; ++i) {
				if (A [i] > 0) {
					_cand.Push (i, -A [i]);
				}
			}
			// Console.WriteLine ("==== new query ");
			int pos = 0;
			foreach (var pair in _cand) {
				var dist = this.DB.Dist (q, this.DB [pair.docid]);
				R.Push (pair.docid, dist);
				// Console.WriteLine ("**** pos: {0}, cand docid: {1}, intersection: {2}, dist: {3}", pos, pair.docid, -pair.dist, dist);
				++pos;
			}
			return R;			
		}
	}
}
