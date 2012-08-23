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
//   Original filename: natix/CompactDS/Bitmaps/RankSelect64GenericIO.cs
// 
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace natix.CompactDS
{
	/// <summary>
	/// Save and load operations for generic IRankSelect objects
	/// </summary>
	public class RankSelect64GenericIO
	{
		/// <summary>
		/// The catalog of RankSelect known types
		/// </summary>
		static List<Type> Catalog = new List<Type>() {
			typeof(SArray64),
			typeof(DiffSetRL2_64),
			typeof(DiffSet64),
			typeof(DiffSetRL64)
		};

		/// <summary>
		/// Save the specified bitmap to the Output stream
		/// </summary>
		public static void Save (BinaryWriter Output, IRankSelect64 bitmap)
		{
			var type = bitmap.GetType ();
			byte idType = 255;
			for (byte i = 0; i < Catalog.Count; i++) {
				if (type == Catalog [i]) {
					idType = i;
					break;
				}
			}
			if (idType == 255) {
				var s = String.Format ("Type {0} is not a recognized bitmap, please add it to " +
					"RankSelect64GenericIO.Catalog", type);
				throw new ArgumentException (s);
			}
			Output.Write (idType);
			bitmap.Save (Output);
		}
		
		/// <summary>
		/// Load an IRankSelect64 object from the binary stream
		/// </summary>
		public static IRankSelect64 Load (BinaryReader Input)
		{
			byte idType = Input.ReadByte ();
			var type = Catalog[idType];
			if (type == null) {
				var s = String.Format ("RankSelect64GenericIO.Catalog returned null using idType: {0}," +
					"is it deprecated?", idType);
				throw new ArgumentNullException (s);
			}
			var rs = (IRankSelect64)Activator.CreateInstance (type);
			rs.Load (Input);
			return rs;
		}
	}
}
