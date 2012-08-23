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
//   Original filename: natix/SortingSearching/PriorityQueue.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
//using System.Xml;
//using System.Xml.Serialization;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace natix.SortingSearching
{
	/// <summary>
	/// A simple priority queue
	/// </summary>
	public class PriorityQueue<T>
	{
		List<T> A;
		IComparer<T> comp;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="comp">
		/// Comparer 
		/// </param>
		/// <param name="size">
		/// The initial size of the queue
		/// </param>
		public PriorityQueue (IComparer<T> comp, int size)
		{
			this.comp = comp;
			this.A = new List<T> (size);
		}
		
		void MaxHeapify (int i)
		{
			int left = this.LeftNode (i);
			int right = this.RightNode (i);
			int largest = 0;
			if (left < A.Count && comp.Compare (A[left], A[i]) > 0) {
				largest = left;
			} else {
				largest = i;
			}
			if (right < A.Count && comp.Compare (A[right], A[largest]) > 0) {
				largest = right;
			}
			if (largest != i) {
				T A_i = A[i];
				A[i] = A[largest];
				A[largest] = A_i;
				this.MaxHeapify (largest);
			}
		}
		/// <summary>
		///  The number of items
		/// </summary>
		public int Count {
			get { return A.Count; }
		}
		
		/// <summary>
		///  The priorest key
		/// </summary>
		public T First {
			get { return A[0]; }
		}
		
		/// <summary>
		/// Increase key
		/// </summary>
		/// <param name="i">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="key">
		/// </param>
		public void HeapIncreaseKey (int i, T key)
		{
			if (comp.Compare (key, A[i]) <= 0) {
				throw new ArgumentException ("HeapIncreaseKey new key is smaller than current key");
			}
			A[i] = key;
			this.FixHeapProperty (i);
		}
		
		void FixHeapProperty (int i)
		{
			int parentNode = this.ParentNode (i);
			while (i > 0 && comp.Compare (A[parentNode], A[i]) < 0) {
				T A_i = A[i];
				A[i] = A[parentNode];
				A[parentNode] = A_i;
				i = this.ParentNode (i);
				parentNode = this.ParentNode (i);
			}
		}
		/// <summary>
		/// Push a key into the queue
		/// </summary>
		/// <param name="key">
		/// </param>
		public void Push (T key)
		{
			A.Add (key);
			this.FixHeapProperty(A.Count - 1);
		}

		/// <summary>
		/// Remove the priorest object
		/// </summary>
		/// <returns>
		/// </returns>
		public T RemoveFirst ()
		{
			T d = A[0];
			int lastIndex = this.Count - 1;
			if (lastIndex > 0) {
				A[0] = A[lastIndex];
				A.RemoveAt (lastIndex);
				this.MaxHeapify (0);
			} else {
				A.RemoveAt (lastIndex);
			}
			return d;
		}

		// we suppose a good JIT inlining access methods
		int ParentNode (int i)
		{
			return i >> 1;
		}
		
		int LeftNode (int i)
		{
			return i << 1;
		}
		
		int RightNode (int i)
		{
			return (i<<1) + 1;
		}
	}
	
	class IntegerComparerIncreasing: IComparer<int>
	{
		public IntegerComparerIncreasing()
		{			
		}
		public int Compare (int x, int y)
		{
			return x - y;
		}
	}

	class IntegerComparerDecreasing: IComparer<int>
	{
		public IntegerComparerDecreasing()
		{			
		}
		public int Compare (int x, int y)
		{
			return y - x;
		}
	}

}
