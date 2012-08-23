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
//   Original filename: natix/Numeric/NumericInt32.cs
// 
using System;
//using System.Text;
//using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
// using System.Runtime.Serialization;
//using System.Linq;

namespace natix
{
	/// <summary>
	/// The Numeric manager for Int32
	/// </summary>
	public class NumericInt32 : INumeric<Int32>
	{
		/// <summary>
		/// Constructor
		/// </summary>

		public NumericInt32 ()
		{
		}
		/// <summary>
		/// From double to Int32
		/// </summary>
		
		public Int32 FromDouble (double d)
		{
			return (Int32)d;
		}
		/// <summary>
		/// From integer
		/// </summary>
		
		public Int32 FromInt(int d)
		{
			return (Int32)d;
		}

		/// <summary>
		/// From Int32 to double
		/// </summary>
		
		public double ToDouble (Int32 d)
		{
			return d;
		}
		/// <summary>
		/// To integer
		/// </summary>

		public int ToInt(Int32 d)
		{
			return (int)d;
		}
		/// <summary>
		/// Write a numeric object to a binary stream
		/// </summary>

		public void WriteBinary (BinaryWriter bw, Int32 d)
		{
			bw.Write (d);
		}
		/// <summary>
		/// Reads a numeric object from a binary stream
		/// </summary>
		
		public Int32 ReadBinary (BinaryReader br)
		{
			return br.ReadInt32 ();
		}

		/// <summary>
		/// Substraction
		/// </summary>

		public double Sub (Int32 a, Int32 b)
		{
			return a - b;
		}
		/// <summary>
		/// Sum
		/// </summary>

		public double Sum (Int32 a, Int32 b)
		{
			return a + b;
		}
		/// <summary>
		/// Product
		/// </summary>

		public double Prod (Int32 a, Int32 b)
		{
			return a * b;
		}
		/// <summary>
		///The size of an object in memory or disk
		/// </summary>

		public int SizeOf()
		{
			return sizeof(Int32);
		}
		/// <summary>
		/// Read a binary vector of Int32
		/// </summary>

		public void ReadBinaryVector (IList<Int32> vec, Stream r, int dim)
		{
			int len = sizeof(Int32) * dim;
			byte[] buff = new byte[len];
			r.Read (buff, 0, len);
			//MemoryStream m = new MemoryStream (buff);
			BinaryReader m = new BinaryReader (new MemoryStream(buff));
			for (int i = 0; i < dim; i++) {
				vec[i] = m.ReadInt32();
			}
		}
		// *****************
		// vector distances
		// *****************
		public double DistLP(IList<Int32> a, IList<Int32> b, float p, bool do_sqrt) {
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i] - b[i];
				d += Math.Pow(Math.Abs(m), p);
			}
			if (do_sqrt) {
				return Math.Pow(d, 1.0/p);
			} else {
				return d;
			}
		}
		
		/// <summary>
		/// Specialization for L2
		/// </summary>
		public double DistL2 (IList<Int32> a, IList<Int32> b)
		{
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i] - b[i];
				d += m * m;
			}
			return Math.Sqrt(d);
		}
		
		/// <summary>
		/// Specialization for L1
		/// </summary>
		public double DistL1(IList<Int32> a, IList<Int32> b) {
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i] - b[i];
				if (m < 0) {
					d -= m;
				} else {
					d += m;
				}
			}
			return d;
		}
		
		/// <summary>
		/// Specialization for L-Infinity
		/// </summary>
		public double DistLInf(IList<Int32> a, IList<Int32> b) {
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i]-b[i];
				if (m < 0) m *= -1;
				if (m > d) d = m;
			}
			return d;
		}
		/// <summary>
		/// Angle between two vectors (computing the cosine between them)
		/// </summary>

		public double DistCos(IList<Int32> a, IList<Int32> b) {
			double sum,norm1,norm2;
			norm1=norm2=sum=0.0f;
			for(int i=0; i<a.Count; i++) {
		    	norm1+=(a[i] * a[i]);
	 	   		norm2+=(b[i] * b[i]);
	    		sum+= (a[i] * b[i]);
			}
			double M = sum/(Math.Sqrt(norm1)*Math.Sqrt(norm2));
			M=Math.Max(-1.0f,Math.Min(1.0f,M));
			//M=min(1.0,M);
			//cerr << "COS::::" << M << endl;
			return Math.Acos(M);
		}

	   /// <summary>
	   /// c[i] = a[i] + b[i]
	   /// </summary>
	   public void Sum(IList<Int32> a, IList<Int32> b, IList<Int32> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Int32)(a[i] + b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] - b[i]
	   /// </summary>
	   public void Sub(IList<Int32> a, IList<Int32> b, IList<Int32> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Int32)(a[i] - b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] * b[i]
	   /// </summary>
	   public void Prod(IList<Int32> a, IList<Int32> b, IList<Int32> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Int32)(a[i] * b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] / b[i]
	   /// </summary>
	   public void Div(IList<Int32> a, IList<Int32> b, IList<Int32> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Int32)(a[i] / b[i]);
	    	}
	   }

	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public void Sum(IList<Int32> a, float b, IList<Int32> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Int32)(a[i] + b);
	    	}
	   }

   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public void Prod (IList<Int32> a, float b, IList<Int32> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Int32)(a[i] * b);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public Int32 SumSingle(Int32 a, float b)
	   {
			return (Int32) (a + b);
	   }

   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public Int32 ProdSingle (Int32 a, float b)
	   {
   			return (Int32) (a * b);
	   }
	 	   	   
    }
}