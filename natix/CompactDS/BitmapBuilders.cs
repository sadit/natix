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
//   Original filename: natix/CompactDS/BitmapBuilders.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public delegate IRankSelect64 BitmapFromList64(IList<long> L, long n);
	public delegate IRankSelect BitmapFromList(IList<int> L);
	public delegate IRankSelect BitmapFromBitStream(FakeBitmap stream);
	
	public class BitmapBuilders
	{
		public BitmapBuilders ()
		{
		}
		
		public static BitmapFromList GetGGMN (short sample_step)
		{
			return delegate (IList<int> L) {
				var rs = new GGMN ();
				rs.Build (L, sample_step);
				return rs;
			};
		}
		
		public static BitmapFromList GetRRR (short block_size)
		{
			return delegate (IList<int> L) {
				var rs = new RRR ();
				rs.Build (L, block_size);
				return rs;
			};
		}
		
		public static BitmapFromList GetRRRv2 (short block_size)
		{
			return delegate (IList<int> L) {
				var rs = new RRRv2 ();
				rs.Build (L, block_size);
				return rs;
			};
		}
		
		public static BitmapFromList GetSArray (BitmapFromBitStream H_builder = null)
		{
			return delegate (IList<int> L) {
				var rs = new SArray ();
				rs.Build (L, 0, H_builder);
				return rs;
			};
		}
		
		public static BitmapFromList GetPlainSortedList ()
		{
			return delegate (IList<int> L) {
				var rs = new PlainSortedList ();
				rs.Build (L);
				return rs;
			};
		}
		
		public static BitmapFromList GetDiffSetRL2 (short sample_step, IIEncoder32 coder = null)
		{
			return delegate (IList<int> L) {
				var rs = new DiffSetRL2 ();
				rs.Build (L, sample_step, coder);
				return rs;
			};
		}
		
		public static BitmapFromList GetDiffSet (short sample_step, IIEncoder32 coder = null)
		{
			return delegate (IList<int> L) {
				var rs = new DiffSet ();
				rs.Build (L, sample_step, coder);
				return rs;
			};
		}
		
		public static BitmapFromList GetDArray (short b_rank, short b_select)
		{
			return delegate (IList<int> L) {
				var rs = new DArray ();
				rs.Build (L, b_rank, b_select);
				return rs;
			};
		}
		
		public static IList<int> CreateSortedList (FakeBitmap bitmap)
		{
			return new SortedListRS (bitmap.GetGGMN (12));
		}
		
		
		public static BitmapFromBitStream GetGGMN_wt (short sample_step)
		{
			return delegate (FakeBitmap b) {
				return b.GetGGMN (12);
			};
		}

		public static BitmapFromBitStream GetDArray_wt (short b_rank, short s_rank)
		{
			return delegate (FakeBitmap b) {
				var rs = new DArray ();
				rs.Build (b.B, b_rank, s_rank);
				return rs;
			};
		}

		public static BitmapFromBitStream GetRRR_wt (short sample_step)
		{
			return delegate (FakeBitmap b) {
				var rs = new RRR ();
				rs.Build (b.B, sample_step);
				return rs;
			};
		}

		public static BitmapFromBitStream GetRRRv2_wt (short sample_step)
		{
			return delegate (FakeBitmap b) {
				var rs = new RRRv2 ();
				rs.Build (b.B, sample_step);
				return rs;
			};
		}
		
		public static BitmapFromBitStream GetSArray_wt (BitmapFromBitStream H_builder = null)
		{
			return delegate (FakeBitmap b) {
				var rs = new SArray ();
				rs.Build (CreateSortedList (b), b.Count, H_builder);
				return rs;
			};
		}
		
		public static BitmapFromBitStream GetPlainSortedList_wt ()
		{
			return delegate (FakeBitmap b) {
				var rs = new PlainSortedList ();
				rs.Build (CreateSortedList (b), b.Count);
				return rs;
			};
		}
		public static BitmapFromBitStream GetDiffSetRL2_wt (short sample_step, IIEncoder32 coder = null)
		{
			return delegate (FakeBitmap b) {
				var rs = new DiffSetRL2 ();
				rs.Build (CreateSortedList (b), b.Count, sample_step, coder);
				return rs;
			};
		}
		
		public static BitmapFromBitStream GetDiffSet_wt (short sample_step, IIEncoder32 coder = null)
		{
			return delegate (FakeBitmap b) {
				var rs = new DiffSet ();
				rs.Build (CreateSortedList (b), b.Count, sample_step, coder);
				return rs;
			};
		}
		
		// 64 bit bitmaps
		public static BitmapFromList64 GetSArray64 (BitmapFromBitStream H_builder = null)
		{
			return delegate (IList<long> L, long n) {
				var rs = new SArray64 ();
				rs.Build (L, n, H_builder);
				return rs;
			};
		}
		
		public static BitmapFromList64 GetDiffSet64 (short b, IIEncoder64 coder = null)
		{
			return delegate (IList<long> L, long n) {
				var rs = new DiffSet64 ();
				rs.Build (L, n, b, coder);
				return rs;
			};
		}

		public static BitmapFromList64 GetDiffSetRL64 (short b, IIEncoder64 coder = null)
		{
			return delegate (IList<long> L, long n) {
				var rs = new DiffSetRL64 ();
				rs.Build (L, n, b, coder);
				return rs;
			};
		}

		public static BitmapFromList64 GetDiffSetRL2_64 (short b, IIEncoder64 coder = null)
		{
			return delegate (IList<long> L, long n) {
				var rs = new DiffSetRL2_64 ();
				rs.Build (L, n, b, coder);
				return rs;
			};
		}
	}
}

