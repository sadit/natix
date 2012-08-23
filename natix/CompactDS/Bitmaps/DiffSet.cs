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
//   Original filename: natix/CompactDS/Bitmaps/DiffSet.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class DiffSet : RankSelectBase
	{
		// static IIEncoder32 Coder = new EliasGamma();
		static int AccStart = -1;
		IIEncoder32 Coder;
		public BitStream32 Stream;
		int N;
		int M;
		protected short B = 31;
		IList<int> Samples;
		IList<long> Offsets;
		
		public DiffSet ()
		{
			this.Samples = new List<int> ();
			this.Offsets = new List<long> ();
			this.Stream = new BitStream32 ();
			this.Coder = new EliasDelta ();
		}
		
		public DiffSet (short B) : this()
		{
			this.B = B;
		}

		public override bool Access (int pos)
		{
			int found_pos;
			this.BackendAccessRank1 (pos, out found_pos, new BitStreamCtx ());
			return pos == found_pos;
		}
		
		public override int Count {
			get {
				return this.N;
			}
		}
		
		public override void Save (BinaryWriter W)
		{
			W.Write (this.N);
			W.Write (this.M);
			W.Write (this.B);
			// Console.WriteLine ("xxxxxx  save samples.count {0}. N: {1}, M: {2}, B: {3}", this.Samples.Count, this.N, this.M, this.B);
			PrimitiveIO<int>.WriteVector (W, this.Samples);
			PrimitiveIO<long>.WriteVector (W, this.Offsets);
			IEncoder32GenericIO.Save (W, this.Coder);
			this.Stream.Save (W);
		}
	
		public override void Load (BinaryReader R)
		{
			this.N = R.ReadInt32 ();
			this.M = R.ReadInt32 ();
			this.B = R.ReadInt16 ();
			int num_samples = this.M / this.B;
			this.Samples = new int[ num_samples ];
			this.Offsets = new long[ num_samples ];
			PrimitiveIO<int>.ReadFromFile (R, num_samples, this.Samples);
			PrimitiveIO<long>.ReadFromFile (R, num_samples, this.Offsets);
			// Console.WriteLine ("xxxxxx  load samples.count {0}. N: {1}, M: {2}, B: {3}", this.Samples.Count, this.N, this.M, this.B);
			this.Coder = IEncoder32GenericIO.Load (R);
			this.Stream = new BitStream32 ();
			this.Stream.Load (R);
		}
		
		public override void AssertEquality (IRankSelect _other)
		{
			DiffSet other = _other as DiffSet;
			if (this.N != other.N) {
				throw new ArgumentException ("DiffSet N difference");
			}
			if (this.M != other.M) {
				throw new ArgumentException ("DiffSet M difference");
			}
			if (this.B != other.B) {
				throw new ArgumentException ("DiffSet B difference");
			}
			Assertions.AssertIList<int> (this.Samples, other.Samples, "DiffSet Samples difference");
			Assertions.AssertIList<long> (this.Offsets, other.Offsets, "DiffSet Offsets difference");
			this.Stream.AssertEquality (other.Stream);
		}
		
		/// <summary>
		/// Write the sequence to the bitmap. DiffSet always commit items.
		/// </summary>
		public virtual void Commit ()
		{
			// nothing here
		}
		
		protected virtual void ResetReader ()
		{
			
		}

		protected virtual int ReadNext (BitStreamCtx ctx)
		{
			return Coder.Decode (this.Stream, ctx);
		}
		
		protected virtual void WriteNewDiff (int u)
		{
			Coder.Encode (this.Stream, u);
		}
		
		/// <summary>
		/// Returns the difference between Select1(rank) - Select1(rank-1)
		/// </summary>
		public int Select1Difference (int rank)
		{
			if (rank < 1) {
				return -1;
			}
			if (rank == 1) {
				return this.Select1 (rank);
			}
			
			BitStreamCtx ctx = new BitStreamCtx ();
			this.BackendSelect1 (rank - 1, ctx);
			return this.ReadNext (ctx);
		}

		/// <summary>
		/// Extract 'count' differences starting from 'start_index', it saves the output in 'output'.
		/// Returns the previous absolute value to start_index (start_index - 1), i.e. the reference
		/// </summary>
		public int ExtractFrom (int start_index, int count, IList<int> output)
		{
			int acc;
			BitStreamCtx ctx = new BitStreamCtx ();
			if (start_index == 0) {
				this.ResetReader ();
				acc = -1;
				ctx.Seek (0);
			} else {
				acc = this.BackendSelect1 (start_index, ctx);
			}
			for (int i = 0; i < count; i++) {
				int val = this.ReadNext (ctx);
				if (i < output.Count) {
					output[i] = val;
				} else {
					output.Add (val);
				}
			}
			return acc;
		}
		
		
		/// <summary>
		/// Returns the position of the rank-th enabled bit
		/// </summary>
		public override int Select1 (int rank)
		{
			return this.BackendSelect1 (rank, new BitStreamCtx());
		}

		/// <summary>
		/// Unlocked Select1 useful
		/// </summary>
		int BackendSelect1 (int rank, BitStreamCtx ctx)
		{
			if (rank < 1) {
				return -1;
			}
			this.ResetReader();
			int start_index = (rank - 1) / this.B;
			int acc;
			int left;
			if (start_index == 0) {
				acc = AccStart;
				ctx.Seek (0);
				left = rank;
			} else {
				acc = this.Samples[start_index - 1];
				ctx.Seek (this.Offsets[start_index - 1]);
				left = rank - start_index * this.B;
			}
			// int start_acc = acc;
			for (int i = 0; i < left; i++) {
				int read = this.ReadNext(ctx);
				acc += read;
			}
			return acc;
		}
		
		int SeqAccessRank1 (int acc, int pos, int max, out int found_pos, BitStreamCtx ctx)
		{
			int i = 0;
			while (i < max && acc < pos) {
				int u = this.ReadNext (ctx);
				if (acc + u > pos) {
					found_pos = acc;
					return i;
				}
				acc += u;
				i++;
			}
			found_pos = acc;
			return i;
		}
		
		public override int Rank1 (int pos)
		{
			int select_pos;
			int rank = this.BackendAccessRank1 (pos, out select_pos, new BitStreamCtx());
			return rank;
		}
		
		int BackendAccessRank1 (int pos, out int found_pos, BitStreamCtx ctx)
		{
			if (pos < 0) {
				found_pos = -1;
				return 0;
			}
			this.ResetReader ();
			int start_index = -1;
			if (this.Samples.Count > 0) {
				start_index = GenericSearch.FindFirst<int> (pos, this.Samples);
			}
			int count;
			if (start_index < 0) {
				ctx.Seek (0);
				count = Math.Min (this.B, this.M);
				return this.SeqAccessRank1 (AccStart, pos, count, out found_pos, ctx);
			}
			ctx.Seek (this.Offsets[start_index]);
			int rel_rank = (start_index + 1) * this.B;
			count = Math.Min (this.B, this.M - rel_rank);
			return rel_rank + this.SeqAccessRank1 (this.Samples[start_index], pos, count, out found_pos, ctx);
		}
		
		public override int Count1 {
			get {
				return this.M;
			}
		}
				
		public void Add (int current)
		{
			this.Add (current, this.Select1 (this.Count1));
		}
				
		/// <summary>
		/// Adds an (ordered) item to the set
		/// </summary>
		public virtual void Add (int current, int prev)
		{
			// Console.WriteLine ("Add current: {0}, prev: {1}", current, prev);
			this.AddItem (current, prev);
			this.Commit ();
		}
		
		/// <summary>
		/// Internal method to add an (ordered) item to the set
		/// </summary>
		protected void AddItem (int current, int prev)
		{
			if (current == 0) {
				prev = AccStart;
			}

			this.WriteNewDiff (current - prev);
			this.M++;
			if (this.M % this.B == 0) {
				this.Commit ();
				this.Samples.Add (current);
				this.Offsets.Add (this.Stream.CountBits);
				//Console.WriteLine ("ADDING SAMPLE M: {0}, current: {1}, prev: {2}, num-samples: {3}",
				//	this.M, current, prev, this.Samples.Count);
				}
			if (current >= this.N) {
				this.N = current + 1;
			}
		}
		
		public void Build (IList<int> orderedList, short b, IIEncoder32 coder = null)
		{
			var n = 0;
			if (orderedList.Count > 0) {
				n = orderedList [orderedList.Count - 1];
			}
			this.Build (orderedList, n, b, coder);
		}
		
		/// <summary>
		///  build methods
		/// </summary>
		public void Build (IEnumerable<int> orderedList, int n, short b, IIEncoder32 coder = null)
		{
			this.N = n;
			this.B = b;
			this.M = 0;
			if (coder == null) {
				coder = new EliasDelta ();
			}
			this.Coder = coder;
			int prev = -1;
			foreach (var current in orderedList) {
				this.Add (current, prev);
				prev = current;
			}
		}
	}
}


