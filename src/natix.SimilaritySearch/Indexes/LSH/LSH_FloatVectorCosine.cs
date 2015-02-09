//
//   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NDesk.Options;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// LSH for Binary hamming space
	/// </summary>
	public class LSH_FloatVectorCosine : LSH
	{
		List<float[]> hyperplanes;

		public override void PreBuild(Random rand, object firstObject)
		{
			float[] vec = this.DB [0] as float[];
			var dim = vec.Length;
			this.hyperplanes = new List<float[]> ();
			for (int i = 0; i < this.Width; ++i) {
				vec = new float[dim];
				for (int j = 0; j < dim; ++j) {
					vec [j] = (float)rand.NextDouble ();
				}
				this.hyperplanes.Add (vec);
			}
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.hyperplanes = new List<float[]> ();
			var dim = (this.DB [0] as float[]).Length;
			for (int i = 0; i < this.Width; ++i) {
				var vec = new float[dim];
				PrimitiveIO<float>.LoadVector(Input, vec.Length, vec);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			for (int i = 0; i < this.Width; ++i) {
				PrimitiveIO<float>.SaveVector(Output, this.hyperplanes[i]);
			}
		}

		/// <summary>
		/// Compute the LSH hashes
		/// </summary>
		public override int ComputeHash (object _u)
		{
			float[] u = _u as float[];
			int hash = 0;
			for (int j = 0; j < this.Width; j++) {
				if (this.IsPositiveDotProduct (u, this.hyperplanes [j])) {
					hash ^= 1 << j;
				}
			}
			return hash;
		}

		public bool IsPositiveDotProduct(float[] a, float[] b)
		{
			var x = 0.0;
			for (int i = 0; i < a.Length; ++i) {
				x += a [i] * b [i];
			}
			return x >= 0;
		}
	}
}
