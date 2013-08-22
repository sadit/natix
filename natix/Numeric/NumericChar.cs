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
    /// The Numeric manager for Char
    /// </summary>
    public class NumericChar : INumeric<Char>
    {
	/// <summary>
	/// Constructor
	/// </summary>

	public NumericChar ()
	    {
	    }
	/// <summary>
	/// From double to Char
	/// </summary>
		
	public Char FromDouble (double d)
	{
	    return (Char)d;
	}
	/// <summary>
	/// From integer
	/// </summary>
		
	public Char FromInt(int d)
	{
	    return (Char)d;
	}

	/// <summary>
	/// From Char to double
	/// </summary>
		
	public double ToDouble (Char d)
	{
	    return d;
	}
	/// <summary>
	/// To integer
	/// </summary>
		
	public int ToInt(Char d)
	{
	    return (int)d;
	}
	/// <summary>
	/// Write a numeric object to a binary stream
	/// </summary>

	public void Save (BinaryWriter bw, Char d)
	{
	    bw.Write (d);
	}
	/// <summary>
	/// Reads a numeric object from a binary stream
	/// </summary>
		
	public Char Load (BinaryReader br)
	{
	    return br.ReadChar ();
	}
 
 	public void LoadVector(BinaryReader input, Char[] vec, int startIndex, int count)
 	{
 		for (int i = 0; i < count; ++i) {
 			vec[startIndex + i ] = input.ReadChar ();
 		}
 	}

	public void SaveVector(BinaryWriter output, Char[] vec, int startIndex, int count)
 	{
 		for (int i = 0; i < count; ++i) {
 			output.Write(vec[startIndex+i]);
 		}
 	}
 	
	/// <summary>
	/// Substraction
	/// </summary>

	public double Sub (Char a, Char b)
	{
	    return a - b;
	}
	/// <summary>
	/// Sum
	/// </summary>

	public double Sum (Char a, Char b)
	{
	    return a + b;
	}
	/// <summary>
	/// Product
	/// </summary>

	public double Prod (Char a, Char b)
	{
	    return a * b;
	}
	/// <summary>
	///The size of an object in memory or disk
	/// </summary>

	public int SizeOf()
	{
	    return sizeof(Char);
	}
	/// <summary>
	/// Read a binary vector of Char
	/// </summary>

	// *****************
	// vector distances
	// *****************
	public double DistLP(Char[] a, Char[] b, float p, bool do_sqrt)
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
	public double DistL2 (Char[] a, Char[] b)
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
	public double DistL1(Char[] a, Char[] b)
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
	public double DistLInf(Char[] a, Char[] b)
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

	public double DistCos(Char[] a, Char[] b)
	{
	    var M = SimCos(a, b);
	    return Math.Acos(M);
	}

	public double SimCos(Char[] a, Char[] b)
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
	public void Sum(Char[] a, Char[] b, Char[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (Char)(a[i] + b[i]);
	    }
	}
	   
	/// <summary>
	/// c[i] = a[i] - b[i]
	/// </summary>
	public void Sub(Char[] a, Char[] b, Char[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (Char)(a[i] - b[i]);
	    }
	}
       
	/// <summary>
	/// c[i] = a[i] * b[i]
	/// </summary>
	public void Prod(Char[] a, Char[] b, Char[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (Char)(a[i] * b[i]);
	    }
	}
	   
	/// <summary>
	/// c[i] = a[i] / b[i]
	/// </summary>
	public void Div(Char[] a, Char[] b, Char[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (Char)(a[i] / b[i]);
	    }
	}

	/// <summary>
	/// c[i] = a[i] + b
	/// </summary>
	public void Sum(Char[] a, float b, Char[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (Char)(a[i] + b);
	    }
	}

	public double Sum(Char[] a)
	{
	    var len = a.Length;
	    double s = 0;
	    for (int i = 0; i < len; ++i) {
		s += a[i];
	    }
	    return s;
	}
	   
	public double Mean(Char[] a)
	{
	    return this.Sum(a) / a.Length;
	}
	   
	public double Max(Char[] a, out int pos)
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
	   
	public double Min(Char[] a, out int pos)
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

	public double Var(Char[] a, double mean)
	{
	    var len = a.Length;
	    double s = 0;
	    for (int i = 0; i < len; ++i) {
		var x = a[i] - mean;
		s += x*x;
	    }
	    return s / len;
	}

	public double StdDev(Char[] a, double mean)
	{
	    return Math.Sqrt(Var(a, mean));
	}
	   
	/// <summary>
	/// c[i] = a[i] * b
	/// </summary>
	public void Prod (Char[] a, float b, Char[] c)
	{
	    var len = a.Length;
	    for (int i = 0; i < len; ++i) {
		c[i] = (Char)(a[i] * b);
	    }
	}
	   
	/// <summary>
	/// c[i] = a[i] + b
	/// </summary>
	public Char SumSingle(Char a, float b)
	{
	    return (Char) (a + b);
	}

	/// <summary>
	/// c[i] = a[i] * b
	/// </summary>
	public Char ProdSingle (Char a, float b)
	{
	    return (Char) (a * b);
	}
	 	   	   
    }
}
