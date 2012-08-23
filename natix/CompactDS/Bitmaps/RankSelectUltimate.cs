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
//   Original filename: natix/CompactDS/Bitmaps/RankSelectUltimate.cs
// 
//using System;
//namespace natix.CompactDS
//{
//	public class RankSelectUltimate
//	{
//		int BlockSize;
//		IIntegerEncoder Coder;
//		BitStream32 Bitmap;
//		
//		public RankSelectUltimate (int blockSize, IIntegerEncoder coder, BitStream32 bitmap)
//		{
//			this.BlockSize = blockSize;
//			this.Coder = coder;
//			this.Bitmap = bitmap;
//			this.IndexBitmap ();
//		}
//		
//		void IndexBitmap ()
//		{
//			/*
//			int[] partialranks = new ushort[ this.Bitmap.CountBits / this.BlockSize ];
//			int acc = 0;
//			for (int i = 0, pos = 0; i < partialranks.Length; pos += this.BlockSize) {
//				acc += BitAccess.Rank1(this.Bitmap.Buffer, pos, this.BlockSize, -1);
//				partialranks[i] = acc;
//			}
//			int galloping = 1;
//			BitStream32 aux = new BitStream32();
//			for (int acc = 0, i = 0; i < partialranks.Length; i++) {
//				acc += partialranks[i];
//				if (i == galloping) {
//					this.Coder.Encode(this.Bitmap, acc);
//					acc = 0;
//					galloping = ((galloping+1) << 1) - 1;
//				}
//			}*/
//		}
//		
//	}
//}
