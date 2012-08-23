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
//   Original filename: natix/SimilaritySearch/Indexes/MLSC.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NDesk.Options;
using natix.CompactDS;

namespace natix.SimilaritySearch
{

	/// <summary>
	/// Multiple locality sensitive hashing sequences
	/// </summary>
	public abstract class MLSC : BasicIndex
	{
		protected LSC[] lsc_indexes;
		/// <summary>
		/// Constructor
		/// </summary>
		public MLSC () : base()
		{
		}
		
		public LSC[] Indexes
		{
			get {
				return this.lsc_indexes;
			}
		}
		
		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var count = Input.ReadInt32 ();
			this.lsc_indexes = new LSC[count];
			for (int i = 0; i < count; ++i) {
				this.lsc_indexes[i] = (LSC) IndexGenericIO.Load(Input);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			var count = this.lsc_indexes.Length;
			Output.Write ((int)count);
			for (int i = 0; i < count; ++i) {
				IndexGenericIO.Save(Output, this.lsc_indexes[i]);
			}
		}

		public override IResult SearchRange (object q, double radius)
		{
			return this.FilterByRadius (this.SearchKNN (q, 1024), radius);
		}
		
		public override IResult SearchKNN (object q, int K, IResult R)
		{
			// var seq_base = this.lsc_indexes [0].GetSeq () as SeqXLB;
			//if (seq_base == null) {
			//	throw new ArgumentNullException ("Currently only SeqXLB instances are allowed");
			//}
			// var perm = seq_base.GetPERM ();
			var Q = this.lsc_indexes [0].GetCandidates (q);
			for (int i = 1; i < this.lsc_indexes.Length; ++i) {
				var P = this.lsc_indexes [i].GetCandidates (q);
				foreach (var item in P) {
					//Q.Add (perm [item]);
					Q.Add (item);
				}
			}
			if (K < 0) {
				foreach (var docId in Q) {
					R.Push (docId, -1);
				}
			} else {
				foreach (var docId in Q) {
					double dist = this.DB.Dist (this.DB [docId], q);
					R.Push (docId, dist);
				}
			}
			return R;
		}
	}
}
