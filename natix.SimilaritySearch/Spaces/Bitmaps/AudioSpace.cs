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
//   Original filename: natix/SimilaritySearch/Spaces/AudioSpace.cs
// 
using System;
using System.IO;
using System.Collections.Generic;
using natix.CompactDS;
using natix.SortingSearching;

namespace  natix.SimilaritySearch
{
	public class AudioSpace : MetricDB
	{
		public IRankSelect LENS;
		public int SymbolSize;
		public int Q;
		IList< byte > Data;
		int numdist = 0;

		public string Name {
			get; set;
		}

		public void Save(BinaryWriter Output)
		{
			RankSelectGenericIO.Save (Output, this.LENS);
			Output.Write ((int) this.SymbolSize);
			Output.Write ((int) this.Q);
			Output.Write ((int) this.Data.Count);
			PrimitiveIO<byte>.WriteVector(Output, this.Data);
			Output.Write (this.Name);
		}

		public void Load (BinaryReader Input)
		{
			this.LENS = RankSelectGenericIO.Load(Input);
			this.SymbolSize = Input.ReadInt32();
			this.Q = Input.ReadInt32();
			var len = Input.ReadInt32 ();
			this.Data = new byte[ len ];
			PrimitiveIO<byte>.ReadFromFile(Input, len, this.Data);
			this.Name = Input.ReadString();
		}
				
		public object Parse (string name, bool isquery)
		{
			if (name.StartsWith ("obj")) {
				var A = name.Split (' ');
				var id = A [1];
				var u = (IList<byte>)this [int.Parse (id)];
				if (A.Length == 2) {
					return u;
				}
				int num_bits = u.Count * 8;
				var num_flips = float.Parse (A [2]) * num_bits;
				var L = new byte[u.Count];
				for (int i = 0; i < u.Count; ++i) {
					L [i] = u [i];
				}
				var rand = new Random ();
				for (int i = 0; i < num_flips; ++i) {
					var pos = rand.Next (0, num_bits);
					if (BitAccess.GetBit (L, pos)) {
						BitAccess.ResetBit (L, pos);
					} else {
						BitAccess.SetBit (L, pos);
					}
				}
				return L;
			}
			var res = BinQ8HammingSpace.LoadObjectFromFile (name, !isquery);
			return new BinQGram (res, 0, this.Q);
		}

		public IResult CreateResult (int K, bool ceiling)
		{
			return new Result (K, ceiling);
		}
	
		public int NumberDistances {
			get {
				return this.numdist;
			}
		}

		public double Dist (object a, object b)
		{
			this.numdist++;
			/* if (this.numdist % 2048 == 0) {
				Console.WriteLine ("numdist: {0}, a.Count: {1}, b.Count: {2}", this.numdist, a.Count, b.Count);
			}
			*/
			return BinQ8HammingSpace.DistMinHamming ((IList<byte>)a, (IList<byte>)b, this.SymbolSize);
		}

		public AudioSpace ()
		{
			this.SymbolSize = -1;
			this.Q = -1;
		}

		public void Build (string listname, int qsize, int symsize)
		{
			this.Q = qsize;
			this.SymbolSize = symsize;
			this.Name = listname;
			int linenum = 0;
			var lens = new List<int>();
			var D = new List<byte>();
			lens.Add(0);
			foreach (var filename in File.ReadAllLines (listname)) {
				linenum++;
				Console.WriteLine ("**** Loading line-number: {0}, file: {1}", linenum, filename);
				var data = BinQ8HammingSpace.LoadObjectFromFile (filename, false);
				//D.Capacity += data.Count;
				foreach (var b in data) {
					D.Add(b);
				}
				lens.Add(lens[lens.Count-1]+data.Count);
			}
			this.LENS = BitmapBuilders.GetSArray().Invoke(lens);
			this.Data = D;
		}

		public object this [int docid] {
			get {
				// int startIndex = docid * this.Q;
				//int startIndex = docid * this.SymbolSize;
				//return new ListGen<byte> (delegate(int i) {
				//	return this.Data[startIndex + i];
				//}, this.Q);
				return new BinQGram (this.Data, docid * this.SymbolSize, this.Q);
			}
		}
				
		public IList<byte> GetAudio (int audioId)
		{
			var startPos = this.LENS.Select1(audioId+1);
			var len = this.LENS.Select1(audioId+2);
			return new BinQGram (this.Data, startPos, len);
		}
		
		public int Count {
			get {
				// return this.Data.Count / this.Q;
				return (this.Data.Count - Q) / this.SymbolSize;
			}
		}

		public int GetDocIdFromBlockId (int blockId)
		{
			blockId *= this.SymbolSize;
			int rank1 = this.LENS.Rank1(blockId);
			return rank1-1;
		}
	}
}

