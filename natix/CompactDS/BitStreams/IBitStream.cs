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
//   Original filename: natix/CompactDS/BitStreams/IBitStream.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public interface IBitStream
	{
		void AssertEquality (object obj);
		//long CurrentOffset {
		//	get;
		//}
		
		long CountBits {
			get;
		}
		
		/*int Count64 {
			get;
		}*/
		
		bool this[long i] {
			get;
			set;
		}
		
		IList<UInt64> GetIList64 ();
		IList<UInt32> GetIList32 ();
		
		//*** IO LIKE METHODS ***
		void Write (bool x);
		void Write (bool x, int times);
		void Write (Int32 x, int numbits);
		void Write (UInt32 x, int numbits);
		void WriteAt (UInt32 x, int numbits, long pos);
		void Write (Int64 x, int numbits);
		void Write (UInt64 x, int numbits);
		bool Read (BitStreamCtx ctx);
		// UInt32 Read32 (BitStreamCtx ctx);
		UInt64 Read (int numbits, BitStreamCtx ctx);
		// void Seek (long offset);
		int ReadZeros (BitStreamCtx ctx);
		int ReadOnes (BitStreamCtx ctx);
		void Save (BinaryWriter w);
		void Load (BinaryReader r);
	}
}
