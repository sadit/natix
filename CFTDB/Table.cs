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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using natix.Sets;
using natix.CompactDS;
using natix.SortingSearching;
using natix.InformationRetrieval;

namespace cftdb
{
	public class Table
	{
		public Column[] Columns;
		public BasicTokenizer InputTokenizer;

		public Table ()
		{
		}

		public void Build (string path, int _num_cols, BasicTokenizer tokenizer, SequenceBuilder seq_builder)
		{
			Console.WriteLine ("*** building Table: '{0}', with {1} columns", path, _num_cols);
			var input = new StreamReader (File.OpenRead (path));
			var C = new ColumnBuilder[ _num_cols ];
			int numcol = 0;
			int numrec = 0;
			var recsep = tokenizer.RecordSeparator.ToString();

			for (int i = 0; i < _num_cols; ++i) {
				C [i] = new ColumnBuilder ();
				C [i].Add(recsep);
			}
			foreach (var p in tokenizer.Parse(input, false)) {
				// Console.WriteLine("<{0}>", p.Data);
				if (p.DataType == TokenType.FieldSeparator) {
					C[numcol].Add(recsep);
					++numcol;
					continue;
				}
				if (p.DataType == TokenType.RecordSeparator) {
					if (numrec % 10000 == 0) {
						Console.WriteLine("-- record: {0}, date-time: {1}", numrec, DateTime.Now);
					}
					while (numcol < _num_cols) {
						C[numcol].Add(recsep);
						// C[numcol].Add("");
						++numcol;
					}
					++numrec;
					numcol = 0;
					continue;
				}
				//if (p.DataType == TokenType.Data) {
				C[numcol].Add(p.Data);
				//}
				//Console.WriteLine ("===> type: {0}, data: '{1}'", p.DataType, p.Data);
			}
			this.InputTokenizer = tokenizer;
			this.Columns = new Column[_num_cols];
			for (int i = 0; i < _num_cols; ++i) {
				Console.WriteLine ("*** compressing column-{0} of '{1}'", i, path);
				C[i].Add (recsep);
				this.Columns[i] = C[i].Finish(recsep, seq_builder);
			}
		}

		public void Load(BinaryReader Input)
		{
			this.InputTokenizer = new BasicTokenizer();
			this.InputTokenizer.Load(Input);
			int len = Input.ReadInt32();
			this.Columns = new Column[len];
			for (int i = 0; i < len; ++i) {
				this.Columns[i] = new Column();
				this.Columns[i].Load(Input);
			}
		}

		public void Save(BinaryWriter Output)
		{
			this.InputTokenizer.Save(Output);
			Output.Write((int)this.Columns.Length);
			for (int i = 0; i < this.Columns.Length; ++i) {
				this.Columns[i].Save(Output);
			}
		}
	
		public StringBuilder GetTextRecord (StringBuilder s, int rec_id)
		{
			for (int i = 0; i < this.Columns.Length; ++i) {
				// Console.WriteLine("----------- column-id: {0}, rec-id: {1}", i, rec_id);
				this.Columns[i].GetTextCell(s, rec_id);
				if (i + 1 < this.Columns.Length) {
					s.Append(this.InputTokenizer.FieldSeparator);
				}
			}
			return s;
		}
	}
}

