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
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;

namespace  natix.SimilaritySearch
{
	public class QGramH1 : ListGenerator<byte>
	{
		public BitStream32 stream;
		public long start;
		public int len;

		public QGramH1 (BitStream32 _stream, long _start, int _len)
		{
			this.stream = _stream;
			this.start = _start;
			this.len = _len;
		}

		public override byte GetItem (int index)
		{
			var ctx = new BitStreamCtx(this.start + index * 8);
			return (byte)this.stream.Read(8, ctx);
		}

		public override void SetItem (int index, byte u)
		{
			throw new System.NotImplementedException ();
		}

		public override int Count {
			get {
				return this.len >> 3;
			}
		}

        public byte[] ToCharArray()
        {
            var A = new byte[this.Count];
            for (int i = 0; i < A.Length; ++i) {
                A[i] = this[i];
            }
            return A;
        }
	}
}

