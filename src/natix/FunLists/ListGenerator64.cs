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
//   Original filename: natix/FunLists/ListGenerator64.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	/// A read only, not caching, list generator indexed with 64 bit integers.
	/// </summary>
	public abstract class ListGenerator64<T> : IEnumerable<T>
	{				
		public abstract T GetItem (long index);
		public abstract void SetItem (long index, T u);
		
		public T this[long index]
		{
			get {
				return this.GetItem (index);
			}
			set {
				this.SetItem (index, value);
			}
		}
		
		public abstract long Count {
			get;
		}
		
		public virtual long IndexOf (T item)
		{
			int hitem = item.GetHashCode ();
			for (long i = 0, L = this.Count; i < L; ++i) {
				if (hitem == this[i].GetHashCode ()) {
					return i;
				}
			}
			return -1L;
		}
		
		public bool Contains (T a)
		{
			return this.IndexOf (a) >= 0;
		}

		public virtual void Insert (int a, T item)
		{
			throw new NotSupportedException ();
		}
		
		public virtual void Add (T item)
		{
			throw new NotSupportedException ();
		}

		public virtual void Clear ()
		{
		}

		public virtual void Remove (object a)
		{
			throw new NotSupportedException ();
		}

		public virtual bool Remove (T a)
		{
			throw new NotSupportedException ();
		}

		public virtual void RemoveAt (int a)
		{
			throw new NotSupportedException ();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			for (long i = 0, L = this.Count; i < L; ++i) {
				yield return this[i];
			}
		}

		public IEnumerator GetEnumerator ()
		{	
			for (long i = 0, L = this.Count; i < L; ++i) {
				yield return this[i];
			}
		}

		public virtual void CopyTo (T[] A, int arrayIndex)
		{
			foreach (var item in ((IEnumerable<T>)this)) {
				A[arrayIndex] = item;
				++arrayIndex;
			}
		}

		public virtual bool IsReadOnly {
			get { return false; }
		}
	}

}

