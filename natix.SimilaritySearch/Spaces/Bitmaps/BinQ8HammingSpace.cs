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
//   Original filename: natix/SimilaritySearch/Spaces/BinaryHammingSpace.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Hamming space for bit strings
	/// </summary>
	public class BinQ8HammingSpace : MetricDB
	{
		static string[] ascii_nibbles = new string[] { 
			"0000", "0001", "0010", "0011", 
			"0100", "0101", "0110", "0111", 
			"1000", "1001", "1010", "1011",	
			"1100", "1101", "1110", "1111"};
		public string Name {
			get;
			set;
		}

		List< byte[] > pool;
		protected int numdist;
		/// <summary>
		///  Symbol's length in bytes
		/// </summary>
		/// <remarks>
		/// The length in bytes of each symbol. For general data this should be 1, for audio MBSES this should be 3. 
		/// </remarks>
		public int symlen;

		public void Load (BinaryReader Input)
		{
		/*	this.ReadFromList(value);
				} else {
					this.ReadFromBinaryFile(value);
				}*/

			this.Name = Input.ReadString ();
			this.symlen = Input.ReadInt32 ();
			int len = Input.ReadInt32 ();
			this.pool.Capacity = len;
			for (int i = 0; i < len; ++i) {
				len = Input.ReadInt32 ();
				var list = new byte[len];
				PrimitiveIO<byte>.ReadFromFile(Input, len, list);
				this.pool.Add (list);
			}
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.Name);
			Output.Write ((int)this.symlen);
			Output.Write ((int)this.pool.Count);
			for (int i = 0; i < this.pool.Count; ++i) {
				Output.Write ((int)this.pool [i].Length);
				PrimitiveIO<byte>.WriteVector (Output, this.pool [i]);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public BinQ8HammingSpace (int symlen)
		{
			this.symlen = symlen;
			this.numdist = 0;
			this.pool = new List<byte[]> ();
		}

		public BinQ8HammingSpace() : this(1)
		{
		}
	
		public IResult CreateResult (int K, bool ceiling)
		{
			//if (this.fixed_len > 0 && this.fixed_dim < 256) {
			//	return new ResultTies (K, ceiling);
			//} else {
			return new Result(K, ceiling);
			//}
		}

		/// <summary>
		/// Read the database from a listing file (one filename per line)
		/// </summary>
		public void Build (string filename)
		{
			Console.WriteLine ("****** Reading database from list of files");
			StreamReader r = new StreamReader (filename);
			while (!r.EndOfStream) {
				string s = r.ReadLine ().Trim ();
				if (s.Length == 0) {
					continue;
				}
				this.pool.Add ((byte[])this.Parse (s, false));
			}
			Console.WriteLine ("done reading");
			r.Close ();
		}

		public int Add(byte[] bitmap)
		{
			this.pool.Add (bitmap);
			return this.pool.Count - 1;
		}

		/// <summary>
		/// Indexer to retrieve an object
		/// </summary>
		public object this[int docid]
		{
			get { return this.pool[docid]; }
		}

		/// <summary>
		/// Returns a string representation of a single byte
		/// </summary>
		public static string ToAsciiString (byte b)
		{
			return ascii_nibbles[(b & 0xF0) >> 4] + ascii_nibbles[b & 0x0F];
		}
		/// <summary>
		/// Returns the string representation of an UInt16
		/// </summary>
		/// <param name="b">
		/// A <see cref="UInt16"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string ToAsciiString (UInt16 b)
		{
			return ToAsciiString((byte)(b >> 8)) + ToAsciiString((byte)(b & 0xFF));
		}
		/// <summary>
		/// Returns the string representation of an UInt32
		/// </summary>
		/// <param name="b">
		/// A <see cref="UInt32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string ToAsciiString (UInt32 b)
		{
			return ToAsciiString((UInt16)(b >> 16)) + ToAsciiString((UInt16)(b & 0xFFFF));
		}

		/// <summary>
		/// Converts an UInt64 to binary (ascii format)
		/// </summary>
		public static string ToAsciiString (UInt64 b)
		{
			return ToAsciiString((UInt32)(b >> 32)) + ToAsciiString((UInt32)(b & 0xFFFFFFFF));
		}
		/// <summary>
		/// Converts an object to a readeable representation in ascii '0' and '1'
		/// </summary>
		public static string ToAsciiString (IList<byte> b)
		{
			StringWriter s = new StringWriter ();
			for (int i = 0; i < b.Count; i++) {
				s.Write (ToAsciiString (b[i]));
			}
			string _s = s.ToString ();
			s.Close ();
			return _s;
		}

		public static string ToAsciiString (int d)
		{
			return BinQ8HammingSpace.ToAsciiString((uint)d);
		}

		/// <summary>
		/// Converts an object to Ascii '0' and '1' using an object id
		/// </summary>
		/// <param name="docid">
		/// The object identifier
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// The '0' and '1' string
		/// A <see cref="System.String"/>
		/// </returns>
		public string ObjectToAsciiString (int docid)
		{
			return BinQ8HammingSpace.ToAsciiString (this.pool[docid]);
		}

		public static byte[] ParseObjectFromString (string data)
		{
			List<byte > res = new List<byte> ();
			int ishift = 0;
			int buffer = 0;
			foreach (char r in data) {
				switch (r) {
				case '1':
					buffer |= 1 << ishift;
					break;
				case '0':
					break;
				default:
					continue;
				}
				if (ishift == 7) {
					ishift = 0;
					res.Add ((byte)buffer);
					buffer = 0;
				} else {
					ishift++;
				}
			}
			// when the bit-string is not padded to 8 bits (e.g. AudioTimeDomainSpace)
			// after this, the bit-string will be padded to 8 bits
			if (ishift != 0) { 
				res.Add((byte)buffer);
			}
			return res.ToArray();
		}
		
		public static byte[] LoadObjectFromFile (string name, bool save_binary_cache)
		{
			if (name.EndsWith (".bin")) {
				return File.ReadAllBytes (name);
			}
			string bin = name + ".bin";
			if (File.Exists (bin)) {
				// Console.WriteLine ("Loading binary version {0}.bin", name);
				return File.ReadAllBytes (bin);
			}
			// Console.WriteLine ("Loading audio fingerprint {0}", name);
			var res = ParseObjectFromString (File.ReadAllText (name));
			if (save_binary_cache) {
				// Console.WriteLine ("Writing binary version {0}.bin of {1} bytes", name, res.Count);
				using (var binfile = new BinaryWriter(File.Create(bin))) {
					PrimitiveIO<byte>.WriteVector (binfile, res);
				}
			}
			return res;
		}
		
		/// <summary>
		/// Converts 'name' into an object
		/// </summary>
		public object Parse (string name, bool isquery)
		{
			//Console.WriteLine ("Parsing '{0}', isquery: {1}", name, isquery);
			if (name.StartsWith ("obj")) {
				return this[int.Parse (name.Split (' ')[1])];
			}
			var res = LoadObjectFromFile (name, !isquery);
			return res;
		}
		/// <summary>
		/// The current length of the space
		/// </summary>
		public int Count {
			get { return this.pool.Count; }
		}
		/// <summary>
		///  The number of computed distances. This property is deprecated
		/// </summary>
		public int NumberDistances {
			get { return this.numdist; }
		}
		/// <summary>
		/// The name of the space type. Used to save and load spaces
		/// </summary>
		public string SpaceType {
			get { return this.GetType ().FullName; }
			set { }
		}
		/// <summary>
		/// Wrap the distance to the given BinDist distance.
		/// </summary>
		public virtual double Dist (object a, object b)
		{
			this.numdist++;
			return DistHamming((byte[])a, (byte[])b);
		}

		public static double DistHamming (byte[] a, byte[] b)
		{
			int d = 0;
			for (int i = 0; i < a.Length; ++i) {
				d += Bits.PopCount8[a[i] ^ b[i]];
			}
			return d;
		}
	}
}
