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
using natix;
using natix.CompactDS;
using cftdb;

namespace testcftdb
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var outname = "db.test";
			if (!File.Exists(outname)) {
				Tokenizer tokenizer = new Tokenizer ('\t', '\n', (char)0x0);
				//Tokenizer tokenizer = new Tokenizer ('/', '\n', (char)0x0);
				Table table = new Table ();
				table.Build (args [0], int.Parse (args [1]), tokenizer, SequenceBuilders.GetSeqXLB_DiffSet64 (16, 31));
				using (var Output = new BinaryWriter(File.Create(outname))) {
					table.Save (Output);
				}
			}
			{
				Table table = new Table();
				using (var Input = new BinaryReader(File.OpenRead(outname))) {
					table.Load(Input);

					for (int i = 0; i < 3; ++i) {
						var s = table.GetTextRecord(new StringBuilder(), i).ToString();
						Console.WriteLine("=== record {0}: {1}", i, s);
					}
				}
			}
		}
	}
}
