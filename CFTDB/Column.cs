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
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.Sets;
using natix.CompactDS;
using natix.SortingSearching;
using natix.InformationRetrieval;

namespace cftdb
{
	public class Column
	{
		public IRankSelectSeq Seq;
		public IList<string> Voc;
		public int RecSep;

		public Column ()
		{
		}

		public Column (IRankSelectSeq seq, IList<string> voc, string recsep)
		{
			this.Seq = seq;
			this.Voc = voc;
			this.GetWordId(recsep, out this.RecSep);
		}

		public void Load(BinaryReader Input)
		{
			this.Seq = RankSelectSeqGenericIO.Load (Input);
			this.RecSep = Input.ReadInt32 ();
			int len = Input.ReadInt32();
			this.Voc = new string[len];
			for (int i = 0; i < len; ++i) {
				this.Voc[i] = Input.ReadString();
			}
		}

		public void Save(BinaryWriter Output)
		{
			RankSelectSeqGenericIO.Save(Output, this.Seq);
			Output.Write((int)this.RecSep);
			Output.Write((int)this.Voc.Count);
			for (int i = 0; i < this.Voc.Count; ++i) {
				Output.Write(this.Voc[i]);
			}
		}

		public bool GetWordId (string s, out int symbol)
		{
			symbol = GenericSearch.FindLast<string>(s, this.Voc);
			return this.Voc [symbol] == s;
		}

		public string GetTextFromWordId (int word_id)
		{
			return this.Voc[word_id];
		}

		public StringBuilder GetTextFromWordId (StringBuilder s, int word_id)
		{
			s.Append( this.Voc[word_id] );
			return s;
		}

		public StringBuilder GetTextCell (StringBuilder s, int rec_id)
		{
			var pos = this.Seq.Select (this.RecSep, rec_id + 1);
			var pos_next = this.Seq.Select (this.RecSep, rec_id + 2);
			//Console.Write ("voc-len: {0}, text-len: {1}, rec-id: {2}, ", this.Voc.Count, this.Seq.Count, rec_id);
			//Console.WriteLine ("pos: {0}, pos_next: {1}, len: {2}",pos, pos_next, pos_next - pos - 1);
			for (++pos; pos < pos_next; ++pos) {
				var word_id = this.Seq.Access(pos);
				//var word = this.GetTextFromWordId(word_id);
				//s.Append("<" + word + ">");
				this.GetTextFromWordId(s, word_id);
			}
			return s;
		}

		public virtual IList<int> SearchPhrase (IList<string> query, IIntersection<int> ialg)
		{
			var posting = this.GetPostingLists (query);
			var _r = this.GetCandidates (posting, ialg);
			var _s = _r as IList<int>;
			if (_s == null) {
				return new List<int> (_r);
			}
			return _s;
		}
				
		protected virtual IList<IList<int>> GetPostingLists (IList<string> q)
		{
			var posting_lists = new List<IList<int>> ();
			for (int i = 0; i < q.Count; ++i) {
				int symbol;
				if (q[i] != null && this.GetWordId (q [i], out symbol)) {
					var rs = this.Seq.Unravel (symbol);
					var L = new SortedListRSCache (rs, -i);
					posting_lists.Add (L);					
				}
			}
			return posting_lists;
		}
		
		protected virtual IEnumerable<int> GetCandidates(IList<IList<int>> posting_lists, IIntersection<int> ialg)
		{
			if (posting_lists.Count == 0) {
				return new int[0];
			} else if (posting_lists.Count == 1) {
				return posting_lists [0];
			} else {
				return ialg.Intersection (posting_lists);
			}
		}

	}
}