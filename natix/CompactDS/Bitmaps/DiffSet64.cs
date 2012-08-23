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
//   Original filename: natix/CompactDS/Bitmaps/DiffSet64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class DiffSet64 : RankSelectBase64
	{
		public class Context
		{
			public BitStreamCtxRL ctx;
			public long prev_arg;
			public long prev_res;

			public Context (long prev_arg, BitStreamCtxRL ctx)
			{
				this.ctx = ctx;
				this.prev_arg = prev_arg;
				this.prev_res = long.MinValue;
			}
		}

		static int AccStart = -1;
		public BitStream32 Stream;
		protected IIEncoder64 Coder;
		protected long N;
		protected int M;
		protected short B = 31;
		IList<long> Samples;
		IList<long> Offsets;
		
		public DiffSet64 ()
		{
			this.Samples = new List<long> ();
			this.Offsets = new List<long> ();
			this.Stream = new BitStream32 ();
			this.Coder = new EliasDelta64 ();
		}
		
		public DiffSet64 (short B) : this()
		{
			this.B = B;
		}

		public override bool Access(long pos)
		{
			long found_pos;
			this.BackendAccessRank1 (pos, out found_pos, new BitStreamCtxRL());
			return pos == found_pos;
		}
		
		public override long Count {
			get {
				return this.N;
			}
		}
		
		public override void Save (BinaryWriter Output)
		{
			Output.Write (this.N);
			Output.Write (this.M);
			Output.Write (this.B);
			IEncoder64GenericIO.Save (Output, this.Coder);
			// Console.WriteLine ("xxxxxx  save samples.count {0}. N: {1}, M: {2}, B: {3}", this.Samples.Count, this.N, this.M, this.B);
/*			if (this.Samples.Count > 32) {
				{
					var sa = new SArray64 ();
					sa.Build (this.Samples);
					sa.Save (Output);
				}
				{
					var sa = new SArray ();
					sa.Build (this.Offsets);
					sa.Save (Output);
				}
			} else {*/
				PrimitiveIO<long>.WriteVector (Output, this.Samples);
				PrimitiveIO<long>.WriteVector (Output, this.Offsets);
//			}
			this.Stream.Save (Output);
		}
	
		public override void Load (BinaryReader Input)
		{
			this.N = Input.ReadInt64 ();
			this.M = Input.ReadInt32 ();
			this.B = Input.ReadInt16 ();
			this.Coder = IEncoder64GenericIO.Load (Input);
			int num_samples = this.M / this.B;
/*			if (num_samples > 32) { 
				{
					var sa = new SArray64 ();
					sa.Load (Input);
					this.Samples = new SortedListRS64 (sa);
				}
				{
					var sa = new SArray ();
					sa.Load (Input);
					this.Offsets = new SortedListSArray (sa);
				}
			} else {*/
				this.Samples = new long[ num_samples ];
				this.Offsets = new long[ num_samples ];
				PrimitiveIO<long>.ReadFromFile (Input, num_samples, this.Samples);
				PrimitiveIO<long>.ReadFromFile (Input, num_samples, this.Offsets);
//			}
			this.Stream = new BitStream32 ();
			this.Stream.Load (Input);
		}
		
		public override void AssertEquality (IRankSelect64 _other)
		{
			DiffSet64 other = _other as DiffSet64;
			if (this.N != other.N) {
				throw new ArgumentException ("DiffSet64 N difference");
			}
			if (this.M != other.M) {
				throw new ArgumentException ("DiffSet64 M difference");
			}
			if (this.B != other.B) {
				throw new ArgumentException ("DiffSet64 B difference");
			}
			Assertions.AssertIList<long> (this.Samples, other.Samples, "DiffSet64 Samples difference");
			Assertions.AssertIList<long> (this.Offsets, other.Offsets, "DiffSet64 Offsets difference");
			this.Stream.AssertEquality (other.Stream);
		}
		
		/// <summary>
		/// Write the sequence to the bitmap. DiffSet always commit items.
		/// </summary>
		public virtual void Commit ()
		{
			// nothing here
		}
		
		protected virtual void ResetReader (BitStreamCtxRL ctx)
		{
			
		}

		protected virtual long ReadNext (BitStreamCtxRL ctx)
		{
			return Coder.Decode (this.Stream, ctx);
		}
		
		protected virtual void WriteNewDiff (long u)
		{
			Coder.Encode (this.Stream, u);
		}
		
		/// <summary>
		/// Returns the difference between Select1(rank) - Select1(rank-1)
		/// </summary>
		public long Select1Difference (int rank)
		{
			if (rank < 1) {
				return -1;
			}
			if (rank == 1) {
				return this.Select1 (rank);
			}
			var ctx = new BitStreamCtxRL ();
			this.BackendSelect1 (rank - 1, ctx);
			return this.ReadNext (ctx);
		}

		/// <summary>
		/// Extract 'count' differences starting from 'start_index', it saves the output in 'output'.
		/// Returns the previous absolute value to start_index (start_index - 1), i.e. the reference
		/// </summary>
		public long ExtractFrom (int start_index, int count, IList<long> output)
		{
			long acc;
			var ctx = new BitStreamCtxRL ();
			if (start_index == 0) {
				this.ResetReader (ctx);
				acc = -1;
				ctx.Seek (0);
			} else {
				acc = this.BackendSelect1 (start_index, ctx);
			}
			for (int i = 0; i < count; i++) {
				long val = this.ReadNext (ctx);
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
		public override long Select1 (long rank)
		{
			return this.BackendSelect1 ((int)rank, new BitStreamCtxRL ());
		}

		public override long Select1 (long rank, UnraveledSymbolXLB unraveled_ctx)
		{
			Context ctx = unraveled_ctx.ctx as Context;
			if (ctx == null) {
				var bctx = new BitStreamCtxRL ();
				unraveled_ctx.ctx = ctx = new Context (rank, bctx);
				ctx.prev_res = this.BackendSelect1 ((int)rank, ctx.ctx);
			} else {
				int left = (int)(rank - ctx.prev_arg);
				if (left < 0 || left > this.B * 2) {
					ctx.prev_res = this.BackendSelect1 ((int)rank, ctx.ctx);
				} else {
					for (int i = 0; i < left; i++) {
						long read = this.ReadNext (ctx.ctx);
						ctx.prev_res += read;
					}
				}
			}
			ctx.prev_arg = rank;
			return ctx.prev_res;
		}

		/// <summary>
		/// Unlocked Select1 useful to some lowlevel operations
		/// </summary>
		long BackendSelect1 (int rank, BitStreamCtxRL ctx)
		{
			if (rank < 1) {
				return -1;
			}
			this.ResetReader(ctx);
			int start_index = (rank - 1) / this.B;
			long acc;
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
				long read = this.ReadNext(ctx);
				acc += read;
			}
			return acc;
		}
		
		long SeqAccessRank1 (long acc, long pos, int max, out long found_pos, BitStreamCtxRL ctx)
		{
			int i = 0;
			while (i < max && acc < pos) {
				long u = this.ReadNext (ctx);
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
		
		public override long Rank1 (long pos)
		{
			long select_pos;
			long rank = this.BackendAccessRank1 (pos, out select_pos, new BitStreamCtxRL());
			return rank;
		}
		
		long BackendAccessRank1 (long pos, out long found_pos, BitStreamCtxRL ctx)
		{
			if (pos < 0) {
				found_pos = -1;
				return 0;
			}
			this.ResetReader (ctx);
			int start_index = -1;
			if (this.Samples.Count > 0) {
				start_index = GenericSearch.FindFirst<long> (pos, this.Samples);
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
		
		public override long Count1 {
			get {
				return this.M;
			}
		}
				
		public void Add (long current)
		{
			this.Add (current, this.Select1 (this.Count1));
		}
				
		/// <summary>
		/// Adds an (ordered) item to the set
		/// </summary>
		public virtual void Add (long current, long prev)
		{
			// Console.WriteLine ("Add current: {0}, prev: {1}", current, prev);
			this.AddItem (current, prev);
			this.Commit ();
		}
		
		/// <summary>
		/// Internal method to add an (ordered) item to the set
		/// </summary>
		protected void AddItem (long current, long prev)
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
		
		public void Build (IList<long> orderedList, short b, IIEncoder64 coder = null)
		{
			long n = 0;
			if (orderedList.Count > 0) {
				n = orderedList [orderedList.Count - 1];
			}
			this.Build (orderedList, n, b, coder);
		}
		
		/// <summary>
		///  build methods
		/// </summary>
		public void Build (IEnumerable<long> orderedList, long n, short b, IIEncoder64 coder = null)
		{
			this.N = n;
			this.B = b;
			this.M = 0;
			if (coder == null) {
				coder = new EliasDelta64 ();
			}
			this.Coder = coder;
			long prev = -1;
			foreach (var current in orderedList) {
				try {
					this.Add (current, prev);
					prev = current;
				} catch (Exception e) {
					Console.WriteLine (e.ToString ());
					Console.WriteLine (e.StackTrace);
					throw e;
				}
			}
			this.Commit ();
		}
	}
}


