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

namespace natix
{
	/// <summary>
	/// Simple I/O output for arrays of items of native types
	/// </summary>
	public class PrimitiveIO<T>
	{	
		static INumeric<T> Num = (INumeric<T>)Numeric.Get (typeof(T));
		
		/// <summary>
		/// Reads "numitems" vectors from rfile, store items in "output" (array or list)
		/// </summary>
		public static void ReadFromFile (BinaryReader rfile, int numitems, IList<T> output)
		{
			if (output.Count > 0) {
				for (int i = 0; i < output.Count; i++) {
					output [i] = Num.ReadBinary (rfile);
				}
				numitems -= output.Count;
			}
			for (int i = 0; i < numitems; i++) {
				output.Add (Num.ReadBinary (rfile));
			}
		}
		
		/// <summary>
		/// Read a large list of items from filename
		/// </summary>
		public static IList<T> ReadAllFromFile (string filename)
		{
			var infile = new BinaryReader (File.OpenRead (filename));
			int size = (int)infile.BaseStream.Length / Num.SizeOf ();
			var list = new List<T> (size);
			int pc = size / 100 + 1;
			int ic = 0;
			Console.WriteLine("===> Reading vectors from {0}", filename);
			for (int i = 0; i < size; i++) {
				if ((i % pc) == 0) {
					// Console.WriteLine("== Advance : {0:0.0}%, i: {1}", i * 1.0 / size * 100, i);
					Console.Write ("{0:0.0}%, i: {1}, ", i * 1.0 / size * 100, i);
					ic++;
					if (ic % 5 == 0) {
						Console.WriteLine();
					} 
				}
				list.Add (Num.ReadBinary (infile));
			}
			Console.WriteLine("===> Done");
			infile.Close ();
			return list;
		}

		/*
		public static IList<T> ReadVectorFromFile (BinaryReader infile, int size)
		{
			var list = new List<T> (size);
			for (int i = 0; i < size; i++) {
				list.Add (Num.ReadBinary (infile));
			}
			return list;
		}*/

		static char[] Sep = new char[2] { ' ', ',' };
		/// <summary>
		/// Load a single vector from a string, saving in a given vector. Each vector is a list of numbers separated by space or comma.
		/// </summary>
		public static IList<T> ReadVectorFromString (string line, IList<T> v, int dim)
		{
			line = line.Trim ();
			string[] vecs = line.Split (Sep);
			for (int j = 0; j < dim; j++) {
				v [j] = Num.FromDouble (Double.Parse (vecs [j]));
			}
			return v;
		}
		
		/// <summary>
		/// Reads a vector from a string
		/// </summary>
		public static IList<T> ReadVectorFromString (string line)
		{
			line = line.Trim ();
			if (line.Length == 0) {
				return new T[0];
			}
			string[] vecs = line.Split (Sep);
			var v = new T[vecs.Length];
			for (int j = 0; j < vecs.Length; j++) {
				v[j] = Num.FromDouble (Double.Parse (vecs[j]));
			}
			return v;
		}

		/// <summary>
		/// Creates (if needed) a binary file parsing an ascii file. Returns the name of the binary file.
		/// </summary>
		public static string CreateBinaryFile (string name, int len, int dim)
		{
			bool isbinary = (name.EndsWith (".bin") || name.EndsWith (".data"));
			if (isbinary) {
				return name;
			} else {
				var binname = name + ".bin";
				if (!File.Exists (binname)) {
					Console.WriteLine ("** Creating binary mirror: {0}", binname);
					StreamReader r = new StreamReader (File.OpenRead (name));
					BinaryWriter w = new BinaryWriter (File.Create (binname));
					T[] V = new T[dim];
					for (int i = 0; i < len; i++) {
						ReadVectorFromString (r.ReadLine (), V, dim);
						WriteVector (w, V);
					}
					w.Close ();
					r.Close ();
				}
				return binname;
			}
		}
		
		/// <summary>
		/// Creates a binary representation of a string of numbers
		/// </summary>
		public static string CreateBinaryFile (string name)
		{
			bool isbinary = (name.EndsWith (".bin") || name.EndsWith (".data"));
			if (isbinary) {
				return name;
			} else {
				var binname = name + ".bin";
				if (!File.Exists (binname)) {
					Console.WriteLine ("** Creating binary mirror: {0}", binname);
					StreamReader r = new StreamReader (File.OpenRead (name));
					BinaryWriter w = new BinaryWriter (File.Create (binname, 1 << 20));
					BinaryWriter wsizes = new BinaryWriter (File.Create (binname + ".sizes", 1 << 20));
					int lineno = 0;
					while (!r.EndOfStream) {
						lineno++;
						try {
							var line = r.ReadLine ();
							var V = ReadVectorFromString (line);
							WriteVector (w, V);
							wsizes.Write ((int)V.Count);
						} catch (FormatException exc) {
							Console.WriteLine ("XXXXX Filename: {0}, binfile: {1}, Line number: {2}",
								name, binname, lineno);
							throw exc;
						}
					}
					r.Close ();
					w.Close ();
					wsizes.Close ();
				}
				return binname;
			}
		}

		/// <summary>
		/// Write a single vector
		/// </summary>
		public static void WriteVector (BinaryWriter w, IEnumerable<T> V)
		{
			foreach (T u in V) {
				Num.WriteBinary (w, u);
			}
		}
	}
}

