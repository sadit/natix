//
//  Copyright 2012  Eric Sadit Tellez Avila
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
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.CompactDS
{
	public class FakeSeq : IRankSelectSeq
	{
		public IList<int> SEQ;
		public int sigma; // upper bound
		public int RealSigma; // real value

		public FakeSeq ()
		{
			this.SEQ = new List<int>();
		}

		public FakeSeq (IList<int> seq, int sigma) : this()
		{
			this.Build(seq, sigma);
		}

		public FakeSeq (int sigma) : this()
		{
			var numbits = ListIFS.GetNumBits(sigma-1);
			this.Build(new ListIFS(numbits), sigma);
		}

		public void Build (IList<int> seq, int sigma)
		{
			this.SEQ = seq;
			this.sigma = sigma;
		}

		public void Load(BinaryReader Input)
		{
			throw new NotSupportedException();
		}

		public void Save(BinaryWriter Output)
		{
			throw new NotSupportedException();
		}

		public void Add (int item)
		{
			if (this.RealSigma < item + 1) {
				this.RealSigma = item + 1;
			}
			this.SEQ.Add(item);
		}

		public int this [int i] {
			get {
				return this.SEQ[i];
			}
			set {
				this.SEQ[i] = value;
			}
		}

		public int Sigma {
			get {
				return sigma;
			}
		}

		public int Count {
			get {
				return this.SEQ.Count;
			}
		}

		public IRankSelect Unravel(int sym)
		{
			throw new NotSupportedException();
		}

		public int Access(int pos)
		{
			return this.SEQ[pos];
		}

		public int Rank (int symbol, int pos)
		{
			throw new NotSupportedException();
		}

		public int Select (int symbol, int rank)
		{
			throw new NotSupportedException();
		}
	}
}

