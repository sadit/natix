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
//   Original filename: natix/CompactDS/Bitmaps/DiffSetRL2_64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{	
	/// <summary>
	/// A DiffSetRL2 supporting items on [0,2^63]
	/// </summary>
	public class DiffSetRL2_64 : RankSelectBase64
	{
		// static IIntegerEncoder Coder = new EliasDelta ();
		static int AccStart = -1;
		//static int PLAIN_SAMPLES_THRESHOLD = 32;
		protected IIEncoder64 Coder;
		protected BitStream32 Stream;
		protected long N;
		protected int M;
		protected short B = 31;
		IList<long> Samples;
		IList<long> Offsets;
		// int run_len = 0;
		
		public DiffSetRL2_64 ()
		{
			this.Samples = new List<long> ();
			this.Offsets = new List<long> ();
			this.Stream = new BitStream32 ();
			this.Coder = new EliasDelta64 ();
		}
		
		/*public DiffSetRL2_64 (short B, IIntegerEncoder) : this()
		{
			this.B = B;
		}*/

		
		public override long Count {
			get {
				return this.N;
			}
		}
		
		public override long Count1 {
			get {
				return this.M;
			}
		}

		public override void Save (BinaryWriter Output)
		{
			// this.Commit ();
			Output.Write ((long)this.N);
			Output.Write ((int)this.M);
			Output.Write ((short)this.B);
			IEncoder64GenericIO.Save (Output, this.Coder);
			//Console.WriteLine ("xxxxxx  save samples.count {0}. N: {1}, M: {2}, B: {3}, BitCount: {4}",
			//	this.Samples.Count, this.N, this.M, this.B, this.Stream.CountBits);
			/*if (this.Samples.Count > PLAIN_SAMPLES_THRESHOLD) {
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
			//}
			this.Stream.Save (Output);
		}
	
		public override void Load (BinaryReader Input)
		{
			// var POS = R.BaseStream.Position;
			this.N = Input.ReadInt64 ();
			this.M = Input.ReadInt32 ();
			this.B = Input.ReadInt16 ();
			this.Coder = IEncoder64GenericIO.Load (Input);
			int num_samples = this.M / this.B;
			/*if (num_samples > PLAIN_SAMPLES_THRESHOLD) {
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
			//}
			// POS = R.BaseStream.Position - POS;
			// Console.WriteLine("=======*******=======>> POS: {0}", POS);
			this.Stream = new BitStream32 ();
			this.Stream.Load (Input);
			//Console.WriteLine ("xxxxxx  load samples.count {0}. N: {1}, M: {2}, B: {3}, BitCount: {4}",
			//	this.Samples.Count, this.N, this.M, this.B, this.Stream.CountBits);

		}
		
		public override void AssertEquality (IRankSelect64 _other)
		{
			DiffSetRL2_64 other = _other as DiffSetRL2_64;
			if (this.N != other.N) {
				throw new ArgumentException ("DiffSetRL2_64 N difference");
			}
			if (this.M != other.M) {
				throw new ArgumentException ("DiffSetRL2_64 M difference");
			}
			if (this.B != other.B) {
				throw new ArgumentException ("DiffSetRL2_64 B difference");
			}
			Assertions.AssertIList<long> (this.Samples, other.Samples, "DiffSetRL2_64 Samples difference");
			Assertions.AssertIList<long> (this.Offsets, other.Offsets, "DiffSetRL2_64 Offsets difference");
			this.Stream.AssertEquality (other.Stream);
		}
		
		/// <summary>
		/// Returns the position of the rank-th enabled bit
		/// </summary>
		public override long Select1 (long rank)
		{
			//Console.WriteLine ("**** Select1> rank: {0}", rank);
			return this.BackendSelect1 ((int)rank, new BitStreamCtxRL ());
		}
		
		long ReadNext (BitStreamCtxRL ctx)
		{
			if (ctx.run_len > 0) {
				ctx.run_len--;
				return 1;
			}
			long d = Coder.Decode (this.Stream, ctx);
			if (d == 1) {
				ctx.run_len = ((int)Coder.Decode (this.Stream, ctx)) - 1;
			}
			return d;
		}
		
		bool IsFilled (int block_id)
		{
			if (block_id == 0) {
				return this.Samples [block_id] == (this.B - 1);
			}
			return (this.Samples [block_id] - this.Samples [block_id - 1]) == this.B;
		}

		long BackendSelect1 (int rank, BitStreamCtxRL ctx)
		{
			if (rank < 1) {
				return -1;
			}
			// reset run_len
			ctx.run_len = 0;
			int start_index = (rank - 1) / this.B;
			long acc;
			int left;
			//Console.WriteLine ("**** BaseSelect1> rank: {0}, start_index: {1}", rank, start_index);
			if (start_index == 0) {
				//if (this.Offsets.Count > 0 && this.Offsets [0] == 1) {
				if (this.Offsets.Count > 0 && this.IsFilled (0)) {
					//Console.WriteLine ("**** INSIDE FULL> ");
					ctx.run_len = this.B - rank;
					return rank - 1;
					// this.run_len = this.B;
				}
				//Console.WriteLine ("**** OUT-SIDE FULL> B: {0}", this.B);
				acc = AccStart;
				ctx.Seek (0);
				left = rank;
			} else {
				acc = this.Samples [start_index - 1];
				left = rank - start_index * this.B;
				//if (this.Offsets.Count > start_index && this.Offsets [start_index] == 1 + this.Offsets [start_index - 1]) {
				if (this.Offsets.Count > start_index && this.IsFilled(start_index)) {
					ctx.run_len = this.B - left;
					return acc + left;
				}
				ctx.Seek (this.Offsets [start_index - 1]);				
			}
//			for (int i = 0; i < left; i++) {
//				int read = this.ReadNext (ctx);
//				acc += read;
//			}
			while (left > 0) {
				long read;
				if (ctx.run_len > 0) {
					read = ctx.run_len;
					ctx.run_len = 0;
					left -= (int)read;
					if (left < 0) {
						read += left;
						ctx.run_len += left;
						left = 0;
					}
				} else {
					read = this.ReadNext (ctx);
					left--;
				}
				acc += read;
			}
			return acc;
		}
		
		long SeqAccessRank1 (long curr_pos, long pos, long max, out long found_pos, BitStreamCtxRL ctx)
		{
			long i = 0;
			long u = -1;
			while (i < max && curr_pos < pos) {
				if (ctx.run_len > 0) {
					u = ctx.run_len;
					ctx.run_len = 0;
					if (curr_pos + u > pos) {
						u = pos - curr_pos;
					}
					curr_pos += u;
					i += u;
				} else {
					u = this.ReadNext (ctx);
					if (curr_pos + u > pos) {
						break;
					}
					curr_pos += u;
					i++;
				}
			}
			found_pos = curr_pos;
			return i;
		}
		
		public override long Rank1 (long pos)
		{
			long select_pos;
			long rank = this.BackendAccessRank1 (pos, out select_pos, new BitStreamCtxRL ());
			return rank;
		}
		
		public override bool Access(long pos)
		{
			long found_pos;
			this.BackendAccessRank1 (pos, out found_pos, new BitStreamCtxRL ());
			return pos == found_pos;
		}
		
		long BackendAccessRank1 (long pos, out long found_pos, BitStreamCtxRL ctx)
		{
			if (pos < 0) {
				found_pos = -1;
				return 0;
			}
			int start_index = -1;
			if (this.Samples.Count > 0) {
				start_index = GenericSearch.FindFirst<long> (pos, this.Samples);
			}
			// reset run_len
			ctx.run_len = 0;
			// int count;
			if (start_index < 0) {
				//if (this.Offsets.Count > 0 && this.Offsets[0] == 1) {
				if (this.Offsets.Count > 0 && this.IsFilled (0)) {
					found_pos = pos;
					return pos + 1;
				} else {
					ctx.Seek (0);
					int count = Math.Min (this.B, this.M);
					return this.SeqAccessRank1 (AccStart, pos, count, out found_pos, ctx);
				}
			}
			int rel_rank = (start_index + 1) * this.B;
			//if (this.Offsets.Count > start_index + 1 && this.Offsets[start_index + 1] == 1 + this.Offsets[start_index]) {
			if (this.Offsets.Count > start_index + 1 && this.IsFilled(start_index + 1) ) {
				found_pos = pos;
				long diff_rank = pos - this.Samples[start_index];
				return rel_rank + diff_rank;
			} else {
				ctx.Seek (this.Offsets[start_index]);
				int count = Math.Min (this.B, this.M - rel_rank);
				return rel_rank + this.SeqAccessRank1 (this.Samples[start_index], pos, count, out found_pos, ctx);
			}
		}
				
		void Commit (BitStreamCtxRL ctx)
		{
			//Console.WriteLine ("commit run_len: {0}, B: {1}", ctx.run_len, this.B);
			if (ctx.run_len > 0) {
				if (ctx.run_len == this.B) {
					Coder.Encode (this.Stream, 1);
				} else {
					Coder.Encode (this.Stream, 1);
					Coder.Encode (this.Stream, ctx.run_len);
				}
				ctx.run_len = 0;
			}
		}
		
		public void Build (IList<long> orderedList, short b, IIEncoder64 coder = null)
		{
			long n = 0;
			if (orderedList.Count > 0) {
				n = orderedList[orderedList.Count - 1] + 1;
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
			var ctx = new BitStreamCtxRL ();
	
			foreach (var current in orderedList) {
				if (current == 0) {
					prev = AccStart;
				}
				this.M++;
				long diff = current - prev;
				//Console.WriteLine ("DIFF {0}, num: {1}, current: {2}", diff, this.M, current);
				if (diff == 1) {
					++ctx.run_len;
				} else {
					this.Commit (ctx);
					// Console.WriteLine ("%%%%%% diff: {0}, prev: {1}, curr: {2}", diff, prev, current);
					Coder.Encode (this.Stream, diff);
				}
				if ((this.M % this.B) == 0) {	
					this.Commit (ctx);
					this.Samples.Add (current);
					this.Offsets.Add (this.Stream.CountBits);
				}
				if (current >= this.N) {
					this.N = current + 1;
				}
				prev = current;
			}
			this.Commit (ctx);
			/*for (int i = 0; i < this.Samples.Count; i++) {
				Console.WriteLine ("-- i: {0}, samples: {1}, offset: {2}", i, Samples[i], Offsets[i]);
			}*/
		}	
	}
}
