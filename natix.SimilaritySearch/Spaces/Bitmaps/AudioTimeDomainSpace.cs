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
//   Original filename: natix/SimilaritySearch/Spaces/AudioTimeDomainSpace.cs
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
		public IBitStream stream;
		public long start;
		public int len;

		public QGramH1 (IBitStream _stream, long _start, int _len)
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
	}

	public class AudioTimeDomainSpace : MetricDB
	{
		public int Q;
		IBitStream Data;
		public ListIDiff ListOfLengths;
		int numdist = 0;
		// IList<string> NameList;

		public void Load (BinaryReader Input)
		{
			this.Q = Input.ReadInt32 ();
			this.Name = Input.ReadString ();
			this.Data = new BitStream32();
			this.Data.Load(Input);
			this.ListOfLengths = new ListIDiff();
			this.ListOfLengths.Load(Input);
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write ((int) this.Q);
			Output.Write (this.Name);
			this.Data.Save(Output);
			this.ListOfLengths.Save(Output);
		}

		public void Build (string listname, int qsize)
		{
			this.Q = qsize;
			int linenum = 0;
			this.ListOfLengths = new ListIDiff ();
			this.Data = new BitStream32();
			foreach (var filename in File.ReadAllLines (listname)) {
				linenum++;
				Console.WriteLine ("**** Loading line-number: {0}, file: {1}", linenum, filename);
				var data = BinQ8HammingSpace.LoadObjectFromFile (filename, false);
				foreach (var u in data) {
					this.Data.Write(u, 8);
				}
				if (data.Count % this.Q > 0) {
					// padding
				}
				this.ListOfLengths.Add (data.Count);
			}
		}

		public string Name {
			get;
			set;
		}

		public object Parse (string name, bool isquery)
		{
			return this._Parse (name, isquery);
		}

		public QGramH1 _Parse (string name, bool isquery)
		{
			if (name.StartsWith ("obj")) {
				var A = name.Split (' ');
				var id = A [1];
				return (QGramH1)this [int.Parse (id)];
			}
			var u = BinQ8HammingSpace.LoadObjectFromFile (name, !isquery);
			var b = new BitStream32(u);
			return new QGramH1(b, 0, this.Q);
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
			return BinQ8HammingSpace.DistMinHamming ((IList<byte>)a, (IList<byte>)b, 1);
		}

		public AudioTimeDomainSpace () : this(30)
		{
		}

		public AudioTimeDomainSpace (int qsize)
		{
			this.Q = qsize;
		}

		public object this [int docid] {
			get {
				return new QGramH1(this.Data, docid, this.Q);
			}
		}
				
		public QGramH1 GetAudio (int audioId)
		{
		    var dset = this.ListOfLengths.dset;
			var startPos = 0;
			if (audioId > 0) {
				startPos = dset.Select1 (audioId);
			}
			var len = this.ListOfLengths [audioId];
			startPos -= audioId - 1;
			return new QGramH1 (this.Data, startPos, len);
		}
		
		public int Count {
			get {
				// return this.Data.Count / this.Q;
				return (int)(this.Data.CountBits - this.Q);
			}
		}

		public int GetDocIdFromBlockId (int blockId)
		{
			int rank1 = this.ListOfLengths.dset.Rank1 (blockId);
			return this.ListOfLengths.dset.Rank1 (blockId + rank1);
		}
	}
}

