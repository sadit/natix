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
//   Original filename: natix/SimilaritySearch/Spaces/VectorSpace.cs
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
	public class VectorSpace<T> : MetricDB
	{
		//static INumeric<T> Num = (INumeric<T>)(natix.Numeric.Get (typeof(T)));
		/// <summary>
		/// The underlying storage for vectors 
		/// </summary>
		public IList< IList< T > > VECTORS;
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
		/// The real distance to be used
		/// </summary>

		public virtual void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			this.Dimension = Input.ReadInt32 ();
			var len = Input.ReadInt32 ();
			this.VECTORS = new IList<T>[len];
			for (int i = 0; i < len; ++i) {
				var vec = new T[this.Dimension];
				PrimitiveIO<T>.ReadFromFile(Input, this.Dimension, vec);
				this.VECTORS[i] = vec;
			}
			this.P = Input.ReadSingle();
		}

		public virtual void Save(BinaryWriter Output)
		{
			Output.Write(this.Name);
			Output.Write((int) this.Dimension);
			var len = this.VECTORS.Count;
			Output.Write((int) len);
			for (int i = 0; i < len; ++i) {
				PrimitiveIO<T>.WriteVector(Output, this.VECTORS[i]);
			}
			Output.Write ((float) this.P);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public VectorSpace () : base()
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
			this.VECTORS = _VECTORS;
			this.Dimension = _VECTORS [0].Count;
		}

		public void Build (string inputname)
		{
			this.Name = inputname;
			// dim size pnorm dbvectors / Example: 112 112682 2 colors.ascii
			string desc = File.ReadAllLines(inputname)[0];
			string[] m = desc.Split (' ');
			// read from this.name file
			this.Dimension = int.Parse (m [0]);
			int len = int.Parse (m [1]);
			this.P = float.Parse (m[2]);
			var dbvecs = Dirty.CombineRelativePath(inputname, m [3]);
			this.VECTORS = new IList<T>[len];
			Console.WriteLine ("** Reading vectors from file {0}", dbvecs);
			using (var Input = new StreamReader(File.OpenRead(dbvecs))) {
				for (int i = 0; i < len; ++i) {
					if (i % 10000 == 0) {
						Console.WriteLine ("** reading {0} of {1} (adv. {2:0.00}%)", i+1, len, i * 100.0 / len);
					}
					var line = Input.ReadLine ();
					this.VECTORS [i] = (IList<T>)this.Parse (line, false);
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
			return PrimitiveIO<T>.ReadVectorFromString (s, this.Dimension);
		}

		/// <summary>
		/// Distance wrapper for any P-norm
		/// </summary>
		public virtual double Dist (object a, object b)
		{
			throw new NotImplementedException();
		}
	}
}
