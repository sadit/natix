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
//   Original filename: natix/SimilaritySearch/Indexes/IndexLoader.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using natix;
using natix.SimilaritySearch;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Generic I/O output for managing Index objects
	/// </summary>
	public class IndexGenericIO
	{

		public static Func< string, string > NormalizePath = (string path) => path;

		/// <summary>
		/// Returns the object Type of the index, this should be done before the generic creation.
		/// </summary>

		public static Index Load (string path, string indexclass = null, Action<Index> after_load_action = null)
		{
			Index I;
            // Console.WriteLine ("=== loading index, path: {0}", path);
			using (var Input = new BinaryReader(File.OpenRead(NormalizePath(path)))) {
				I = Load (Input, indexclass, after_load_action);
			}
			return I;
		}

		public static Index Load (BinaryReader Input, string indexclass = null, Action<Index> after_load_action = null)
		{
			Index I;
			var typename = Input.ReadString ();
			if (indexclass != null) {
				typename = indexclass;
			}
            // Console.WriteLine ("=== loading index, type: {0}", typename);
			var type = Type.GetType (typename);
			I = (Index)Activator.CreateInstance (type);
			I.Load (Input);

			if (after_load_action != null) {
				after_load_action (I);
			}
			return I;
		}

		public static void Save (string path, Index I)
		{
			using (var Output = new BinaryWriter(File.Create(NormalizePath(path)))) {
				Save (Output, I);
			}
		}

		public static void Save (BinaryWriter Output, Index idx)
		{
			Output.Write(idx.GetType().ToString());
			idx.Save(Output);
		}
	}
}
