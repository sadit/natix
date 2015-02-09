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
//  2014-09-24 Added query expansion

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using natix.CompactDS;

namespace natix.SimilaritySearch
{

	/// <summary>
	/// Abstract class for locality sensitive hashing
	/// </summary>
	public abstract class LSH : BasicIndex
	{
		/// <summary>
		/// Matrix. One vector per LSH function 
		/// </summary>
		protected InvertedIndex invindex;
		public static int DEFAULT_QUERY_EXPANSION = 0;
		public int Width = 0;

		/// <summary>
		/// Constructor
		/// </summary>
		public LSH () : base()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.Width = Input.ReadInt32 ();
			this.invindex = GenericIO<InvertedIndex>.Load (Input);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write(this.Width);
			GenericIO<InvertedIndex>.Save (Output, this.invindex);
		}

		public virtual void Build (MetricDB db, int width, Random rand, Func<InvertedIndex,InvertedIndex> create_invertedindex = null, Func<int,object> get_item = null)
		{
			this.DB = db;
			this.Width = width;

			int len = this.DB.Count;
			int pc = len / 100 + 1;
			int numbits = width > 32 ? 32 : width;

			Plain64InvertedIndex table = new Plain64InvertedIndex ();
			table.Initialize (1 << numbits);
			int maxhash = 0;

			this.PreBuild (rand, this.DB [0]);
			for (int objID = 0; objID < len; objID++) {
				if (objID % pc == 0) {
					Console.WriteLine ("Advance: {0:0.00}%, docid: {1}, total: {2}", objID * 100.0 / len, objID, len);
				}
				int hash;
				if (get_item == null) {
					hash = this.ComputeHash (this.DB [objID]);
				} else {
					hash = this.ComputeHash (get_item (objID));
				}

				table.AddItem(hash, objID);
				if (hash > maxhash) {
					maxhash = hash;
				}
			}

			table.Trim (maxhash + 1);
			if (create_invertedindex == null) {
				this.invindex = table;
			} else {
				this.invindex = create_invertedindex (table);
			}
		}

		public abstract void PreBuild (Random rand, object firstObject);
		public abstract int ComputeHash (object u);

		public virtual void GetCandidates (int hash, HashSet<int> cand, int expansion = -1)
		{
			if (expansion < 0) {
				expansion = DEFAULT_QUERY_EXPANSION;
			}

			foreach (var u in this.invindex[hash]) {
				cand.Add ((int) u);
			}

			if (expansion > 0) {
				var max = Math.Min (this.Width, 32);
				for (int i = 0; i < max; ++i) {
					this.GetCandidates (hash ^ (1 << i), cand, expansion - 1);
				}
			}
		}

		public override IResult SearchKNN (object q, int K, IResult R)
		{
			this.SearchKNNExpansion (q, R, DEFAULT_QUERY_EXPANSION);
			return R;
		}
	
		public virtual void SearchKNNExpansion (object q, IResult R, int expansion)
		{
			var cand = new HashSet<int> ();
			this.GetCandidates (this.ComputeHash (q), cand, expansion);

			foreach (var docId in cand) {
				double dist = this.DB.Dist (this.DB [docId], q);
				R.Push (docId, dist);
			}
		}
	}
}
