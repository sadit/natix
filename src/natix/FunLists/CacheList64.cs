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
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
    public class CacheList64<T>  : ListGenerator64<T>
    {
        public ListGenerator64<T> list;
        public Dictionary<long,T> cache;
        public LinkedList<long> keys;
        int maxitems;

        public CacheList64 (ListGenerator64<T> list, int maxitems)
        {
            this.list = list;
            this.maxitems = maxitems;
            this.cache = new Dictionary<long, T>(maxitems);
            this.keys = new LinkedList<long>();
        }

        protected void FixSize()
        {
            if (this.keys.Count == this.maxitems) {
                var first = this.keys.First;
                this.cache.Remove(first.Value);
                this.keys.RemoveFirst();
            }
        }

        public override void Add (T item)
        {
            this.FixSize();
            var c = this.list.Count;
            this.list.Add(item);
            this.keys.AddLast(c);
            this.cache[c] = item;
        }

        public override long Count {
            get {
                return this.list.Count;
            }
        }

        public override T GetItem (long index)
        {
            T u;
            if (this.cache.TryGetValue (index, out u)) {
                return u;
            }
            this.FixSize();
            u = this.list[index];
            this.cache.Add( index, u );
            return u;
        }

        public override void SetItem (long index, T u)
        {
            if (this.cache.ContainsKey (index)) {
                this.list [index] = u;
                this.cache [index] = u;
            } else {
                this.FixSize ();
                this.list [index] = u;
                this.cache.Add (index, u);
            }
        }
    }
}