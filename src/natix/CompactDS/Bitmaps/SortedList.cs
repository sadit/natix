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
//   Original filename: natix/CompactDS/Bitmaps/PlainSortedList.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class SortedList : Bitmap
	{
		protected int N;
		protected List<int> sortedList;
		
		public override void AssertEquality (Bitmap _other)
		{
			var other = (SortedList)_other;
			if (this.N != other.N) {
				throw new ArgumentException ("Parameter PlainSortedList.N have differences");
			}
			Assertions.AssertIList<int> (this.sortedList, other.sortedList, "PlainSortedList.sortedList");
		}

		public override void Save (BinaryWriter bw)
		{
			bw.Write ((int)this.N);
			bw.Write ((int)this.sortedList.Count);
			PrimitiveIO<int>.SaveVector (bw, this.sortedList);
		}
		
		public override void Load (BinaryReader br)
		{
			this.N = br.ReadInt32 ();
			int len = br.ReadInt32 ();
			this.sortedList = new List<int>(len);
			PrimitiveIO<int>.LoadVector (br, len, this.sortedList);
		}
		
		public SortedList () : base()
		{		
		}
	
		public void Add(int p)
		{
			if (this.sortedList.Count == 0) {
				this.sortedList.Add (p);
				return;
			}
			if (this.sortedList[this.sortedList.Count-1] >= p) {
				throw new ArgumentOutOfRangeException ();
			}
			this.sortedList.Add (p);
			if (p+1 > this.N) {
				this.N = p + 1;
			}
		}

		public virtual void Build (List<int> sortedList, int n = 0)
		{
			if (n <= 0 && sortedList.Count > 0) {
				n = 1 + sortedList [sortedList.Count - 1];
			}
			this.sortedList = sortedList;
			this.N = n;
		}

		public override int Select1 (int rank)
		{
			if (rank < 1) {
				return -1;
			}
			return this.sortedList[rank - 1];
		}
		
		public override int Rank1 (int pos)
		{
			if (pos < 0 || this.sortedList.Count < 1) {
				return 0;
			}
			return 1 + Search.FindLast<int> (pos, this.sortedList);
		}
		
		public override bool Access (int i)
		{
			var rank = this.Rank1 (i);
			return i == this.Select1 (rank);
		}

		public override int Count {
			get {
				return this.N;
			}
		}
		
		public override int Count1 {
			get {
				return this.sortedList.Count;
			}
		}
	}
}
