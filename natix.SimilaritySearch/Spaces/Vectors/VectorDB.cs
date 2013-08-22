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
//   Original filename: natix/SimilaritySearch/Spaces/VectorDB.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// Vector space
	/// </summary>
	public class VectorDB<T> : MetricDB where T: struct
	{
		public static bool LOAD_IN_MEMORY = true;
		public static INumeric<T> Num = (INumeric<T>)(natix.Numeric.Get (typeof(T)));
		public static void SetNumeric(object num)
		{
			Num = (INumeric<T>)num;
		}
		/// <summary>
		/// The underlying storage for vectors 
		/// </summary>
		public IList< T[] > VECTORS;
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
		protected int numdist;
		/// <summary>
		/// The file containing binary vectors
		/// </summary>
		public string VectorFilename;

		public virtual void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			this.VectorFilename = Input.ReadString ();
			this.Dimension = Input.ReadInt32 ();
			this.P = Input.ReadSingle();
			this.VECTORS = new DiskVectorList<T> (this.VectorFilename, this.Dimension);
			if (LOAD_IN_MEMORY) {
				Console.WriteLine ("XXX DEBUG Loading vectors in memory {0}", this.VectorFilename);
				this.LoadInMemory ();
			} else {
				Console.WriteLine ("XXX DEBUG Using memory mapped file {0}", this.VectorFilename);
			}
		}

		public void LoadInMemory()
		{
			var vecs = new List<T[]> ();
			var len = this.VECTORS.Count;
			for (int docID = 0; docID < len; ++docID) {
				if (docID % 10000 == 0) {
					Console.WriteLine("== loading vectors: {0}/{1}, {2:0.00}%", docID, len, docID * 100.0 / len);
				}
				vecs.Add(this.VECTORS[docID]);
			}
			this.VECTORS = vecs;
		}

		public virtual void Save(BinaryWriter Output)
		{
			Output.Write(this.Name);
			Output.Write (this.VectorFilename);
			Output.Write((int) this.Dimension);
			Output.Write ((float) this.P);
			using (var vecs = new DiskVectorList<T> (this.VectorFilename, this.Dimension)) {
				var len = this.VECTORS.Count;
				for (int docID = 0; docID < len; ++docID) {
					vecs.Add (this.VECTORS [docID]);
				}
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public VectorDB () : base()
		{
		}

		/// <summary>
		/// Get/Set (and load) the database name
		/// </summary>
		public string Name {
			get;
			set;
		}
		
		public IResult CreateResult (int K, bool ceiling)
		{
			return new Result (K, ceiling);
		}

		/// <summary>
		///  The accumulated number of distances in the space
		/// </summary>
		public int NumberDistances {
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

		/// <summary>
		/// Retrieves the object with identifier docid
		/// </summary>
		public object this[int docid]
		{
			get { return this.VECTORS[docid]; }
		}

		public void Build (string name, IList<IList<T>> _VECTORS, float _P = -1)
		{
			this.Name = name;
			this.P = _P;
			this.VECTORS = new List<T[]> (this.VECTORS.Count);
			foreach (var list in _VECTORS) {
				var new_list = new T[list.Count];
				list.CopyTo(new_list, 0);
				this.VECTORS.Add (new_list);
			}
			this.Dimension = _VECTORS [0].Count;
		}

		public void Build (string inputname, string vecsname)
		{
			this.Name = inputname;
			this.VectorFilename = vecsname;
			// dim size pnorm dbvectors / Example: 112 112682 2 colors.ascii
			string desc = File.ReadAllLines(inputname)[0];
			string[] m = desc.Split (' ');
			// read from this.name file
			this.Dimension = int.Parse (m [0]);
			int len = int.Parse (m [1]);
			this.P = float.Parse (m[2]);
			this.VECTORS = new List<T[]>(len);
			var dbvecs = Dirty.CombineRelativePath(inputname, m [3]);

			Console.WriteLine ("** Reading vectors from file {0}", dbvecs);
			using (var Input = new StreamReader(File.OpenRead(dbvecs))) {
				for (int i = 0; i < len; ++i) {
					if (i % 10000 == 0) {
						Console.WriteLine ("*\t\t* reading {0} of {1} (adv. {2:0.00}%)", i + 1, len, i * 100.0 / len);
					}
					var line = Input.ReadLine ();
					var vec = this.Parse (line, false) as T[];
					this.VECTORS.Add(vec);
				}
			}
		}

		/// <summary>
		/// Returns a vector from an string
		/// </summary>
		public object Parse (string s, bool isquery)
		{
			if (s.StartsWith ("obj")) {
				return this[int.Parse (s.Split (' ')[1])];
			}
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
