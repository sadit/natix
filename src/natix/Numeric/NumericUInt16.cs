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

	public UInt16 FromString (string d)
	{
	    return UInt16.Parse(d);
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

	public void Save (BinaryWriter bw, UInt16 d)
	{
	    bw.Write (d);
	}
	/// <summary>
	/// Reads a numeric object from a binary stream
	/// </summary>
		
	public UInt16 Load (BinaryReader br)
	{
	    return br.ReadUInt16 ();
	}
 
 	public void LoadVector(BinaryReader input, UInt16[] vec, int startIndex, int count)
 	{
 		for (int i = 0; i < count; ++i) {
 			vec[startIndex + i ] = input.ReadUInt16 ();
 		}
 	}

	public void SaveVector(BinaryWriter output, UInt16[] vec, int startIndex, int count)
 	{
 		for (int i = 0; i < count; ++i) {
 			output.Write(vec[startIndex+i]);
 		}
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

	// *****************
	// vector distances
	// *****************
	public double DistLP(UInt16[] a, UInt16[] b, float p, bool do_sqrt)
	{
	    double d = 0;
	    for (int i = 0; i < a.Length; i++) {
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
	public double DistL2 (UInt16[] a, UInt16[] b)
	{
	    double d = 0;
	    for (int i = 0; i < a.Length; i++) {
		double m = a[i] - b[i];
		d += m * m;
	    }
	    return Math.Sqrt(d);
	}
		
	/// <summary>
	/// Specialization for L1
	/// </summary>
	public double DistL1(UInt16[] a, UInt16[] b)
	{
	    double d = 0;
	    for (int i = 0; i < a.Length; i++) {
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
	public double DistLInf(UInt16[] a, UInt16[] b)
	{
	    double d = 0;
	    for (int i = 0; i < a.Length; i++) {
		double m = a[i]-b[i];
		if (m < 0) m *= -1;
		if (m > d) d = m;
	    }
	    return d;
	}

	/// <summary>
	/// Angle between two vectors (computing the cosine between them)
	/// </summary>

	public double DistCos(UInt16[] a, UInt16[] b)
	{
	    var M = SimCos(a, b);
	    return Math.Acos(M);
	}

	public double SimCos(UInt16[] a, UInt16[] b)
	{
	    double norm1, norm2, sum;
	    norm1 = norm2 = sum = 0;
	    for(int i=0; i<a.Length; i++) {
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
	public void Sum(UInt16[] a, UInt16[] b, UInt16[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (UInt16)(a[i] + b[i]);
	    }
	}
	   
	/// <summary>
	/// c[i] = a[i] - b[i]
	/// </summary>
	public void Sub(UInt16[] a, UInt16[] b, UInt16[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (UInt16)(a[i] - b[i]);
	    }
	}
       
	/// <summary>
	/// c[i] = a[i] * b[i]
	/// </summary>
	public void Prod(UInt16[] a, UInt16[] b, UInt16[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (UInt16)(a[i] * b[i]);
	    }
	}
	   
	/// <summary>
	/// c[i] = a[i] / b[i]
	/// </summary>
	public void Div(UInt16[] a, UInt16[] b, UInt16[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (UInt16)(a[i] / b[i]);
	    }
	}

	/// <summary>
	/// c[i] = a[i] + b
	/// </summary>
	public void Sum(UInt16[] a, float b, UInt16[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (UInt16)(a[i] + b);
	    }
	}

	public double Sum(UInt16[] a)
	{
	    var len = a.Length;
	    double s = 0;
	    for (int i = 0; i < len; ++i) {
		s += a[i];
	    }
	    return s;
	}
	   
	public double Mean(UInt16[] a)
	{
	    return this.Sum(a) / a.Length;
	}
	   
	public double Max(UInt16[] a, out int pos)
	{
	    var len = a.Length;
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
	   
	public double Min(UInt16[] a, out int pos)
	{
	    var len = a.Length;
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

	public double Var(UInt16[] a, double mean)
	{
	    var len = a.Length;
	    double s = 0;
	    for (int i = 0; i < len; ++i) {
		var x = a[i] - mean;
		s += x*x;
	    }
	    return s / len;
	}

	public double StdDev(UInt16[] a, double mean)
	{
	    return Math.Sqrt(Var(a, mean));
	}
	   
	/// <summary>
	/// c[i] = a[i] * b
	/// </summary>
	public void Prod (UInt16[] a, float b, UInt16[] c)
	{
	    var len = a.Length;
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
