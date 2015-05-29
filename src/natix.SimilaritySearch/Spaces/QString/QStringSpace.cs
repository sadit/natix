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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.CompactDS;
using System.Text;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// String space
	/// </summary>
	public class QStringSpace : MetricDB
	{
		/// <summary>
		/// The length of each element
		/// </summary>
		public int Q;
		public IList<int> TEXT;
		public bool CopyQGramsOnAccess;
		public bool ParseIntegers;
		protected long numdist = 0;

		public virtual void Load (BinaryReader Input)
		{
            this.Name = Input.ReadString ();
			this.Q = Input.ReadInt32 ();
			this.CopyQGramsOnAccess = Input.ReadBoolean();
			this.ParseIntegers = Input.ReadBoolean();
			this.TEXT = ListIGenericIO.Load(Input);
		}

		public virtual void Save (BinaryWriter Output)
		{
            Output.Write (this.Name);
			Output.Write ((int) this.Q);
			Output.Write ((bool) this.CopyQGramsOnAccess);
			Output.Write ((bool) this.ParseIntegers);
			ListIGenericIO.Save(Output, this.TEXT);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public QStringSpace ()
		{
		}

		/// <summary>
		/// Retrieves the object associated to object id docid
		/// </summary>
		public object this[int docid]
		{
			get {
				return this.GetQGram(docid, this.Q);
			}
		}

		public IList<int> GetQGram (int docid, int qlen = 0)
		{
			if (qlen <= 0) {
				qlen = this.Q;
			}
			var slist = new ListShiftIndex<int>(this.TEXT, docid*this.Q, qlen);
			if (this.CopyQGramsOnAccess) {
				return new List<int>(slist);
			} else {
				return slist;
			}
		}

		/// <summary>
		/// Get/Set (and load) the database
		/// </summary>
		public string Name {
			get;
			set;
		}

		public void Build (string outname, IList<int> text, int sigma, int q, bool copy_on_access = true, bool parse_integers = false, ListIBuilder list_builder = null)
        {
            this.Name = outname;
            this.Q = q;
            this.CopyQGramsOnAccess = copy_on_access;
            this.ParseIntegers = parse_integers;
            if (list_builder == null) {
                list_builder = ListIBuilders.GetListIFS ();
            }
            var N = (int)(Math.Ceiling (text.Count * 1.0 / q)) * q;
            Console.WriteLine ("=== sigma: {0}, q: {1}, N: {2}", sigma, q, N);
            if (N == text.Count) {
                this.TEXT = list_builder (text, sigma);
            } else {
                this.TEXT = list_builder (new ListPaddingToN<int> (text, N, sigma), sigma);
            }
		}

		/// <summary>
		/// Parse an string into the object representation
		/// </summary>
		public object Parse (string s)
		{
			if (this.ParseIntegers) {
				return PrimitiveIO<int>.LoadVector (s);
			} else {
				var u = new int[s.Length];
				for (int i = 0; i < u.Length; ++i) {
					u[i] = Convert.ToInt32(s[i]);
				}
				return u;
			}
		}

		public int Add(object a)
		{
			throw new NotSupportedException ();
		}

        public string AsString (int docid)
        {
            return this.AsString(this.GetQGram(docid));
        }

        public string AsString (IList<int> L)
        {
            var s = new StringBuilder ();
            foreach (var u in L) {
                s.Append(Convert.ToChar(u));
            }
            return s.ToString();
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
			get {
				// this.TEXT.Count is padded to be multiplo of Q
				return this.TEXT.Count/this.Q;
			}
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
 		public static int Levenshtein (IList<int> a, IList<int> b, byte inscost, byte delcost, byte repcost) 
		{
			int alength = a.Count;
			int blength = b.Count;
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
					if (a[i].CompareTo(b[j]) == 0) {
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
		public static double Levenshtein (IList<int> a, IList<int> b)
		{
			return Levenshtein (a, b, 1, 1, 1);
		}
		/// <summary>
		/// LCS over Levenshtein
		/// </summary>
		public static double LCS (IList<int> a, IList<int> b)
		{
			return Levenshtein (a, b, 1, 1, 2);
		}
		/// <summary>
		/// Hamming distance for Generic Datatype
		/// </summary>
		public static double Hamming (IList<int> a, IList<int> b)
		{
			int d = 0;
			for (int i = 0; i < a.Count; i++) {
				if (a[i].CompareTo (b[i]) != 0) {
					d++;
				}
			}
			return d;
		}
		/// <summary>
		/// lexicographic comparison, starting always at position 0 of every sequence
		/// </summary>
		public static int LexicographicCompare (IList<int> a, IList<int> b)
		{
			return LexicographicCompare (a, 0, b.Count, b, 0, a.Count);
		}
		/// <summary>
		/// Compare to arrays lexicographically, returns an integer representing something like a - b
		/// </summary>
		public static int LexicographicCompare (IList<int> a, int aStart, int aEnd, IList<int> b, int bStart, int bEnd)
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
		public static double Jaccard (IList<int> a, IList<int> b)
		{
			// a & b are already sorted
			// union
			int U = a.Count + b.Count;
			// intersection
			int I = 0;
			int cmp;
			for (int ia = 0, ib = 0; ia < a.Count && ib < b.Count;) {
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
		public static double Dice (IList<int> a, IList<int> b)
		{
			// a & b are already sorted
			// union
			// intersection
			int I = 0;
			int cmp;
			for (int ia = 0, ib = 0; ia < a.Count && ib < b.Count;) {
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
			return -(I * 2.0) / (a.Count + b.Count);
		}
		/// <summary>
		/// Knr Intersection distance
		/// </summary>
		public static double Intersection (IList<int> a, IList<int> b)
		{
			// a & b are already sorted
			// union
			// intersection
			int I = 0;
			int cmp;
			for (int ia = 0, ib = 0; ia < a.Count && ib < b.Count;) {
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
		public static double PrefixLength (IList<int> a, IList<int> b)
		{
			int i, min = Math.Min (a.Count, b.Count);
			for (i = 0; i < min && a[i] == b[i]; i++) {
				//empty
			}
			return -i;
		}
	}
}
