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
using NDesk.Options;
using natix.CompactDS;

namespace natix.SimilaritySearch
{

	/// <summary>
	/// Abstract class for locality sensitive hashing
	/// </summary>
	public abstract class LSC : BasicIndex
	{
		/// <summary>
		/// Matrix. One vector per LSH function 
		/// </summary>
		protected ushort[] H;
		protected Sequence Seq;
		public static int DEFAULT_QUERY_EXPANSION = 0;

		public Sequence GetSeq ()
		{
			return this.Seq;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public LSC () : base()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			var c = Input.ReadInt32 ();
			this.H = new ushort[c];
			PrimitiveIO<ushort>.LoadVector(Input, c, this.H);
			this.Seq = GenericIO<Sequence>.Load(Input);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write((int) this.H.Length);
			PrimitiveIO<ushort>.SaveVector(Output, this.H);
			GenericIO<Sequence>.Save(Output, this.Seq);
		}

		public virtual void Build (MetricDB db, int sampleSize,
		                           SequenceBuilder seq_builder = null, Func<int,object> get_item = null)
		{
			this.DB = db;
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqXLB_SArray64 (16);
			}
			this.H = new ushort[sampleSize];
			Random rand = new Random ();
			{
				HashSet<int> _coordinates = new HashSet<int> ();
				int i = 0;
				while (_coordinates.Count < sampleSize) {
					var p = (ushort)(rand.Next () % ushort.MaxValue);
					if (_coordinates.Add (p)) {
						this.H [i] = p;
						++i;
					}
				}
				Array.Sort (this.H);
			}
			int len = this.DB.Count;
			int pc = len / 100 + 1;
			int numbits = sampleSize > 32 ? 32 : sampleSize;
			var seq = new ListIFS (numbits);
			// Console.WriteLine ("DIMENSION: {0}, LENGTH: {1}", numbits, len);
			for (int docid = 0; docid < len; docid++) {
				if (docid % pc == 0) {
					Console.WriteLine ("Advance: {0:0.00}%, docid: {1}, total: {2}", docid * 100.0 / len, docid, len);
				}
				int hash;
				if (get_item == null) {
					hash = this.ComputeHash (this.DB [docid]);
				} else {
					hash = this.ComputeHash (get_item (docid));
				}
				// Console.WriteLine ("hash: {0}, max: {1}, sample-size: {2}", hash, 1 << sampleSize, sampleSize);
				seq.Add (hash);
			}
			Console.WriteLine ("*** Creating index of sequences");
			this.Seq = seq_builder (seq, 1 << numbits);
			// IndexLoader.Save(outname, this);
		}
				
		public abstract int ComputeHash (object u);

		public virtual void GetCandidates (int hash, HashSet<int> cand, int expansion = -1)
		{
			if (expansion < 0) {
				expansion = DEFAULT_QUERY_EXPANSION;
			}
			Bitmap L = this.Seq.Unravel (hash);
			var len = L.Count1;
			for (int i = 1; i <= len; i++) {
				cand.Add (L.Select1 (i));
			}
			if (expansion > 0) {
				var max = Math.Min (this.H.Length, 32);
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
			// K = -1;
			// Console.WriteLine ("q: {0}, K: {1}, len: {2}", q, K, len);
			foreach (var docId in cand) {
				double dist = this.DB.Dist (this.DB [docId], q);
				R.Push (docId, dist);
			}
		}
	}
}
