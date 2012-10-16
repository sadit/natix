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
//   Original filename: natix/natix/Numeric/Numeric.cs.template
//
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	/// The Numeric manager for Byte
	/// </summary>
	public class NumericByte : INumeric<Byte>
	{
		/// <summary>
		/// Constructor
		/// </summary>

		public NumericByte ()
		{
		}
		/// <summary>
		/// From double to Byte
		/// </summary>
		
		public Byte FromDouble (double d)
		{
			return (Byte)d;
		}
		/// <summary>
		/// From integer
		/// </summary>
		
		public Byte FromInt(int d)
		{
			return (Byte)d;
		}

		/// <summary>
		/// From Byte to double
		/// </summary>
		
		public double ToDouble (Byte d)
		{
			return d;
		}
		/// <summary>
		/// To integer
		/// </summary>

		public int ToInt(Byte d)
		{
			return (int)d;
		}
		/// <summary>
		/// Write a numeric object to a binary stream
		/// </summary>

		public void WriteBinary (BinaryWriter bw, Byte d)
		{
			bw.Write (d);
		}
		/// <summary>
		/// Reads a numeric object from a binary stream
		/// </summary>
		
		public Byte ReadBinary (BinaryReader br)
		{
			return br.ReadByte ();
		}

		/// <summary>
		/// Substraction
		/// </summary>

		public double Sub (Byte a, Byte b)
		{
			return a - b;
		}
		/// <summary>
		/// Sum
		/// </summary>

		public double Sum (Byte a, Byte b)
		{
			return a + b;
		}
		/// <summary>
		/// Product
		/// </summary>

		public double Prod (Byte a, Byte b)
		{
			return a * b;
		}
		/// <summary>
		///The size of an object in memory or disk
		/// </summary>

		public int SizeOf()
		{
			return sizeof(Byte);
		}
		/// <summary>
		/// Read a binary vector of Byte
		/// </summary>

		public void ReadBinaryVector (IList<Byte> vec, Stream r, int dim)
		{
			int len = sizeof(Byte) * dim;
			byte[] buff = new byte[len];
			r.Read (buff, 0, len);
			//MemoryStream m = new MemoryStream (buff);
			BinaryReader m = new BinaryReader (new MemoryStream(buff));
			for (int i = 0; i < dim; i++) {
				vec[i] = m.ReadByte();
			}
		}
		// *****************
		// vector distances
		// *****************
		public double DistLP(IList<Byte> a, IList<Byte> b, float p, bool do_sqrt) {
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
		public double DistL2 (IList<Byte> a, IList<Byte> b)
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
		public double DistL1(IList<Byte> a, IList<Byte> b) {
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
		public double DistLInf(IList<Byte> a, IList<Byte> b) {
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

		public double DistCos(IList<Byte> a, IList<Byte> b) {
			var M = SimCos(a, b);
			return Math.Acos(M);
		}

		public double SimCos(IList<Byte> a, IList<Byte> b) {
			double sum,norm1,norm2;
			norm1=norm2=sum=0.0f;
			for(int i=0; i<a.Count; i++) {
		    	norm1+=(a[i] * a[i]);
	 	   		norm2+=(b[i] * b[i]);
	    		sum+= (a[i] * b[i]);
			}
			double M = sum/(Math.Sqrt(norm1)*Math.Sqrt(norm2));
			if (M > 1.0) {
				return 1.0;
			}
			if (M < -1.0) {
				return -1.0;
			}
			return M;
		}
	   /// <summary>
	   /// c[i] = a[i] + b[i]
	   /// </summary>
	   public void Sum(IList<Byte> a, IList<Byte> b, IList<Byte> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Byte)(a[i] + b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] - b[i]
	   /// </summary>
	   public void Sub(IList<Byte> a, IList<Byte> b, IList<Byte> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Byte)(a[i] - b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] * b[i]
	   /// </summary>
	   public void Prod(IList<Byte> a, IList<Byte> b, IList<Byte> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Byte)(a[i] * b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] / b[i]
	   /// </summary>
	   public void Div(IList<Byte> a, IList<Byte> b, IList<Byte> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Byte)(a[i] / b[i]);
	    	}
	   }

	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public void Sum(IList<Byte> a, float b, IList<Byte> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Byte)(a[i] + b);
	    	}
	   }

	   public double Sum(IList<Byte> a)
	   {
	   		var len = a.Count;
	   		double s = 0;
	    	for (int i = 0; i < len; ++i) {
	    		s += a[i];
	    	}
	    	return s;
	   }
	   
	   public double Mean(IList<Byte> a)
	   {
	   		return this.Sum(a) / a.Count;
	   }
	   
	   public double Max(IList<Byte> a, out int pos)
	   {
	   		var len = a.Count;
	   		double s = a[0];
	   		pos = 0;
	    	for (int i = 1; i < len; ++i) {
	    		var x = a[i];
	    		if (s < x) {
	    			s = x;
	    			pos = i;
	    		}
	    	}
	    	return s;
	   }
	   
	   public double Min(IList<Byte> a, out int pos)
	   {
	   		var len = a.Count;
	   		double s = a[0];
	   		pos = 0;
	    	for (int i = 1; i < len; ++i) {
	    		var x = a[i];
	    		if (s > x) {
	    			s = x;
	    			pos = i;
	    		}
	    	}
	    	return s;
	   }

	   public double Var(IList<Byte> a, double mean)
	   {
	   		var len = a.Count;
	   		double s = 0;
	    	for (int i = 0; i < len; ++i) {
	    		var x = a[i] - mean;
	    		s += x*x;
	    	}
	    	return s / len;
	   }

	   public double StdDev(IList<Byte> a, double mean)
	   {
	   		return Math.Sqrt(Var(a, mean));
	   }
	   
   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public void Prod (IList<Byte> a, float b, IList<Byte> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (Byte)(a[i] * b);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public Byte SumSingle(Byte a, float b)
	   {
			return (Byte) (a + b);
	   }

   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public Byte ProdSingle (Byte a, float b)
	   {
   			return (Byte) (a * b);
	   }
	 	   	   
    }
}