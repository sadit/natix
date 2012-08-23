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
//   Original filename: natix/CompactDS/IntCoders/BinaryCodes/IEncoderGenericIO.cs
// 
using System;
using System.IO;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// Save/Load IIntegerEncoder objects
	/// </summary>
	public class IEncoder64GenericIO
	{
		public static List<Type> Catalog = new List<Type>() {
			typeof(BinarySearchCoding64),
			typeof(DoublingSearchCoding64),
			typeof(UltimateSearchCoding64),
			typeof(EliasGamma64),
			typeof(EliasDelta64)
		};
		
		/// <summary>
		/// Saves "coder" to the binary file "Output"
		/// </summary>
		public static void Save (BinaryWriter Output, IIEncoder64 coder)
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
				var s = String.Format ("Type {0} is not a recognized IIEncoder64, please add it to " +
					"IntegerEncoderGenericIO.Catalog", type);
				throw new ArgumentException (s);
			}
			Output.Write (idType);
			coder.Save (Output);
		}

		/// <summary>
		/// Loads a "coder" from the binary file "Input"
		/// </summary>

		public static IIEncoder64 Load (BinaryReader Input)
		{
			byte idType = Input.ReadByte ();
			var type = Catalog[idType];
			if (type == null) {
				var s = String.Format ("IEncoder64GenericIO.Catalog returned null using idType: {0}," +
					"is it deprecated?", idType);
				throw new ArgumentNullException (s);
			}
			var coder = (IIEncoder64)Activator.CreateInstance (type);
			coder.Load (Input);
			return coder;
		}
		
	}
}

