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
//   Original filename: natix/CompactDS/Bitmaps/RRRv2.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	public class RRRv2 : RRR
	{
	    ListSDiffCoder LI;
		
		public RRRv2 () : base()
		{
		}
		
		
		override protected void InitClasses ()
		{
			//this.LI = new ListSDiffCoder( new DoublingSearchCoding(), this.BlockSize);
			this.LI = new ListSDiffCoder( new ZeroCoding(new EliasGamma32(), 1), this.BlockSize);
			this.Klasses = this.LI;
		}
		
		override protected void SaveClasses (BinaryWriter Output)
		{
			this.LI.Save (Output);
			// (this.Klasses as ListShortIntegersCoder).Save (Output);
		}
		
		override protected void LoadClasses (BinaryReader Input)
		{
			
			var L = new ListSDiffCoder();
			L.Load(Input);
			this.Klasses = L;
			this.LI = L;
		}
		
		override protected void EncodeClass (int klass)
		{
			if (klass < 8) {
				// encoding from left to right, as even numbers
				// this.Klasses.Add (klass << 1);
				this.LI.Add (klass << 1);
			} else {
				// encoding from right to left as odd numbers
				// this.Klasses.Add(((15 - klass) << 1)+1);
				this.LI.Add(((15 - klass) << 1)+1);
			}
		}
				
		override protected int DecodeClass (int i, CtxCache ctx)
		{
			int klass;
			// Console.WriteLine("XXXXX i: {0}, offset: {1}", i, ctx.Offset);
			if (ctx.Offset == -1 || ctx.prev_item + 1 != i) {
				klass = this.LI.GetItem(i, ctx);
			} else {
				klass = this.LI.GetNext(ctx);
			}
			ctx.prev_item = i;
			// var klass = this.Klasses[i];
			// var klass = this.LI[i];
			if ((0x1 & klass) == 0x1) { // check for odd
				klass >>= 1; // removing encding as odd
				klass = 15 - klass; // going right to left
			}
			else {
				klass >>= 1; // removing 
			}
			return klass;
		}
	}
}
