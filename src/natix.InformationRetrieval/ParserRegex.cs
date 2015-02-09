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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using natix;
using System.IO;

namespace natix.InformationRetrieval
{
	public class ParserRegex : ILoadSave
	{
		/// <summary>
		/// Regular expressions to recognize valid entities-words in the text
		/// </summary>
		/// 
		//static Regex RegexWhiteSpace = new Regex (@"(\s*)(\S{1,20})(\s*)",
		// TODO: the length of entities must be upper bounded
		public Regex RegexWhiteSpace;
		public Regex RegexWord;
		/// <summary>
		/// Separator for files, necessarily an invalid word
		/// It must be invalid and not recognized neither by regex_white_spaces nor regex_word.
		/// </summary>
		public string FileSeparator;
		public string str_white_space_regex;
		public string str_word_regex;
		
		public ParserRegex ()
		{
		}
		
		public ParserRegex (string _str_white_space_regex, string _str_word_regex, string file_sep)
		{
			this.str_word_regex = _str_word_regex;
			this.str_white_space_regex = _str_white_space_regex;
			this.FileSeparator = file_sep;
			this.SetRegex ();
		}
		
		public void Load (BinaryReader Input)
		{
			this.str_word_regex = Input.ReadString ();
			this.str_white_space_regex = Input.ReadString ();
			this.FileSeparator = Input.ReadString ();
			this.SetRegex ();
		}
		
		void SetRegex ()
		{
			var options = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant;
			this.RegexWord = new Regex (this.str_word_regex, options);
			this.RegexWhiteSpace = new Regex (this.str_white_space_regex, options);
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.str_word_regex);
			Output.Write (this.str_white_space_regex);
			Output.Write (this.FileSeparator);
		}
		
		/// <summary>
		/// Gets a parser for a text composed of concatenation of ((WORD SEP)* | SEP*)*
		/// </summary>
		public static ParserRegex GetSimpleTextParser ()
		{
			var white = @"(\s*)(\S+)(\s*)";
			var word = @"(\W*)(\w{1,20})(\W*)";
			return new ParserRegex (white, word, " $newfile$ ");
		}
		
		/// <summary>
		/// Gets a parser for a text composed of ((WORD SEP) | SEP* | SYM*)* where SYM is in [^\s,/:;!\?¡¿&%\$\.]
		/// </summary>
		public static ParserRegex GetTextParser ()
		{
			var white = @"(\s*)(\S+)(\s*)";
			// var word = @"(\W*)(\w[^,/:;!\?¡¿&%\$\.]*)(\W*)";
			var word = @"(\W*)(\w+)(\W*)";
			return new ParserRegex (white, word, " $newfile$ ");
		}
	}
}