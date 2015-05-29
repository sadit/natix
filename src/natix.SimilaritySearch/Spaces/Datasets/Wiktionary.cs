//
// Copyright 2015 Eric S. Tellez <eric.tellez@infotec.com.mx>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class Wiktionary : MetricDB
	{
		public class DictItem : ILoadSave
		{
			public string Lang;
			public string Word;
			public string EntityType;
			public string Data;

			public DictItem()
			{
			}

			public DictItem (string lang, string word, string type, string data)
			{
				this.Lang = lang;
				this.Word = word;
				this.EntityType = type;
				this.Data = data;
			}

			public void Load(BinaryReader Input)
			{
				this.Lang = Input.ReadString ();
				this.Word = Input.ReadString ();
				this.EntityType = Input.ReadString ();
				this.Data = Input.ReadString ();
			}

			public void Save (BinaryWriter Output)
			{
				Output.Write (this.Lang);
				Output.Write (this.Word);
				Output.Write (this.EntityType);
				Output.Write (this.Data);
			}

		}

		public void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			var len = Input.ReadInt32 ();
			this.Items = new List<DictItem> (len);
			CompositeIO<DictItem>.LoadVector (Input, len, this.Items);
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.Name);
			Output.Write (this.Count);
			CompositeIO<DictItem>.SaveVector (Output, this.Items);
		}

		public double Dist(object _a, object _b)
		{
			++this.numdist;
			var a = _a as DictItem;
			var b = _b as DictItem;
			return StringLevenshteinSpace.Levenshtein (a.Word, b.Word);
		}

		long numdist = 0;
		string name = "";

		public long NumberDistances {
			get {
				return this.numdist;
			}
		}

		public string Name {
			get {
				return this.name;
			}
			set {
				this.name = value;
			}
		}
		public List<DictItem> Items = new List<DictItem>();

		public int Count
		{
			get {
				return this.Items.Count;
			}
		}

		public object this[int i]
		{
			get {
				return this.Items [i];
			}
		}

		public Wiktionary ()
		{
		}

		public void Build(string filename)
		{
			this.Name = filename;
			Console.WriteLine ("**** creating binary string from {0}", filename);
			var lines = File.ReadAllLines (filename);
			foreach (var line in lines) {
				if (this.Count % 10000 == 0) {
					Console.WriteLine ("**** {0} advance {1}/{2}", filename, this.Count, lines.Length);
				}
				this.Add (line);
			}
		}

		public int Add(object a)
		{
			var s = a as string;
			if (a == null) {
				throw new ArgumentException ("object should be an string");
			}

			var arr = s.Split ('\t');
			if (arr.Length == 1) {
				var item = new DictItem (null, s, null, null);
				this.Items.Add (item);
			} else {
				var item = new DictItem (arr [0], arr [1], arr [2], arr [3]);
				this.Items.Add (item);
			}

			return this.Items.Count - 1;
		}

		public object Parse(string word)
		{
			return new DictItem (null, word, null, null);
		}

	}
}

