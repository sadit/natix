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
//   Original filename: natix/Numeric/INumeric.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
		/// <summary>
	/// Defines some basic operations for numeric types, needed because Generics doesn't support numeric types
	/// </summary>
	public interface INumeric<T> where T: struct
	{
		/// <summary>
		/// Cast from double
		/// </summary>
		T FromDouble (double d);
		/// <summary>
		/// Parses a numeric value from string
		/// </summary>
		T FromString (string d);
		/// <summary>
		/// Cast from int
		/// </summary>
		T FromInt (int d);
		/// <summary>
		/// To double
		/// </summary>
		double ToDouble (T d);
		/// <summary>
		/// To integer 32bit
		/// </summary>
		int ToInt (T d);
		/// <summary>
		/// Write as binary
		/// </summary>
		void Save (BinaryWriter w, T d);
		/// <summary>
		/// Read from binary stream
		/// </summary>
		T Load (BinaryReader r);
		void LoadVector (BinaryReader input, T[] V, int startIndex, int count);
		void SaveVector (BinaryWriter output, T[] V, int startIndex, int count);
		/// <summary>
		/// Substraction of a - b
		/// </summary>
		double Sub (T a, T b);
		/// <summary>
		/// Sum two numbers
		/// </summary>
		double Sum (T a, T b);
		/// <summary>
		/// The product of two numbers
		/// </summary>
		double Prod (T a, T b);
		/// <summary>
		/// The size of a numeric object in memory
		/// </summary>
		int SizeOf ();
		/// <summary>
		/// Read a binary vector 
		/// </summary>
		// void ReadBinaryVector (T[] v, Stream r, int dim);
		/// <summary>
		///  L_P distance
		/// </summary>
		double DistLP (T[] a, T[] b, float p, bool do_sqrt);
		/// <summary>
		///  L_2 distance
		/// </summary>
		double DistL2 (T[] a, T[] b);
		/// <summary>
		///  L_1 distance
		/// </summary>
		double DistL1 (T[] a, T[] b);
		/// <summary>
		///  L_Inf distance
		/// </summary>
		double DistLInf (T[] a, T[] b);
		/// <summary>
		///  angle (from cosine) distance
		/// </summary>
		double DistCos (T[] a, T[] b);
		double SimCos (T[] a, T[] b);
		
		void Sum (T[] u, T[] v, T[] output);
		void Sub (T[] u, T[] v, T[] output);
		void Prod (T[] u, T[] v, T[] output);
		void Div (T[] u, T[] v, T[] output);
		void Sum (T[] u, float c, T[] output);
		double Sum (T[] u);
		double Mean (T[] u);
		double Min (T[] u, out int pos);
		double Max (T[] u, out int pos);
		double Var (T[] u, double mean);
		double StdDev (T[] u, double mean);
		void Prod (T[] u, float c, T[] output);
		T SumSingle (T u, float c);
		T ProdSingle (T u, float c);
	}
}

