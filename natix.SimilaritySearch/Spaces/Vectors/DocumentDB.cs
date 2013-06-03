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
//   Original filename: natix/SimilaritySearch/Spaces/DocumentSpace.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NDesk.Options;
using natix.SortingSearching;
// Adapted from SISAP library src/spaces/documents/objdocuments.h to Natix library.

namespace natix.SimilaritySearch
{

	/// <summary>
	/// The document's space
	/// </summary>
	public class DocumentDB : MetricDB
	{
		/// <summary>
		/// Represents a vocabulary entry
		/// </summary>
		public struct Tvoc
		{
			/// <summary>
			/// Key identifier
			/// </summary>
			public Int32 keyid;
			/// <summary>
			/// Weight for this key
			/// </summary>
			public float weight;
			/// <summary>
			///  Constructor
			/// </summary>
			/// <param name="k">
			/// A <see cref="UInt32"/>
			/// </param>
			/// <param name="w">
			/// A <see cref="System.Single"/>
			/// </param>
			public Tvoc (Int32 k, float w)
			{
				this.keyid = k;
				this.weight = w;
			}
		}
		
		/// <summary>
		///  A document vector representation
		/// </summary>
		public class Tdoc
		{
			/// <summary>
			/// The document vector
			/// </summary>
			public Tvoc[] doc;
			/// <summary>
			/// Constructor
			/// </summary>
			public Tdoc (Tvoc[] d)
			{
				this.doc = d;
			}
			
			/// <summary>
			/// Builds the Tdoc from two vectors
			/// </summary>
			public Tdoc (IList<Int32> keywords, IList<Single> weigths)
			{
				var v = new Tvoc [keywords.Count];
				for (int i = 0; i < keywords.Count; i++) {
					var keyid = keywords[i];
					float d = weigths[i];
					v[i] = new Tvoc (keyid, d);
				}
				this.doc = v;
			}
			
			/// <summary>
			/// Save this document's vector
			/// </summary>
			public void Save (BinaryWriter b)
			{
				b.Write ((int)this.doc.Length);
				foreach (Tvoc v in this.doc) {
					b.Write (v.keyid);
					b.Write (v.weight);
				}
			}
			
			/// <summary>
			/// Load a document's vector from stream
			/// </summary>
			public static Tdoc Load (BinaryReader b)
			{
				int len = b.ReadInt32 ();
				// Console.WriteLine ("***** BIG LOAD? {0}", len);
				Tdoc res = new Tdoc (new Tvoc[len]);
				for (int i = 0; i < len; i++) {
					res.doc[i] = new Tvoc (b.ReadInt32 (), b.ReadSingle ());
				}
				return res;
			}
		}

		public List<Tdoc> DOCS;
		int numdist;

		/// <summary>
		/// Constructor
		/// </summary>
		public DocumentDB ()
		{
			this.DOCS = new List<Tdoc> ();
			this.numdist = 0;
		}

		public void Build (string name, List<Tdoc> docs)
		{
			this.Name = name;
			this.DOCS = docs;
		}

		public void Build (string name, IList<string> filenames)
		{
			this.Name = name;
			int pc = 1 + filenames.Count / 100;
			for (int i = 0; i < filenames.Count; i++) {
				var fname = filenames[i];
				if ((i % pc) == 0) {
					Console.WriteLine ("Loading document: {0}, name: {1}, advance: {2:0.00}%", i, filenames [i], i * 100.0 / filenames.Count);
				}
				var docvec = this.ParseFromFile (fname, false);
				this.DOCS.Add (docvec);
			}
		}

		public void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			var len = Input.ReadInt32 ();
			this.DOCS = new List<Tdoc>(len);
			for (int i = 0; i < len; ++i) {
				this.DOCS.Add(Tdoc.Load(Input));
			}
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.Name);
			Output.Write ((int)this.DOCS.Count);
			for (int i = 0; i < this.DOCS.Count; ++i) {
				this.DOCS[i].Save(Output);
			}
		}

		/// <summary>
		/// The length of the space
		/// </summary>
		public int Count {
			get { return this.DOCS.Count; }
		}
		
		/// <summary>
		/// Indexer to retrieve an object by id
		/// </summary>
		public object this[int docid]
		{
			get { return this.DOCS[docid]; }
		}
		
		/// <summary>
		/// Get/Set database name
		/// </summary>
		/// <remarks>The set operation loads the database</remarks>
		public string Name {
			get; set;
		}
		
		public IResult CreateResult (int K, bool ceiling)
		{
			return new Result (K, ceiling);
		}

		/// <summary>
		/// Returns the number of accumulated distances
		/// </summary>
		public int NumberDistances {
			get { return this.numdist; }
		}
		
		/// <summary>
		/// Parse an string into the document's vector
		/// </summary>
		public object Parse (string s, bool isquery)
		{
			return this.ParseFromFile (s, isquery);
		}
		
		/// <summary>
		/// Load vectors from ascii string
		/// </summary>
		public Tdoc ParseFromString (string data, bool isquery)
		{
			List<Tvoc> v = new List<Tvoc> ();
			var matches = Regex.Matches (data, @"[\d\.eE\-\+]+");
			var mcount = matches.Count;
			for (int i = 0; i < mcount; i += 2) {
				var k = int.Parse(matches[i].Value);
				var d = float.Parse(matches[i+1].Value);
				v.Add(new Tvoc(k, d));
			}
			return new Tdoc (v.ToArray ());
		}
		
		/// <summary>
		/// Parse document vector from file
		/// </summary>
		public Tdoc ParseFromFile (string name, bool isquery)
		{
			return this.ParseFromString (File.ReadAllText (name), isquery);
		}

		/// <summary>
		/// The distance function (angle between vectors)
		/// </summary>
		public double Dist (object _v1, object _v2)
		{
			this.numdist++;
			var v1 = (Tdoc) _v1;
			var v2 = (Tdoc) _v2;
			var w1 = v1.doc;
			int n1 = v1.doc.Length;
			var w2 = v2.doc;
			int n2 = v2.doc.Length;
			double sum, norm1, norm2;
			norm1 = norm2 = sum = 0.0;
			for (int i = 0; i < n1; i++) {
				norm1 += w1[i].weight * w1[i].weight;
			}
			for (int i = 0; i < n2; i++) {
				norm2 += w2[i].weight * w2[i].weight;
			}
			for (int i = 0,j = 0; (i < n1) && (j < n2);) {
				if (w1[i].keyid == w2[j].keyid) {
					// match
					sum += w1[i].weight * w2[j].weight;
					i++;
					j++;
				} else if (w1[i].keyid < w2[j].keyid) {
					i++;
				} else {
					j++;
				}
			}
			// free(w1); free(w2);
			// printf ("internal product: %f\n",sum/(sqrt(norm1)*sqrt(norm2)));
			// printf ("distance: %f\n",acos(sum/(sqrt(norm1)*sqrt(norm2))));
			double M = sum/(Math.Sqrt(norm1)*Math.Sqrt(norm2));
			//M=max(-1.0,min(1.0,M));
			M=Math.Min(1.0, M);
			return Math.Acos(M);
		}
	}
}
