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
//   Original filename: natix/CompactDS/Bitmaps/FakeBitmap.cs
// 
using System;
using System.IO;

namespace natix.CompactDS
{
	public class FakeBitmap : RankSelectBase
	{
		public BitStream32 B;
		
		public FakeBitmap ()
		{
			this.B = new BitStream32 ();
		}
		
		public FakeBitmap (BitStream32 B)
		{
			this.B = B;
		}
		
		public void Write (bool b)
		{
			this.B.Write (b);
		}
		
		public bool this[long i] {
			get {
				return this.B[i];
			}
		}
		
		public override int Rank1 (int i)
		{
			throw new NotSupportedException ();
		}

		public override int Count {
			get {
				return (int) this.B.CountBits;
			}
		}
		
		public override int Count1 {
			get {
				throw new NotSupportedException ();
			}
		}
		
		public override void AssertEquality (IRankSelect other)
		{
			throw new NotSupportedException ();
		}
		
		public override void Save (BinaryWriter bw)
		{
			throw new NotSupportedException ();
		}
		
		public override void Load (BinaryReader br)
		{
			throw new NotSupportedException ();
		}
		
		public GGMN GetGGMN (short step)
		{
			var g = new GGMN ();
			g.Build (this.B, step);
			return g;
		}
	}
}

