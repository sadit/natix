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
using System;
using System.IO;

namespace natix.CompactDS
{
	public class NullBitmap : Bitmap
	{
		
		public NullBitmap () : base()
		{
		}
		
		public override bool Access(int i)
		{
			throw new NotSupportedException();
		}

		public override int Select1 (int i)
		{
			throw new NotSupportedException ();
		}

		public override int Rank1 (int i)
		{
			throw new NotSupportedException ();
		}

		public override int Count {
			get {
                throw new NotSupportedException ();
			}
		}
		
		public override int Count1 {
			get {
				throw new NotSupportedException ();
			}
		}
		
		public override void AssertEquality (Bitmap other)
		{
			throw new NotSupportedException ();
		}
		
		public override void Save (BinaryWriter bw)
		{
		}
		
		public override void Load (BinaryReader br)
		{
		}		
	}
}

