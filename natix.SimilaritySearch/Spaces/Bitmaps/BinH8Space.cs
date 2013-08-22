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
//   Original filename: natix/SimilaritySearch/Spaces/BinH8Space.cs
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
	public class BinH8Space : MetricDB
	{
		public List<byte> DATA;
		public Bitmap LENS;
		int numdist;

		/// <summary>
		/// Constructor
		/// </summary>
		public BinH8Space ()
		{
		}

		public void Load(BinaryReader Input)
		{
			this.Name = Input.ReadString();
			var len = Input.ReadInt32 ();
			this.DATA = new List<byte> (len);
			PrimitiveIO<byte>.LoadVector(Input, len, this.DATA);
			this.LENS = GenericIO<Bitmap>.Load(Input);
		}

		public void Save(BinaryWriter Output)
		{
			Output.Write(this.Name);
			Output.Write((int) this.DATA.Count);
			PrimitiveIO<byte>.SaveVector(Output, this.DATA);
			GenericIO<Bitmap>.Save(Output, this.LENS);
		}

		/// <summary>
		/// Get/Set the database name.
		/// </summary>
		public string Name
		{
			get;
			set;
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
		public void Build (string filename, BitmapFromBitStream len_builder = null)
		{
			Console.WriteLine ("****** Reading database from list of files");
			this.Name = filename;
			var NAMES = File.ReadAllLines (filename);
			int counter = 0;
			var data_stream = new List<byte> ();
			var lens_stream = new BitStream32 ();
			foreach (var s in NAMES) {
				++counter;
				if (s.Length == 0) {
					continue;
				}
				if (counter % 1000 == 0) {
					Console.WriteLine ("*** Processing docid {0}/{1} (adv: {2:0.000}%): '{3}'",
					                   counter, NAMES.Length, counter*100.0/NAMES.Length, s);
				}
				var data = (IList<byte>)this.Parse (s, true);
				if (data.Count == 0) {
					throw new ArgumentException(String.Format("AFP files must not be empty: {0}", s));
				}
				lens_stream.Write (true);
				lens_stream.Write (false, data.Count-1);
				data_stream.Capacity += data.Count;
				foreach (var b in data) {
					data_stream.Add (b);
				}
			}
			lens_stream.Write(true);
			if (len_builder == null) {
				len_builder = BitmapBuilders.GetGGMN_wt (12);
			}
			this.LENS = len_builder (new FakeBitmap (lens_stream));
			this.DATA = data_stream;
		}

		public void Build (string out_filename, IList<IList<byte>> data_list, BitmapFromBitStream len_builder = null)
		{
			this.Name = out_filename;
			int counter = 0;
			var data_stream = new List<byte> ();
			var lens_stream = new BitStream32 ();
			foreach (var data in data_list) {
				++counter;
				if (counter % 1000 == 0) {
					Console.WriteLine ("*** Processing docid {0}/{1} (adv: {2:0.000}%)",
					                   counter, data_list.Count, counter*100.0/data_list.Count);
				}
				lens_stream.Write (true);
				lens_stream.Write (false, data.Count-1);
				// data_stream.Capacity += data.Count;
				foreach (var b in data) {
					data_stream.Add (b);
				}
			}
			lens_stream.Write(true);
			if (len_builder == null) {
				len_builder = BitmapBuilders.GetGGMN_wt (12);
			}
			this.LENS = len_builder (new FakeBitmap (lens_stream));
			this.DATA = data_stream;
		}


		/// <summary>
		/// Indexer to retrieve an object
		/// </summary>
		public object this[int docid]
		{
			get {
				var start_index = this.LENS.Select1(docid+1);
				var last_index = this.LENS.Select1(docid+2);
				var len = last_index - start_index;
				var s = new BinQGramList(this.DATA, start_index, len);
				return s;
			}
		}

		public static IList<byte> ParseFromString (string data)
		{
			return BinQ8HammingSpace.ParseObjectFromString(data);
		}
		
		public static IList<byte> ParseAndLoadFromFile (string name, bool save_binary_cache)
		{
			return BinQ8HammingSpace.LoadObjectFromFile(name, save_binary_cache);
		}
		
		/// <summary>
		/// Converts 'name' into an object
		/// </summary>
		public virtual object Parse (string name, bool isquery)
		{
			//Console.WriteLine ("Parsing '{0}', isquery: {1}", name, isquery);
			if (name.StartsWith ("obj")) {
				return this[int.Parse (name.Split (' ')[1])];
			}
			var res = ParseAndLoadFromFile (name, !isquery);
			return res;
		}

		/// <summary>
		/// The current length of the space
		/// </summary>
		public int Count {
			get {
				return this.LENS.Count1 - 1;
			}
		}
		/// <summary>
		///  The number of computed distances. This property is deprecated
		/// </summary>
		public int NumberDistances {
			get { return this.numdist; }
		}

		/// <summary>
		/// The distance function
		/// </summary>
		public virtual double Dist (object _a, object _b)
		{
			this.numdist++;
			IList<byte> a = (IList<byte>) _a;
			IList<byte> b = (IList<byte>) _b;
			if (a.Count < b.Count) {
				IList<byte> w = a;
				a = b;
				b = w;
			}
			int bL = b.Count;
			int d = 0;
			for (int i = 0; i < bL; ++i) {
				d += Bits.PopCount8[a[i] ^ b[i]];
			}
			return d;
		}

		public double DistMin (IList<byte> a, IList<byte> b)
		{
			this.numdist++;
			int min = int.MaxValue;
			if (a.Count < b.Count) {
				IList<byte> w = a;
				a = b;
				b = w;
			}
			int bL = b.Count;
			int aL = a.Count - bL;
			int d;
			//Console.WriteLine ("aL: {0} bL: {1}, symlen: {2}", aL, bL, this.symlen);
			for (int askip = 0; askip <= aL; askip ++) {
				d = 0;
				for (int bskip = 0, abskip = askip; bskip < bL; bskip++,abskip++) {
					// Console.WriteLine ("a:{0}, b:{1}, A: {2}, B: {3}", askip, bskip, a[askip], b[bskip]);
					d += Bits.PopCount8[a[abskip] ^ b[bskip]];
				}
				if (min > d) {
					min = d;
				}
			}
			return min;
		}
	}
}
