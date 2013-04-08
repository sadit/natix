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
	public class DiskVectorList<T> : ListGenerator<T[]>, IDisposable where T: struct
	{
		/// <summary>
		/// The underlying storage for vectors 
		/// </summary>
		public DiskList64<T> Data;
//		/// <summary>
//		/// The vector collection
//		/// </summary>
	    public CacheList64< T[] > Cache;
		/// <summary>
		/// Dimension of the space
		/// </summary>
		public int Dimension;
		int _Count;
		/// <summary>
		/// Constructor
		/// </summary>
		public DiskVectorList (string filename, int dim) : base()
		{
			this.Dimension = dim;
			this.Data = new DiskList64<T> (filename, 1024);
			this._Count = (int)(this.Data.Count / this.Dimension);
			var read_vector = new ListGen64<T[]> ((long i) => this.ReadVector (i), long.MaxValue);
			this.Cache = new CacheList64<T[]> (read_vector, 128);
		}

		~DiskVectorList()
		{
			this.Dispose ();
		}

		public void Dispose()
		{
			this.Data.Dispose ();
		}

		/// <summary>
		/// Length of the space
		/// </summary>
		public override int Count {
			get {
				return this._Count;
		    }
		}

		public T[] ReadVector(long docID)
		{
			var vec = new T[ this.Dimension ];
			var sp = docID * this.Dimension;
			for (int i = 0; i < this.Dimension; ++i) {
				vec[i] = this.Data[sp + i];
			}
			return vec;
		}

		public override void Add (T[] vector)
		{
			foreach (var u in vector) {
				this.Data.Add(u);
			}
			++this._Count;
		}

		public override T[] GetItem (int index)
		{
			return this.Cache [index];
			//return this.ReadVector (index);
		}

		public override void SetItem (int index, T[] vector)
		{
			this.Cache.cache.Remove (index);
			var sp = index * this.Dimension;
			for (int i = 0; i < this.Dimension; ++i) {
				this.Data[sp + i] = vector[i];
			}
		}
	}
}
