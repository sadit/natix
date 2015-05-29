//
//   Copyright 2012-2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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
//   Original filename: natix/SimilaritySearch/Spaces/VectorDB.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Vector space
	/// </summary>
	public class MemVectorDB<T> : MetricDB where T: struct
	{
		public static INumeric<T> Num = (INumeric<T>)(natix.Numeric.Get (typeof(T)));
		public static void SetNumeric(object num)
		{
			Num = (INumeric<T>)num;
		}
		/// <summary>
		/// The underlying storage for vectors 
		/// </summary>
		public List<T[]> VECTORS;
		/// <summary>
		/// Dimension of the space
		/// </summary>
		public int Dimension;
		/// <summary>
		/// P norm
		/// </summary>
		public float P;
		/// <summary>
		/// Number of distances
		/// </summary>
		protected long numdist;

		public virtual void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			this.Dimension = Input.ReadInt32 ();
			this.P = Input.ReadSingle();
			int n = Input.ReadInt32 ();
			this.LoadVectors (Input, n);
		}

		public void LoadVectors(BinaryReader Input, int n)
		{
			this.VECTORS = new List<T[]> (n);
			int pc = 1 + n / 100;
			for (int docID = 0; docID < n; ++docID) {
				if (docID % pc == 0) {
					Console.WriteLine("== loading vectors: {0}/{1}, {2:0.00}% -- {3}", docID, n, docID * 100.0 / n, DateTime.Now);
				}
				var vec = new T[this.Dimension];
				PrimitiveIO<T>.LoadVector (Input, vec.Length, vec);
				this.VECTORS.Add (vec);
			}
		}

		public virtual void Save(BinaryWriter Output)
		{
			Output.Write(this.Name);
			Output.Write((int) this.Dimension);
			Output.Write ((float) this.P);
			Output.Write (this.VECTORS.Count);
			foreach (var vec in this.VECTORS) {
				PrimitiveIO<T>.SaveVector (Output, vec);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public MemVectorDB () : base()
		{
		}

		/// <summary>
		/// Get/Set (and load) the database name
		/// </summary>
		public string Name {
			get;
			set;
		}

		/// <summary>
		///  The accumulated number of distances in the space
		/// </summary>
		public long NumberDistances {
			get {
				return this.numdist;
			}
		}

		/// <summary>
		/// Length of the space
		/// </summary>
		public int Count {
			get {
				return this.VECTORS.Count;
		    }
		}

		public int Add(object a)
		{
			var objID = this.VECTORS.Count;
			var v = a as T[];
			if (v == null) {
				throw new ArgumentException ("Object is not a T[] instance");
			}
			this.VECTORS.Add (v);
			return objID;
		}
		/// <summary>
		/// Retrieves the object with identifier docid
		/// </summary>
		public object this[int docid]
		{
			get { return this.VECTORS[docid]; }
		}

		public void Build (string name, IList<T[]> _VECTORS, float _P = -1)
		{
			this.Name = name;
			this.P = _P;
			this.VECTORS = new List<T[]> (_VECTORS);
			this.Dimension = _VECTORS [0].Length;
		}
			
		public void Build (string name, IList<IList<T>> _VECTORS, float _P = -1)
		{
			this.Name = name;
			this.P = _P;
			this.VECTORS = new List<T[]> (_VECTORS.Count);
			foreach (var list in _VECTORS) {
				var new_list = new T[list.Count];
				list.CopyTo(new_list, 0);
				this.VECTORS.Add (new_list);
			}
			this.Dimension = _VECTORS [0].Count;
		}

		public void Build (string inputname)
		{
			this.Name = inputname;
			// dim size pnorm / Example: 112 112682 2
			string desc = File.ReadAllLines(inputname)[0];
			string[] fields = desc.Split (' ');
			// read from this.name file
			this.Dimension = int.Parse (fields [0]);
			int len = int.Parse (fields[1]);
			this.P = float.Parse (fields[2]);
			this.VECTORS = new List<T[]>(len);
			//var dbvecs = Dirty.CombineRelativePath(inputname, m [3]);


			Console.WriteLine ("** Reading vectors from file {0}.vecs", inputname);
			using (var Input = new StreamReader(File.OpenRead(inputname + ".vecs"))) {
				for (int i = 0; i < len; ++i) {
					if (i % 100000 == 0) {
						Console.WriteLine ("*\t\t* reading {0} of {1} (adv. {2:0.00}%), timestamp: {3}", i + 1, len, i * 100.0 / len, DateTime.Now);
					}
					var line = Input.ReadLine ();
					var vec = this.Parse (line) as T[];
					this.VECTORS.Add(vec);
				}
			}
		}

		/// <summary>
		/// Returns a vector from an string
		/// </summary>
		public object Parse (string s)
		{
			return PrimitiveIO<T>.LoadVector (s);
		}

		/// <summary>
		/// Distance wrapper for any P-norm
		/// </summary>
		public virtual double Dist (object a, object b)
		{
			++this.numdist;
			if (this.P == 1) {
				return Num.DistL1 ((T[])a, (T[])b);
			} else if (this.P == 2) {
				return Num.DistL2 ((T[])a, (T[])b);
			} else if (this.P <= 0) {
				return Num.DistLInf ((T[])a, (T[])b);
			} else {
				return Num.DistLP ((T[])a, (T[])b, this.P, true);
			}
		}

	}
}
