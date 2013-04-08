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
//   Original filename: natix/Util/LoadSaveIO.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix
{
	/// <summary>
	/// Simple I/O output for objects implementing the ILoadSave interafce
	/// </summary>
	public class CompositeIO<T> where T: ILoadSave, new()
    {		
		/// <summary>
		/// Reads "numitems" vectors from rfile, store items in "output" (array or list)
		/// </summary>
		public static IList<T> LoadVector (BinaryReader Input, int numitems, IList<T> output = null)
		{
			if (output == null) {
				output = new T[numitems];
			}            
            if (output.Count > 0) {
                for (int i = 0; i < output.Count; i++) {
                    //var u = default(T);
                    var u = new T();
                    u.Load(Input);
                    output [i] = u;
                }
                numitems -= output.Count;
            }
            for (int i = 0; i < numitems; i++) {
                //var u = default(T);
                var u = new T();
                u.Load(Input);
                output.Add (u);
            }
			return output;
		}

		/// <summary>
		/// Write a single vector
		/// </summary>
		public static void SaveVector (BinaryWriter Output, IEnumerable<T> V)
		{
			foreach (T u in V) {
                u.Save(Output);
			}
		}

        public static T Load(BinaryReader Input)
        {
            var new_item = new T();
            new_item.Load(Input);
            return new_item;
        }
	}
}

