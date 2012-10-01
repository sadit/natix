// 
//  Copyright 2012 Eric Sadit Tellez Avila, donsadit@gmail.com
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
using natix;

namespace natix.CompactDS
{
	public class GraphSeq : IGraph
	{
		public IRankSelect LENS;
		public IRankSelectSeq SEQ;
		
		public GraphSeq ()
		{
		}

		public void Build (string filename, SequenceBuilder seqbuilder, BitmapFromBitStream bitmapbuilder = null)
		{
			this.BuildWebGraph (filename, seqbuilder, bitmapbuilder);
		}
		
		public void BuildWebGraph (string filename, SequenceBuilder seqbuilder, BitmapFromBitStream bitmapbuilder = null)
		{
			if (bitmapbuilder == null) {
				bitmapbuilder = BitmapBuilders.GetGGMN_wt (12);
			}
			var len_stream = new BitStream32 ();
			var seq = new List<int> ();
			int prev_context = -1;
			using (var Input = File.OpenText (filename)) {
				string line;
				int lineno = 0;
				int counterlineno = 0;
				while (true) {
					{
						if (lineno % 10000 == 0) {
							if (counterlineno % 10 == 0) {
								Console.WriteLine ();
								Console.Write ("Processing lines: ");
							}
							++counterlineno;
							Console.Write ("{0}, ", lineno);
						}
						++lineno;
					}
					line = Input.ReadLine ();
					if (line == null) {
						break;
					}
					if (line.StartsWith ("#")) {
						continue;
					}
					var link = line.Split ('\t', ' ');
					var start_node = int.Parse (link [0]);
					var end_node = int.Parse (link [1]);
					// on webgraph format, starting nodes are already sorted, just advance and count
					if (start_node != prev_context) {
						for (int diffcount = start_node - prev_context; diffcount > 0; --diffcount) {
							len_stream.Write (true);
						}
						prev_context = start_node;
					}
					len_stream.Write (false);
					seq.Add (end_node);
				}
				// a simple hack simplifying  direct-neighbors's retrieval
				len_stream.Write (true);
			}
			this.SEQ = seqbuilder (seq, prev_context + 1);
			this.LENS = bitmapbuilder (new FakeBitmap (len_stream));
		}
		
		public void Save(BinaryWriter Output)
		{
			RankSelectGenericIO.Save(Output, this.LENS);
			RankSelectSeqGenericIO.Save (Output, this.SEQ);
		}
		
		public void Load (BinaryReader Input)
		{
			this.LENS = RankSelectGenericIO.Load (Input);
			this.SEQ = RankSelectSeqGenericIO.Load (Input);
		}
		
		
		public int CountEdges {
			get {
				return this.LENS.Rank0 (this.LENS.Count - 1);
			}
		}
		
		public int CountVertices {
			get {
				return this.LENS.Count1 - 1;
			}
		}
		
		public IList<int> GetDirect (int node)
		{
			int start_pos = this.LENS.Select1 (node + 1);
			int end_pos = this.LENS.Select1 (node + 2);
			int len = end_pos - start_pos - 1;
			// rank0 = pos + 1 - rank1 ==> seq_start_index = start_pos + 1 - (node+1)
			int seq_start_index = start_pos - node;
			Func<int,int> D = (int i) => this.SEQ.Access (seq_start_index + i);
			return new ListGen<int>(D, len);
		}
		
		public IList<int> GetReverse (int node)
		{
			return new SortedListRSCache(this.SEQ.Unravel (node));
		}
	}
}
