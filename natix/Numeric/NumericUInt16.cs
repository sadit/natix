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
//   Original filename: natix/Numeric/NumericUInt16.cs
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
	/// The Numeric manager for UInt16
	/// </summary>
	public class NumericUInt16 : INumeric<UInt16>
	{
		/// <summary>
		/// Constructor
		/// </summary>

		public NumericUInt16 ()
		{
		}
		/// <summary>
		/// From double to UInt16
		/// </summary>
		
		public UInt16 FromDouble (double d)
		{
			return (UInt16)d;
		}
		/// <summary>
		/// From integer
		/// </summary>
		
		public UInt16 FromInt(int d)
		{
			return (UInt16)d;
		}

		/// <summary>
		/// From UInt16 to double
		/// </summary>
		
		public double ToDouble (UInt16 d)
		{
			return d;
		}
		/// <summary>
		/// To integer
		/// </summary>

		public int ToInt(UInt16 d)
		{
			return (int)d;
		}
		/// <summary>
		/// Write a numeric object to a binary stream
		/// </summary>

		public void WriteBinary (BinaryWriter bw, UInt16 d)
		{
			bw.Write (d);
		}
		/// <summary>
		/// Reads a numeric object from a binary stream
		/// </summary>
		
		public UInt16 ReadBinary (BinaryReader br)
		{
			return br.ReadUInt16 ();
		}

		/// <summary>
		/// Substraction
		/// </summary>

		public double Sub (UInt16 a, UInt16 b)
		{
			return a - b;
		}
		/// <summary>
		/// Sum
		/// </summary>

		public double Sum (UInt16 a, UInt16 b)
		{
			return a + b;
		}
		/// <summary>
		/// Product
		/// </summary>

		public double Prod (UInt16 a, UInt16 b)
		{
			return a * b;
		}
		/// <summary>
		///The size of an object in memory or disk
		/// </summary>

		public int SizeOf()
		{
			return sizeof(UInt16);
		}
		/// <summary>
		/// Read a binary vector of UInt16
		/// </summary>

		public void ReadBinaryVector (IList<UInt16> vec, Stream r, int dim)
		{
			int len = sizeof(UInt16) * dim;
			byte[] buff = new byte[len];
			r.Read (buff, 0, len);
			//MemoryStream m = new MemoryStream (buff);
			BinaryReader m = new BinaryReader (new MemoryStream(buff));
			for (int i = 0; i < dim; i++) {
				vec[i] = m.ReadUInt16();
			}
		}
		// *****************
		// vector distances
		// *****************
		public double DistLP(IList<UInt16> a, IList<UInt16> b, float p, bool do_sqrt) {
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
		public double DistL2 (IList<UInt16> a, IList<UInt16> b)
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
		public double DistL1(IList<UInt16> a, IList<UInt16> b) {
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
		public double DistLInf(IList<UInt16> a, IList<UInt16> b) {
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

		public double DistCos(IList<UInt16> a, IList<UInt16> b) {
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
	   public void Sum(IList<UInt16> a, IList<UInt16> b, IList<UInt16> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt16)(a[i] + b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] - b[i]
	   /// </summary>
	   public void Sub(IList<UInt16> a, IList<UInt16> b, IList<UInt16> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt16)(a[i] - b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] * b[i]
	   /// </summary>
	   public void Prod(IList<UInt16> a, IList<UInt16> b, IList<UInt16> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt16)(a[i] * b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] / b[i]
	   /// </summary>
	   public void Div(IList<UInt16> a, IList<UInt16> b, IList<UInt16> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt16)(a[i] / b[i]);
	    	}
	   }

	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public void Sum(IList<UInt16> a, float b, IList<UInt16> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt16)(a[i] + b);
	    	}
	   }

   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public void Prod (IList<UInt16> a, float b, IList<UInt16> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt16)(a[i] * b);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public UInt16 SumSingle(UInt16 a, float b)
	   {
			return (UInt16) (a + b);
	   }

   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public UInt16 ProdSingle (UInt16 a, float b)
	   {
   			return (UInt16) (a * b);
	   }
	 	   	   
    }
}