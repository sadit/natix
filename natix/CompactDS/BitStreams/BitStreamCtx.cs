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
//   Original filename: natix/CompactDS/BitStreams/BitStreamCtx.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public class BitStreamCtx
	{
		public long Offset = 0;
//		public long Offset {
//			get {
//				return this._offset;
//			}
//			set {
//				if (value < 0) {
//					throw new ArgumentOutOfRangeException (String.Format("XXXX Offset cannot be negative {0} XXXX", value));
//				}
//				this._offset = value;
//			}
//		}
		public BitStreamCtx (long offset)
		{
			this.Offset = offset;
		}
		
		public BitStreamCtx ()
		{
			this.Offset = 0;
		}
		
		public void Seek (long offset)
		{
			this.Offset = offset;
		}
	}
}