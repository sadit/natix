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
//   Original filename: natix/SimilaritySearch/Spaces/SpaceCache.cs
// 
using System;
using System.Collections.Generic;
using System.IO;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Works as cache for loaded spaces and a loader of spaces.
	/// </summary>
	public class SpaceGenericIO
	{
		static Dictionary< string, MetricDB > cache = new Dictionary< string, MetricDB >();

		public static void SetCache(string path, MetricDB sp)
		{
			var full_path = Path.GetFullPath (sp.Name);
			cache[full_path] = sp;
		}

		public static MetricDB Load (string path, bool save_into_cache = true)
		{
			if (path == "") {
				return new NullSpace();
			}
			MetricDB sp;
			var full_path = Path.GetFullPath (path);
			if (cache.TryGetValue (full_path, out sp)) {
				return sp;
			}
			using (var Input = new BinaryReader(File.OpenRead(full_path))) {
				sp = Load (Input, save_into_cache);
			}
			return sp;
		}

		public static MetricDB Load (BinaryReader Input, bool save_into_cache)
		{
			var typename = Input.ReadString ();
			var type = Type.GetType (typename);
			MetricDB sp = (MetricDB)Activator.CreateInstance (type);
			sp.Load (Input);
			if (save_into_cache) {
				SetCache(sp.Name, sp);
			}
			return sp;
		}

		public static void Save (string path, MetricDB sp, bool save_into_cache = true)
		{
			var prevname = sp.Name;
			sp.Name = path;
			using (var Output = new BinaryWriter(File.Create(path))) {
				Save (Output, sp, save_into_cache);
			}
			sp.Name = prevname;
		}

		public static void Save (BinaryWriter Output, MetricDB sp, bool save_into_cache)
		{
			Output.Write(sp.GetType ().ToString());
			Console.WriteLine ("XXXX {0}", sp);
			sp.Save(Output);
			if (save_into_cache) {
				SetCache(sp.Name, sp);
			}
		}

		public static void RemoveCache (string dbname)
		{
			cache.Remove (Path.GetFullPath(dbname));
		}
		
		public static void RemoveAllFromCache ()
		{
			cache.Clear ();
		}
	}
}
