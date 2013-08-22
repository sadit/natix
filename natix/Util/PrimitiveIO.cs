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
//   Original filename: natix/Util/PrimitiveIO.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace natix
{
	/// <summary>
	/// Simple I/O output for arrays of items of native types
	/// </summary>
	public class PrimitiveIO<T>  where T: struct
	{
		public static INumeric<T> NUMERIC = (INumeric<T>)Numeric.Get (typeof(T));
		
		/// <summary>
		/// Reads "numitems" vectors from rfile, store items in "output" (array or list)
		/// </summary>
		public static IList<T> LoadVector (BinaryReader Input, int numitems, IList<T> output = null)
		{
			T[] vec_output;
			if (output == null) {
				vec_output = new T[numitems];
				NUMERIC.LoadVector (Input, vec_output, 0, numitems);
				return vec_output;
			}
			vec_output = output as T[];
			if (vec_output != null) {
				NUMERIC.LoadVector (Input, vec_output, 0, numitems);
				return vec_output;
			}
			if (output.Count > 0) {
				for (int i = 0; i < output.Count; i++) {
					output [i] = NUMERIC.Load (Input);
				}
				numitems -= output.Count;
			}
			for (int i = 0; i < numitems; i++) {
				output.Add (NUMERIC.Load (Input));
			}
			return output;
		}

		/// <summary>
		/// Reads "numitems" vectors from rfile. Appends items to "output"
		/// </summary>
		public static void LoadVector (BinaryReader Input, T[] output, int sp, int count)
		{
			NUMERIC.LoadVector (Input, output, sp, count);
		}

		static string[] split_vector(string line)
		{
			// return line.Split (CharSeparators);
			var matches = Regex.Matches (line, @"\S+");
			//Console.WriteLine ("XXXXXXX>" + line);
			var s = new string[matches.Count];
			for (int i = 0; i < s.Length; ++i) {
				s[i] = matches[i].Value;
				// Console.WriteLine ("YY:" + s[i]);
			}
			return s;
		}

		/// <summary>
		/// Reads "numitems" vectors from String. Appends items to "output"
		/// </summary>
		public static void LoadVector (string line, T[] output, int sp, int count)
		{
			line = line.Trim ();
			string[] vecs = split_vector (line);
			for (int i = 0; i < vecs.Length; i++) {
				output[sp + i] = NUMERIC.FromDouble (Double.Parse (vecs [i]));
			}
		}

		/// <summary>
		/// Reads a vector from a string. The vector is stored (appended) to "output". It returns the number of 
		/// loaded scalars
		/// </summary>
		public static int LoadVector (string line, List<T> output)
		{
			line = line.Trim ();
			if (line.Length == 0) {
				return 0;
			}
			string[] vecs = split_vector (line);
			for (int i = 0; i < vecs.Length; i++) {
				var d = NUMERIC.FromDouble (Double.Parse (vecs[i]));
				output.Add( d );
			}
			return vecs.Length;
		}

		/// <summary>
		/// Reads a vector from a string.
		/// </summary>
		public static T[] LoadVector (string line)
		{
			line = line.Trim ();
			if (line.Length == 0) {
				return new T[0];
			}
			string[] vecs = split_vector (line);
			var output = new T[vecs.Length];
			for (int i = 0; i < vecs.Length; i++) {
				var d = NUMERIC.FromDouble (Double.Parse (vecs[i]));
				output[i] = d;
			}
			return output;
		}


		/// <summary>
		/// Write a single vector
		/// </summary>
		public static void SaveVector (BinaryWriter w, IEnumerable<T> V)
		{
			var vec = V as T[];
			if (vec == null) {
				foreach (T u in V) {
					NUMERIC.Save (w, u);
				}
			} else {
				NUMERIC.SaveVector (w, vec, 0, vec.Length);
			}
		}
	}
}

