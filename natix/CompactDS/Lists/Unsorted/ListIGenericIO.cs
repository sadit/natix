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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListIGenericIO.cs
// 
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace natix.CompactDS
{
	/// <summary>
	/// Save/Load lists of integers
	/// </summary>
	public class ListIGenericIO
	{
		static List<Type> Primitive = new List<Type>() {
			typeof(int[]),
			typeof(List<int>),
			typeof(IList<int>)
		};
		static List<Type> Catalog = new List<Type>() {
			typeof(ListIDiff),
			typeof(ListIFS),
			typeof(ListIRRR),
			typeof(ListIWT),
			typeof(ListIWT8),
			typeof(ListRL),
			typeof(ListRL2),
			typeof(ListSDiff),
			typeof(ListSDiff64),
			typeof(ListSDiffCoder),
			typeof(ListSDiffCoder64),
			typeof(ListSDiffCoderRL)
		};
	
		/// <summary>
		/// Gets the type identifier (index) of "type" in "C". Returns 255 if "type" is not in "C"
		/// </summary>
		public static byte GetTypeId (List<Type> C, Type type)
		{
			byte idType = 255;
			for (byte i = 0; i < C.Count; i++) {
				if (type == C [i]) {
					idType = i;
					break;
				}
			}
			return idType;
		}
		
		/// <summary>
		/// Saves the "list" to "Output"
		/// </summary>
		public static void Save (BinaryWriter Output, IList<int> seq)
		{
			var type = seq.GetType ();
			var idType = (byte)GetTypeId (Catalog, type);
			if (idType == 255) {
				idType = (byte)GetTypeId (Primitive, type);
				if (idType == 255) {
					var s = String.Format ("Type {0} is not a recognized indexed sequence, please add it to " +
					                       "ListIntegersGenericIO.Catalog", type);
					throw new ArgumentException (s);
				}
				Output.Write ((byte)255);
				Output.Write (seq.Count);
				PrimitiveIO<int>.WriteVector (Output, seq);
			} else {
				var S = seq as ILoadSave;
				Output.Write (idType);
				S.Save (Output);
			}
		}
		
		/// <summary>
		/// Load a list from the specified Input.
		/// </summary>
		public static IList<int> Load (BinaryReader Input)
		{
			byte idType = Input.ReadByte ();
			if (idType == 255) {
				var len = Input.ReadInt32 ();
				var array = new int[len];
				PrimitiveIO<int>.ReadFromFile (Input, len, array);
				return array;
			} else {
				var type = Catalog [idType];
				if (type == null) {
					var s = String.Format ("ListIGenericIO.Catalog returned null " +
					                       "using idType: {0}, is it deprecated?", idType);
					throw new ArgumentNullException (s);
				}
				var seq = (ILoadSave)Activator.CreateInstance (type);
				seq.Load (Input);
				return (IList<int>)seq;
			}
		}

	}
}
