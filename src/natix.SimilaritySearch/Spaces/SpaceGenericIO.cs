//
//   Copyright 2012-2015 Eric Sadit Tellez <eric.tellez@infotec.com.mx>
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
		public static Dictionary< string, MetricDB > DB_CACHE = new Dictionary< string, MetricDB >();
		public static string NormalizePath (string path)
		{
		    return Path.GetFullPath(path);
		}


		public static void SaveCache(string path, MetricDB db) 
		{
		    var key = Path.GetFileName(path);
		    DB_CACHE [key] = db;
		}

		public static bool LoadCache(string path, out MetricDB db)
		{
		    var key = Path.GetFileName(path);
		    MetricDB val;
		    if (DB_CACHE.TryGetValue(key, out val)) {
				db = val;
				return true;
		    } else {
				db = null;
				return false;
		    }
		}

		public static MetricDB Load (string path, bool remember_in_cache = true, bool try_load_from_cache = true)
		{
		    if (path == "") {
				return new NullSpace();
		    }
			Console.WriteLine("XXX BEGIN Load MetricDB: '{0}', save_into_cache: {1}", path, remember_in_cache);
		    MetricDB sp = null;
			if (try_load_from_cache && LoadCache(path, out sp)) {
				Console.WriteLine ("XXX Using the cached value");
				return sp;
		    }

		    if (!File.Exists (path)) {
				path = Path.GetFileName (path);
		    }
		    using (var Input = new BinaryReader(File.OpenRead(path))) {
				sp = Load (Input, false);
		    }
		    sp.Name = path;
		    if (remember_in_cache) {
				SaveCache(path, sp);
		    }
		    Console.WriteLine ("XXX END Loaded '{0}', type: {1}", sp.Name, sp);
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
			SaveCache(sp.Name, sp);
		    }
		    return sp;
		}

		public static void Save (string path, MetricDB sp, bool save_into_cache = true)
		{
		    sp.Name = NormalizePath(path);
		    using (var Output = new BinaryWriter(File.Create(path))) {
				Save (Output, sp, save_into_cache);
		    }
		}

		public static void Save (BinaryWriter Output, MetricDB sp, bool save_into_cache)
		{
		    if (sp.Name != "") {
				sp.Name = sp.Name;
		    }
		    Output.Write(sp.GetType ().ToString());
		    sp.Save(Output);

		    if (save_into_cache) {
				SaveCache(sp.Name, sp);
		    }
		}

		public static void SmartSave (BinaryWriter Output, MetricDB db)
		{
		    if (db.Name == null) {
				db.Name = "";
		    }
		    if (db.Name != "") {
				db.Name = NormalizePath (db.Name);
		    }
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
