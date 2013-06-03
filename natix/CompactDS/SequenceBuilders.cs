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
	public delegate Sequence SequenceBuilder(IList<int> seq, int sigma);
	
	public class SequenceBuilders
	{
		public SequenceBuilders ()
		{
		}

		public static SequenceBuilder GetSeqSinglePermListIDiffs (short t, short bsize = 16, BitmapFromBitStream bitmap_builder = null, IIEncoder32 encoder = null)
		{
			var pbuilder = PermutationBuilders.GetCyclicPermsListIDiffs(t, bsize, bitmap_builder, encoder);
			return GetSeqSinglePerm(pbuilder, null);
		}

        public static SequenceBuilder GetSeqSinglePermIFS (short t, BitmapFromBitStream bitmap_builder = null)
        {
            var pbuilder = PermutationBuilders.GetCyclicPermsListIFS(t);
            return GetSeqSinglePerm(pbuilder, bitmap_builder);
        }

		public static SequenceBuilder GetSeqSinglePerm (PermutationBuilder perm_builder = null, BitmapFromBitStream bitmap_builder = null)
		{
			if (perm_builder == null) {
				perm_builder = PermutationBuilders.GetCyclicPermsListIDiffs (16, 63);
			}
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetGGMN_wt(8);
			}
			return delegate (IList<int> seq, int sigma) {
				var S = new SeqSinglePerm ();
				S.Build (seq, sigma,perm_builder, bitmap_builder);
				return S;
			};
		}

		public static SequenceBuilder GetGolynski (PermutationBuilder perm_builder = null,
		                                           BitmapFromBitStream bitmap_builder = null)
		{
			if (perm_builder == null) {
				perm_builder = PermutationBuilders.GetCyclicPermsListIFS(16);
			}
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetGGMN_wt(16);
			}
			return delegate (IList<int> seq, int sigma) {
				var S = new GolynskiMunroRaoSeq ();
				S.Build (seq, sigma, perm_builder, bitmap_builder);
				return S;
			};
		}
		
		public static SequenceBuilder GetGolynskiRL (short t = 16)
		{
			return GetGolynski(PermutationBuilders.GetCyclicPermsListRL(t));
		}

		public static SequenceBuilder GetGolynskiSucc (short t = 16, BitmapFromBitStream bitmap_builder = null)
		{
			return GetGolynski(PermutationBuilders.GetCyclicPermsListIFS(t), bitmap_builder);
		}

		public static SequenceBuilder GetWT_GGMN_BinaryCoding (short b)
		{
			return GetWT_BinaryCoding(BitmapBuilders.GetGGMN_wt(b));
		}
		
		public static SequenceBuilder
			GetWT_BinaryCoding(BitmapFromBitStream bitmap_builder = null)
		{
			return GetWT(bitmap_builder);
		}

		public static SequenceBuilder GetWT (
			BitmapFromBitStream bitmap_builder = null,
			Func<int, IIEncoder32> get_coder = null
		)
		{
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetGGMN_wt(16);
			}
			return delegate (IList<int> seq, int sigma) {
				var wt = new WaveletTree ();
				wt.BitmapBuilder = bitmap_builder;
				// var enc = new BinaryCoding (numbits);
				IIEncoder32 enc;
				if (get_coder == null) {
					int numbits = (int)Math.Ceiling (Math.Log (sigma, 2));
					enc = new BinaryCoding (numbits);
				} else {
					enc = get_coder(sigma);
				}
				wt.Build (seq, sigma, enc);
				return wt;
			};
		}


		public static SequenceBuilder GetWTM_TopKFreqCoder (byte bits_per_code, int K, SequenceBuilder seq_builder = null)
		{
			return delegate (IList<int> seq, int sigma) {
				var wt = new WTM();
				TopKFreqCoder symcoder;
				var __coder = new EqualSizeCoder(bits_per_code, sigma - 1);
				var freqs = new int[sigma];
				for (int i = 0; i < seq.Count; ++i) {
					freqs[seq[i]]++;
				}
				symcoder = new TopKFreqCoder(K, freqs, __coder);
				wt.Build(seq, sigma, symcoder, seq_builder);
				return wt;
			};
		}
		public static SequenceBuilder GetWTM (byte bits_per_code, SequenceBuilder seq_builder = null)
		{
			return delegate (IList<int> seq, int sigma) {
				var wt = new WTM();
				var symcoder = new EqualSizeCoder(bits_per_code, sigma - 1);
				wt.Build(seq, sigma, symcoder, seq_builder);
				return wt;
			};
		}

		public static SequenceBuilder GetWTM ( ISymbolCoder symcoder = null, SequenceBuilder seq_builder = null)
		{
			return delegate (IList<int> seq, int sigma) {
				var wt = new WTM();
				wt.Build(seq, sigma, symcoder, seq_builder);
				return wt;
			};
		}

		public static SequenceBuilder GetIISketches (int sketch_blocksize,
			BitmapFromList bitmap_builder = null)
		{
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetSArray();
			}
			return delegate (IList<int> seq, int sigma) {
				var iis = new InvIndexSketches ();
				iis.BitmapBuilder = bitmap_builder;
				iis.Build (seq, sigma, sketch_blocksize);
				return iis;
			};
		}
		
		public static SequenceBuilder GetIISeq (
			BitmapFromList bitmap_builder = null
			)
		{
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetSArray();
			}
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

		public static SequenceBuilder GetSeqPlain(short B = 0, ListIBuilder list_builder = null, BitmapFromBitStream bitmap_builder = null, bool CopyOnUnravel = false)
		{
			return delegate (IList<int> seq, int sigma) {
                if (CopyOnUnravel) {
                    var s = new SeqPlainCopyOnUnravel();
                    s.Build(seq, sigma, B, list_builder, bitmap_builder);
                    return s;
                } else {
                    var s = new SeqPlain();
                    s.Build(seq, sigma, B, list_builder, bitmap_builder);
                    return s;
                }
			};
		}

		public static SequenceBuilder GetSeqPlainRL(short B = 0, BitmapFromBitStream bitmap_builder = null)
		{
            return GetSeqPlain(B, ListIBuilders.GetListEqRL(), bitmap_builder);
		}
	}
}
