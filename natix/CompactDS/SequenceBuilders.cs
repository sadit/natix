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
//   Original filename: natix/CompactDS/SequenceBuilders.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public delegate IRankSelectSeq SequenceBuilder(IList<int> seq, int sigma);
	
	public class SequenceBuilders
	{
		public SequenceBuilders ()
		{
		}

		public static SequenceBuilder GetGolynskiSinglePerm (short t)
		{
			return delegate (IList<int> seq, int sigma) {
				var S = new GolynskiSinglePermSeq ();
				S.PermBuilder = PermutationBuilders.GetSuccRL2CyclicPerms (t);
				S.Build (seq, sigma);
				return S;
			};
		}
		
		public static SequenceBuilder GetGolynskiSinglePerm (PermutationBuilder pbuilder)
		{
			return delegate (IList<int> seq, int sigma) {
				var S = new GolynskiSinglePermSeq ();
				S.PermBuilder = pbuilder;
				S.Build (seq, sigma);
				return S;
			};
		}
				
		public static SequenceBuilder GetGolynskiListRL2 (short t = 16, short block_size = 127, IIEncoder32 coder = null)
		{
			return delegate (IList<int> seq, int sigma) {
				var S = new GolynskiListRL2Seq ();
				//S.PermCodingBuildParams = new SuccRL2CyclicPerms_MRRR.BuildParams (coder, block_size);
				if (coder == null) {
					coder = new EliasDelta ();
				}
				S.Build (seq, sigma, t, coder, block_size);
				return S;
			};
		}

		public static SequenceBuilder GetGolynskiSucc (short t = 16)
		{
			return delegate (IList<int> seq, int sigma) {
				var S = new GolynskiMunroRaoSeq ();
				S.PermBuilder = PermutationBuilders.GetSuccCyclicPerms (t);
				S.Build (seq, sigma);
				return S;
			};
		}
		
		public static SequenceBuilder GetGolynski (short t = 16)
		{
			return delegate (IList<int> seq, int sigma) {
				var S = new GolynskiMunroRaoSeq ();
				S.PermBuilder = PermutationBuilders.GetCyclicPerms (t);
				S.Build (seq, sigma);
				return S;
			};
		}
		
		public static SequenceBuilder GetGolynskiRL (short t = 16)
		{
			return delegate (IList<int> seq, int sigma) {
				var S = new GolynskiMunroRaoSeq ();
				S.PermBuilder = PermutationBuilders.GetSuccRLCyclicPerms (t);
				S.Build (seq, sigma);
				return S;
			};
		}

		public static SequenceBuilder GetWT_GGMN_BinaryCoding (short b)
		{
			return delegate (IList<int> seq, int sigma) {
				var wt = new WaveletTree ();
				wt.BitmapBuilder = BitmapBuilders.GetGGMN_wt (b);
				int numbits = (int)Math.Ceiling (Math.Log (sigma, 2));
				var enc = new BinaryCoding (numbits);
				wt.Build (enc, sigma, seq);
				return wt;

			};
		}
		
		public static SequenceBuilder
			GetWT_BinaryCoding(BitmapFromBitStream bitmap_builder)
		{
			return delegate (IList<int> seq, int sigma) {
				var wt = new WaveletTree ();
				wt.BitmapBuilder = bitmap_builder;
				int numbits = (int)Math.Ceiling (Math.Log (sigma, 2));
				var enc = new BinaryCoding (numbits);
				wt.Build (enc, sigma, seq);
				return wt;
			};
		}
		
		public static SequenceBuilder GetWT (
			BitmapFromBitStream bitmap_builder,
			Func<int, IIEncoder32> get_coder
		)
		{
			return delegate (IList<int> seq, int sigma) {
				var wt = new WaveletTree ();
				wt.BitmapBuilder = bitmap_builder;
				var enc = get_coder (sigma);
				// var enc = new BinaryCoding (numbits);
				wt.Build (enc, sigma, seq);
				return wt;

			};
		}
		
		public static SequenceBuilder GetIISketches_SArray (int sketch_blocksize)
		{
			return delegate (IList<int> seq, int sigma) {
				var iis = new InvIndexSketches ();
				iis.BitmapBuilder = BitmapBuilders.GetSArray ();
				iis.Build (seq, sigma, sketch_blocksize);
				return iis;
			};
		}
		
		public static SequenceBuilder GetIISketches (
			BitmapFromList bitmap_builder, int sketch_blocksize
			)
		{
			return delegate (IList<int> seq, int sigma) {
				var iis = new InvIndexSketches ();
				iis.BitmapBuilder = bitmap_builder;
				iis.Build (seq, sigma, sketch_blocksize);
				return iis;
			};
		}
		
		public static SequenceBuilder GetIISeq_SArray ()
		{
			return delegate (IList<int> seq, int sigma) {
				var iis = new InvIndexSeq ();
				iis.BitmapBuilder = BitmapBuilders.GetSArray ();
				iis.Build (seq, sigma);
				return iis;
			};
		}
		
		public static SequenceBuilder GetIISeq (
			BitmapFromList bitmap_builder
			)
		{
			return delegate (IList<int> seq, int sigma) {
				var iis = new InvIndexSeq ();
				iis.BitmapBuilder = bitmap_builder;
				iis.Build (seq, sigma);
				return iis;
			};
		}
		
		public static SequenceBuilder GetInvIndexXLBSeq (short t = 16, BitmapFromList row_builder = null, BitmapFromBitStream len_builder = null)
		{
			if (row_builder == null) {
				row_builder = BitmapBuilders.GetSArray ();
			}
			if (len_builder == null) {
				len_builder = BitmapBuilders.GetGGMN_wt (12);
			}
			return delegate (IList<int> seq, int sigma) {
				var iis = new InvIndexXLBSeq();
				iis.Build (seq, sigma, t, row_builder, len_builder);
				return iis;
			};
		}
		
		public static SequenceBuilder GetSeqXLB_SArray64 (short t = 16, BitmapFromBitStream H_builder = null)
		{
			return GetSeqXLB (t, BitmapBuilders.GetSArray64 (H_builder));
		}
		
		public static SequenceBuilder GetSeqXLB_DiffSet64 (short t = 16, short b = 127, IIEncoder64 coder = null)
		{
			if (coder == null) {
				coder = new EliasDelta64 ();
			}
			return GetSeqXLB (t, BitmapBuilders.GetDiffSet64 (b, coder));
		}
		
		public static SequenceBuilder GetSeqXLB_DiffSetRL64 (short t = 16, short b = 127, IIEncoder64 coder = null)
		{
			if (coder == null) {
				coder = new EliasDelta64 ();
			}
			return GetSeqXLB (t, BitmapBuilders.GetDiffSetRL64 (b, coder));
		}
		
		public static SequenceBuilder GetSeqXLB_DiffSetRL2_64 (short t = 16, short b = 127, IIEncoder64 coder = null)
		{
			if (coder == null) {
				coder = new EliasDelta64();
			}
			return GetSeqXLB (t, BitmapBuilders.GetDiffSetRL2_64 (b, coder));
		}
		
		public static SequenceBuilder GetSeqXLB (short t, BitmapFromList64 bitmap_builder)
		{
			return delegate (IList<int> seq, int sigma) {
				var seqxl = new SeqXLB ();
				seqxl.Build (seq, sigma, t, bitmap_builder);
				return seqxl;
			};
		}
	}
}
