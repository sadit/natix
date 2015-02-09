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
//   Original filename: natix/CompactDS/Sequences/RankSelectSeqGenericIO.cs
// 
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace natix.CompactDS
{
	/// <summary>
	/// Save/Load an index of sequences
	/// </summary>
	public class SymbolCoderGenericIO
	{
		static List<Type> Catalog = new List<Type>() {
			typeof(EqualSizeCoder),
			typeof(TopKFreqCoder)
		};
		
		/// <summary>
		/// Saves "seq" to the "Output"
		/// </summary>

		public static void Save (BinaryWriter Output, ISymbolCoder coder)
		{
			var type = coder.GetType ();
			byte idType = 255;
			for (byte i = 0; i < Catalog.Count; i++) {
				if (type == Catalog [i]) {
					idType = i;
					break;
				}
			}
			if (idType == 255) {
				var s = String.Format ("Type {0} is not a recognized SymbolCoder", type);
				throw new ArgumentException (s);
			}
			Output.Write (idType);
			coder.Save (Output);
		}


		public static ISymbolCoder Load (BinaryReader Input)
		{
			byte idType = Input.ReadByte ();
			var type = Catalog[idType];
			if (type == null) {
				var s = String.Format ("ISymbolCoderGenericIO.Catalog returned null " +
					"using idType: {0}, is it deprecated?", idType);
				throw new ArgumentNullException (s);
			}
			var coder = (ISymbolCoder)Activator.CreateInstance (type);
			coder.Load (Input);
			return coder;
		}
	}
}
