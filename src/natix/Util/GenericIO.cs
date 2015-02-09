//
//   Copyright 2013 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
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
//   Original filename: natix/Util/LoadSaveIO.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix
{
	/// <summary>
	/// Simple I/O output for objects implementing the ILoadSave interface
	/// </summary>
	public class GenericIO<T> where T: ILoadSave
    {

		public static T Load(string path)
		{
			using (var Input = new BinaryReader(File.OpenRead(path))) {
				return Load (Input);
			}
		}

		public static void Save(string path, T obj)
		{
			using (var Output = new BinaryWriter(File.Create(path))) {
				Save (Output, obj);
			}
		}

        public static T Load(BinaryReader Input)
        {
			T new_object;
			var typename = Input.ReadString ();
			var type = Type.GetType (typename);
			new_object = (T)Activator.CreateInstance (type);
			new_object.Load (Input);
			return new_object;
		}

		public static void Save (BinaryWriter Output, T obj)
		{
			Output.Write(obj.GetType().ToString());
			obj.Save(Output);
		}

	}
}

