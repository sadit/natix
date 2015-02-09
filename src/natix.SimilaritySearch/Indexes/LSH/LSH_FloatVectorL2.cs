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
	public class LSH_FloatVectorL2 : LSH
	{
		List<float[]> supportVectors;
		double b;
		const double W = 1 / 16.0;
		const int WBITS = 4;
		const int WMASK = 15;

		public float[] GetPStableRandomVector(Random rand, int dim)
		{
			// we choose Gaussing distribution for simplicity as p-stable distribution
			var list = new float[dim];
			//var mean = 0.0;
			//var stdDev = 1.0;
			for (int i = 0; i < dim; ++i) {
				// http://stackoverflow.com/questions/218060/random-gaussian-variables
				// Jarrett's suggestion of using a Box-Muller transform is good for a quick-and-dirty solution. A simple implementation:
				double u1 = rand.NextDouble(); 
				double u2 = rand.NextDouble();
				double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *	Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
				//double randNormal = mean + stdDev * randStdNormal;
				//list [i] = (float)randNormal;
				list [i] = (float)randStdNormal;
			}
			return list;
		}

		public override void PreBuild(Random rand, object firstObject)
		{
			this.b = rand.NextDouble () * W;

			float[] vec = this.DB [0] as float[];
			var dim = vec.Length;
			this.supportVectors = new List<float[]> ();
			for (int i = 0; i < this.Width; i+=WBITS) {
				vec = this.GetPStableRandomVector (rand, dim);
				this.supportVectors.Add (vec);
			}
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.b = Input.ReadDouble ();
			this.supportVectors = new List<float[]> ();
			var dim = (this.DB [0] as float[]).Length;
			for (int i = 0; i < this.Width; i+=WBITS) {
				var vec = new float[dim];
				PrimitiveIO<float>.LoadVector(Input, vec.Length, vec);
				this.supportVectors.Add (vec);
			}
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write (this.b);
			for (int i = 0, j = 0; j < this.Width; ++i, j+=WBITS) {
				PrimitiveIO<float>.SaveVector(Output, this.supportVectors[i]);
			}
		}

		/// <summary>
		/// Compute the LSH hashes
		/// </summary>
		public override int ComputeHash (object _u)
		{
			float[] u = _u as float[];
			int hash = 0;
			for (int i = 0, j = 0; j < this.Width; ++i, j+=WBITS) {
				var h = this.microHash (u, this.supportVectors [i]);
				hash |= h << j;
			}
			return hash;
		}

		public int microHash(float[] a, float[] b)
		{
			var x = 0.0;
			for (int i = 0; i < a.Length; ++i) {
				x += Math.Abs(a [i] * b [i]);
			}
			var c = (int)(((x + this.b) / a.Length) / W);
			return c & WMASK;
		}
	}
}
