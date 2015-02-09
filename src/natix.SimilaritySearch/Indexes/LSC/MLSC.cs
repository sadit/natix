//
//   Copyright 2012, 2013, 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
//  2014-09-24 Added support for query expansion

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

		public override IResult SearchKNN (object q, int K, IResult R)
		{
			var cand = new HashSet<int> ();
			foreach (var lsc in this.lsc_indexes) {
				lsc.GetCandidates (lsc.ComputeHash(q), cand);
			}

			foreach (var docId in cand) {
				double dist = this.DB.Dist (this.DB [docId], q);
				R.Push (docId, dist);
			}
			return R;
		}
	}
}
