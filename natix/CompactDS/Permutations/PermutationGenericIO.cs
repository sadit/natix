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
//   Original filename: natix/CompactDS/Permutations/PermutationGenericIO.cs
// 
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace natix.CompactDS
{
	/// <summary>
	/// Load/Save IPermutation objects
	/// </summary>
	public class PermutationGenericIO
	{
		static List<Type> Catalog = new List<Type>() {
			typeof(PlainPerms),
			typeof(CyclicPerms_MRRR),
			typeof(SuccCyclicPerms_MRRR),
			typeof(RLCyclicPerms_MRRR),
			typeof(SuccRLCyclicPerms_MRRR),
			typeof(SuccRL2CyclicPerms_MRRR),
			typeof(ListGen_MRRR)
		};
		
		/// <summary>
		/// Saves "u" to the "Output"
		/// </summary>
		public static void Save (BinaryWriter Output, IPermutation u)
		{
			var type = u.GetType ();
			byte idType = 255;
			for (byte i = 0; i < Catalog.Count; i++) {
				if (type == Catalog [i]) {
					idType = i;
					break;
				}
			}
			if (idType == 255) {
				var s = String.Format ("Type {0} is not a recognized IPerms, please add it to " +
					"PermsGenericIO.Catalog", type);
				throw new ArgumentException (s);
			}
			Output.Write (idType);
			u.Save (Output);
		}
		
		/// <summary>
		/// Loads a permutation from "Input"
		/// </summary>

		public static IPermutation Load (BinaryReader Input)
		{
			byte idType = Input.ReadByte ();
			var type = Catalog[idType];
			if (type == null) {
				var s = String.Format ("PermsGenericIO.Catalog returned null using idType: {0}, " +
					"is it deprecated?", idType);
				throw new ArgumentNullException (s);
			}
			var u = (IPermutation)Activator.CreateInstance (type);
			u.Load (Input);
			return u;
		}
	}
}
