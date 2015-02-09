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
		public static Dictionary< string, MetricDB > CACHE = new Dictionary< string, MetricDB >();
		public static Func< string, string >  NormalizePath = (string path) => Path.GetFullPath(path);
		public static Func< string, MetricDB > CacheHandler = (string path) => {
			if (CACHE.ContainsKey(path)) {
				return CACHE[path];
			}
			return null;
		};

		public static void SetCacheHandler(Func<string, MetricDB> cacheHandler)
		{
			CacheHandler = cacheHandler;
		}

		public static MetricDB Load (string path, bool remember_in_cache = true, bool try_load_from_cache = true)
		{
			if (path == "") {
				return new NullSpace();
			}
			path = NormalizePath (path);
			MetricDB sp = null;
			if (try_load_from_cache) {
				sp = CacheHandler (path);
			}
			Console.WriteLine("XXX Loading MetricDB: '{0}', save_into_cache: {1}", path, remember_in_cache);
			if (sp != null) {
				Console.WriteLine ("XXX Using the cached value");
				return sp;
			}
			using (var Input = new BinaryReader(File.OpenRead(path))) {
				sp = Load (Input, false);
			}
			sp.Name = path;
			if (remember_in_cache) {
				CACHE[sp.Name] = sp;
			}
			Console.WriteLine ("XXX Loaded '{0}', type: {1}", path, sp);
			return sp;
		}

		public static MetricDB Load (BinaryReader Input, bool save_into_cache)
		{
			var typename = Input.ReadString ();
			var type = Type.GetType (typename);
			Console.WriteLine ("XXX Loading Typename: '{0}'", typename);
			MetricDB sp = (MetricDB)Activator.CreateInstance (type);
			sp.Load (Input);
			if (save_into_cache) {
				CACHE[Path.GetFullPath(sp.Name)] = sp;
			}
			return sp;
		}

		public static void Save (string path, MetricDB sp, bool save_into_cache = true)
		{
			path = NormalizePath (path);
			sp.Name = path;
			using (var Output = new BinaryWriter(File.Create(path))) {
				Save (Output, sp, save_into_cache);
			}
			// sp.Name = prevname;
		}

		public static void Save (BinaryWriter Output, MetricDB sp, bool save_into_cache)
		{
		    if (sp.Name != "") sp.Name = NormalizePath (sp.Name);
		    Output.Write(sp.GetType ().ToString());
		    sp.Save(Output);
		    if (save_into_cache) {
			CACHE[sp.Name] = sp;
		    }
		}

		public static void SmartSave (BinaryWriter Output, MetricDB db)
		{
			if (db.Name == null) {
				db.Name = "";
			}
			if (db.Name != "") db.Name = NormalizePath (db.Name);
			Output.Write (db.Name);
			if (db.Name == "") {
				SpaceGenericIO.Save(Output, db, false);
			}
		}

		public static MetricDB SmartLoad (BinaryReader Input, bool save_cache)
		{
			var dbname = Input.ReadString ();
			if (dbname == "") {
				return SpaceGenericIO.Load(Input, false);
			} else {
				return SpaceGenericIO.Load(dbname, save_cache);
			}
		}

	}
}
