//
//  Copyright 2014  Eric S. Tellez
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	public class DictionaryRandomAccess<KeyType,DataType> 
		: IEnumerable<KeyValuePair<KeyType,DataType>> where DataType : class 
	{
		List<KeyValuePair<KeyType,DataType>> Data;
		Dictionary<KeyType,int> Keys;
		int removed;
		Random rand;

		public DictionaryRandomAccess(int cap) {
			this.Data = new List<KeyValuePair<KeyType,DataType>>(cap);
			this.Keys = new Dictionary<KeyType, int>(cap);
			this.rand = new Random();
			this.removed = 0;
		}

		public DictionaryRandomAccess(DictionaryRandomAccess<KeyType,DataType> a)
		{
			this.Data = new List<KeyValuePair<KeyType,DataType>> (a.Count);
			this.Keys = new Dictionary<KeyType, int> (a.Count);
			this.rand = new Random ();
			this.removed = 0;
			foreach (var p in a) {
				this.Add (p.Key, p.Value);
			}
		}

		public void Add(KeyType key, DataType value)
		{
			this.Keys.Add (key, this.Data.Count);
			this.Data.Add (new KeyValuePair<KeyType, DataType>(key, value));
		}

		public DataType this[KeyType key]
		{
			get {
				return this.Data [this.Keys [key]].Value;
			}
			set {
				this.Data [this.Keys [key]] = new KeyValuePair<KeyType, DataType>(key, value);
			}
		}

		protected void Rebuild()
		{
			// var K = this.Keys;
			var V = this.Data;
			int size = this.Count;
			this.Keys = new Dictionary<KeyType, int> (size);
			this.Data = new List<KeyValuePair<KeyType,DataType>> (size);
			foreach (var p in V) {
				if (p.Value != null) {
					this.Add (p.Key, p.Value);
				}
			}
		}

		public int Count {
			get { return this.Data.Count - this.removed; }
		}

		public KeyValuePair<KeyType,DataType> GetRandom()
		{
			KeyValuePair<KeyType,DataType> p;
			do {
				p = this.Data [this.rand.Next (this.Data.Count)];
			} while (p.Value == null);
			return p;
		}

		public DataType Remove(KeyType k)
		{
			var m = this.Keys [k];
			this.Keys.Remove (k);
			var v = this.Data [m];
			this.Data [m] = new KeyValuePair<KeyType, DataType> (k, null);
			++this.removed;
			if (this.removed + this.removed > this.Data.Count) {
				this.Rebuild ();
			}
			// rebuild when the fill's ratio becomes too low
			return v.Value;
		}

		IEnumerator<KeyValuePair<KeyType,DataType>> IEnumerable<KeyValuePair<KeyType,DataType>>.GetEnumerator()
		{
			foreach (var p in this.Data) {
				if (p.Value != null) {
					yield return p;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (var p in this.Data) {
				if (p.Value != null) {
					yield return p;
				}
			}
		}
	}
}

