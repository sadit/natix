// 
//  Copyright 2012  sadit
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
using natix;
using natix.SortingSearching;
using natix.Sets;
using natix.CompactDS;
using NDesk.Options;

namespace natix.InformationRetrieval
{
	public class SeqTextIR
	{
		public IList<string> FileNames;
		public IList<string> Voc;
		public IRankSelectSeq Seq;
		public Tokenizer InputTokenizer;
		protected int sep_symbol;
		
		public SeqTextIR ()
		{
		}
		
		public IRankSelectSeq SeqIndex {
			get {
				return this.Seq;
			}
		}
		
		public int Count {
			get {
				return this.Seq.Count;
			}
		}
		
		public virtual void Build (IEnumerable<string> list, SequenceBuilder seq_builder, IList<int> seq_container)
		{
			this.FileNames = new List<string> ();
			int docid = 0;
			this.InputTokenizer = new Tokenizer('\0', '\0', '\0');
			var parser = new TextParser(this.InputTokenizer, seq_container);
			foreach (var filename in list) {
				this.FileNames.Add (filename);
				parser.AddPlainString(parser.GetFileSeparator());
				parser.Parse (File.ReadAllText (filename));
				if (docid % 500 == 0) {
					Console.WriteLine ("== reviewing docid {0}, date-time: {1}", docid, DateTime.Now);
				}
				++docid;
			}
			this.Voc = MapVocSeq.SortingVoc (parser.Voc, parser.Seq);
			this.Seq = seq_builder (parser.Seq, this.Voc.Count);
			this.sep_symbol = this.RankVoc (parser.GetFileSeparator ());
		}
		
		public int RankVoc (string w)
		{
			return GenericSearch.FindLast<string> (w, this.Voc);
		}
		
		public bool RankVoc (string w, out int pos)
		{
			pos = GenericSearch.FindLast<string> (w, this.Voc);
			return this.Voc [pos] == w;
		}

		public virtual void Load (string basename)
		{
			using (var input = new BinaryReader(File.OpenRead(basename))) {
				this.InputTokenizer = new Tokenizer();
				this.InputTokenizer.Load(input);
			}
			this.FileNames = File.ReadAllLines (basename + ".names");
			using (var input = new BinaryReader(File.OpenRead(basename + ".seq"))) {
				this.Seq = RankSelectSeqGenericIO.Load (input);
			}
			using (var input = new BinaryReader(File.OpenRead(basename + ".voc"))) {
				var size = input.ReadInt32 ();
				this.Voc = new string[size];
				for (int i = 0; i < size; ++i) {
					this.Voc [i] = input.ReadString ();
				}
			}
			this.sep_symbol = this.RankVoc (this.InputTokenizer.RecordSeparator.ToString());
		}
		
		public virtual void Save (string basename)
		{
			using (var output = new BinaryWriter(File.Create(basename))) {
				this.InputTokenizer.Save(output);
			}	
			using (var output = File.CreateText(basename + ".names")) {
				foreach (var filename in this.FileNames) {
					output.WriteLine (filename);
				}
			}
			using (var output = new BinaryWriter(File.Create(basename + ".seq"))) {
				RankSelectSeqGenericIO.Save (output, this.Seq);
			}
			using (var output = new BinaryWriter(File.Create(basename + ".voc"))) {
				output.Write ((int)this.Voc.Count);
				foreach (var w in this.Voc) {
					output.Write (w);
				}
			}
		}
		
		public virtual IList<int> SearchPhrase (string query, IIntersection<int> ialg, Action<QueryParser> modify_query = null)
		{
			var qparser = new QueryParser (this.InputTokenizer);
			qparser.Parse (query);
			if (null != modify_query) {
				modify_query (qparser);
			}
			var posting = this.GetPostingLists (qparser);
			var _r = this.GetCandidates (qparser,  posting, ialg);
			var _s = _r as IList<int>;
			if (_s == null) {
				return new List<int> (_r);
			}
			return _s;
		}
	
		protected virtual IList<IList<int>> GetPostingLists (QueryParser qparser)
		{
			var posting_lists = new List<IList<int>> ();
			for (int i = 0; i < qparser.Query.Count; ++i) {
				int symbol;
				if (qparser.Query[i] != null && this.RankVoc (qparser.Query [i], out symbol)) {
					var rs = this.Seq.Unravel (symbol);
					var L = new SortedListRSCache (rs, -i);
					posting_lists.Add (L);					
				}
			}
			return posting_lists;
		}
		
		protected virtual IEnumerable<int> GetCandidates(QueryParser qparser, IList<IList<int>> posting_lists, IIntersection<int> ialg)
		{
			if (posting_lists.Count == 0) {
				return new int[0];
			} else if (posting_lists.Count == 1) {
				return posting_lists [0];
			} else {
				return ialg.Intersection (posting_lists);
			}
		}
		
		public string GetSnippet (int occpos, int snippet_max_len, out string uri, out int docid, out int sp, out int len)
		{
			docid = this.GetDocidFromOccpos(occpos);
			uri = this.GetFileName (docid);
			this.GetFileLocation (docid, out sp, out len);
			int snippet_sp = Math.Max (sp, occpos - snippet_max_len / 2);
			return this.ExtractWithSepBounds (snippet_sp, snippet_max_len);
		}
		
		public int GetDocidFromOccpos (int occpos)
		{
			return this.Seq.Rank (this.sep_symbol, occpos) - 1;
		}
		
		public void GetFileLocation (int docid, out int sp, out int len)
		{
			sp = 1 + this.Seq.Select (this.sep_symbol, docid + 1);
			len = this.Seq.Select (this.sep_symbol, docid + 2) - sp;
		}
		
		public string GetFileData (int docid, out string filename, out int sp, out int len)
		{
			this.GetFileLocation (docid, out sp, out len);
			filename = this.GetFileName (docid);
			return this.Extract (sp, len);			
		}

		public string Extract (int sp, int len)
		{
			StringWriter output = new StringWriter ();
			for (int i = 0; i < len; ++i) {
				var u = this.Voc [this.Seq.Access (sp + i)];
				output.Write (u);
			}
			return output.ToString ();
		}

		public string ExtractWithSepBounds (int sp, int max_len)
		{
			StringWriter output = new StringWriter ();
			for (int i = 0; i < max_len; ++i) {
				var sym = this.Seq.Access (sp + i);
				if (sym == this.sep_symbol) {
					break;
				}
				var u = this.Voc [sym];
				output.Write (u);
			}
			return output.ToString ();
		}

		public virtual string GetFileName (int docid)
		{
			return this.FileNames [docid];
		}
	}
}