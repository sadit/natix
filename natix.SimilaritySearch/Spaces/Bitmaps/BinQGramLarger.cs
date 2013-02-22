//
//  Copyright 2012  Eric Sadit Tellez Avila donsadit@gmail.com
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Collections.Generic;

namespace  natix.SimilaritySearch
{
	public class BinQGramLarger : ListGenerator<byte>
	{
		int StartIndex;
		int Len;
		IList<byte> Data;
        byte FillChar;

		public BinQGramLarger (IList<byte> Data, int startIndex, int Len, byte fillchar)
		{
			this.StartIndex = startIndex;
			this.Data = Data;
			this.Len = Len;
            this.FillChar = fillchar;
		}
		
		public override int Count {
			get {
				return this.Len;
			}
		}
		
		public override byte GetItem (int index)
        {
            var i = this.StartIndex + index;
            if (i < this.Data.Count) {
                return this.Data [i];
            } else {
                return this.FillChar;
            }
		}
		
		public override void SetItem (int index, byte u)
		{
			throw new NotSupportedException ();
		}
	}
}