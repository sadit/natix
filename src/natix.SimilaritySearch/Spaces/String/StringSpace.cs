//
//   Copyright 2015 Eric Sadit Tellez <eric.tellez@infotec.com.mx>
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
//   Original filename: natix/SimilaritySearch/Spaces/SequenceSpace.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// String space
	/// </summary>
	public class StringSpace : MetricDB 
	{
		public List<string> seqs = new List<string> ();
		protected long numdist;

		public void Add(string u)
		{
			this.seqs.Add (u);
		}

		public virtual void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString();
			var len = Input.ReadInt32 ();
			this.seqs.Clear ();
			this.seqs.Capacity = len;
			for (int i = 0; i < len; ++i) {
				var v = Input.ReadString ();
				this.seqs.Add (v);
			}
		}

		public virtual void Save (BinaryWriter Output)
		{
			Output.Write (this.Name);
			Output.Write ((int)this.seqs.Count);
			for (int i = 0; i < this.seqs.Count; ++i) {
				Output.Write(this.seqs[i]);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public StringSpace ()
		{
		}

		/// <summary>
		/// Retrieves the object associated to object id docid
		/// </summary>
		public object this[int docid]
		{
			get { return this.seqs[docid]; }
		}

		/// <summary>
		/// Get/Set (and load) the database
		/// </summary>
		public string Name {
			get;
			set;

		}

		public void Build (string outname, IList<string> seqs)
		{
			this.Name = outname;
			this.seqs = new List<string> (seqs.Count);
			foreach (var s in seqs) {
				this.seqs.Add (s);
			}
		}

		public virtual void Build (string seqlist)
		{
			Console.WriteLine ("Reading strings from '{0}'", seqlist);
			this.seqs = new List<string> ();
			this.Name = seqlist;
			using (var stream = File.OpenText(seqlist)) {
				while (!stream.EndOfStream) {
					var line = stream.ReadLine ();
					this.seqs.Add (line);
				}
			}
		}

		/// <summary>
		/// Parse an string into the object representation
		/// </summary>
		public object Parse (string s)
		{
			return s;
		}
		
		/// <summary>
		/// Accumulated number of distances
		/// </summary>
		public long NumberDistances {
			get { return this.numdist; }
		}
		
		/// <summary>
		/// The length of the space
		/// </summary>
		public int Count {
			get { return this.seqs.Count; }
		}
		
		/// <summary>
		/// Wrapper to the real string distance
		/// </summary>
		public virtual double Dist (object a, object  b)
		{
			throw new NotSupportedException();
		}

		private static int minimum3 (int a, int b, int c)
		{
			if (a > b) {
				a = b;
			}
			if (a > c) {
				a = c;
			}
			return a;
		}
		
		/// <summary>
		/// Levenshtein distance for generic datatype. It has customizable costs
		/// </summary>
		public static int Levenshtein (string a, string b, byte inscost, byte delcost, byte repcost) 
		{
			int alength = a.Length;
			int blength = b.Length;
			if (alength <= 0) {
				return blength;
			}
			if (blength <= 0) {
				return alength;
			}
			blength++;
			int[] C = new int[blength];
			int A_ant = 0;
			for (int i = 0; i < blength; i++) {
				C[i] = i;
			}
			blength--;
			for (int i = 0; i < alength; i++) {
				A_ant = i + 1;
				int C_ant = C[0];
				int j = 0;
				for (j = 0; j < blength; j++) {
					int cost = repcost;
					if (a[i] == b[j]) {
						cost = 0;
					}
					// adjusting the indices
					j++;
					C[j-1] = A_ant;
					A_ant = minimum3 (C[j] + delcost, A_ant + inscost, C_ant + cost);
					C_ant = C[j];
					// return to default values. Only to be clear
					j--;
					
				}
				C[j] = A_ant;
			}
			return A_ant;
		}
		/// <summary>
		/// Edit distance
		/// </summary>
		public static double Levenshtein (string a, string b)
		{
			return Levenshtein (a, b, 1, 1, 1);
		}
		/// <summary>
		/// LCS over Levenshtein
		/// </summary>
		public static double LCS (string a, string b)
		{
			return Levenshtein (a, b, 1, 1, 2);
		}
		/// <summary>
		/// Hamming distance for Generic Datatype
		/// </summary>
		public static double Hamming (string a, string b)
		{
			int d = 0;
			for (int i = 0; i < a.Length; i++) {
				if (a[i] != b[i]) {
					d++;
				}
			}
			return d;
		}
		/// <summary>
		/// lexicographic comparison, starting always at position 0 of every sequence
		/// </summary>
		public static int LexicographicCompare (string a, string b)
		{
			return LexicographicCompare (a, 0, b.Length, b, 0, a.Length);
		}
		/// <summary>
		/// Compare to arrays lexicographically, returns an integer representing something like a - b
		/// </summary>
		public static int LexicographicCompare (string a, int aStart, int aEnd, string b, int bStart, int bEnd)
		{
			int cmp = 0;
			for (int i = aStart, j = bStart; i < aEnd && j < bEnd; i++,j++) {
				cmp = a[i].CompareTo (b[j]);
				if (cmp != 0) {
					return cmp;
				}
			}
			return (aEnd-aStart) - (bEnd-bStart);
		}
		/// <summary>
		/// Jaccard's distance
		/// </summary>
		public static double Jaccard (string a, string b)
		{
			// a & b are already sorted
			// union
			int U = a.Length + b.Length;
			// intersection
			int I = 0;
			int cmp;
			for (int ia = 0, ib = 0; ia < a.Length && ib < b.Length;) {
				cmp = a[ia].CompareTo (b[ib]);
				if (cmp == 0) {
					U--;
					I++;
					ia++;
					ib++;
				} else if (cmp < 0) {
					ia++;
				} else {
					ib++;
				}
			}
			// Console.WriteLine ("I {0}, U {1}", I, U);
			return 1.0 - ((double)I) / U;
		}
		
		/// <summary>
		/// Hamming distance
		/// </summary>
		public static double Dice (string a, string b)
		{
			// a & b are already sorted
			// union
			// intersection
			int I = 0;
			int cmp;
			for (int ia = 0, ib = 0; ia < a.Length && ib < b.Length;) {
				cmp = a[ia].CompareTo (b[ib]);
				if (cmp == 0) {
					I++;
					ia++;
					ib++;
				} else if (cmp < 0) {
					ia++;
				} else {
					ib++;
				}
			}
			return -(I * 2.0) / (a.Length + b.Length);
		}
		/// <summary>
		/// Knr Intersection distance
		/// </summary>
		public static double Intersection (string a, string b)
		{
			// a & b are already sorted
			// union
			// intersection
			int I = 0;
			int cmp;
			for (int ia = 0, ib = 0; ia < a.Length && ib < b.Length;) {
				cmp = a[ia].CompareTo (b[ib]);
				if (cmp == 0) {
					I++;
					ia++;
					ib++;
				} else if (cmp < 0) {
					ia++;
				} else {
					ib++;
				}
			}
			return -I;
		}
		
		/// <summary>
		/// Knr prefix length distance
		/// </summary>
		public static double PrefixLength (string a, string b)
		{
			int i, min = Math.Min (a.Length, b.Length);
			for (i = 0; i < min && a[i] == b[i]; i++) {
				//empty
			}
			return -i;
		}
	}
}
