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
//   Original filename: natix/SimilaritySearch/Indexes/DynamicSequentialHash.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NDesk.Options;
using natix;
using natix.SortingSearching;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The sequential index
	/// </summary>
	public class DynamicSequentialOrdered : DynamicSequential
	{
		protected HashSet<int> docs;
        public List<int> order;
        protected BitStream32 removed;

		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicSequentialOrdered ()
		{
		}

		public override void Remove (int objID)
        {
            this.docs.Remove(objID);
            this.removed[objID] = true;
		}

        public void SortByPivot (object piv)
        {
            DynamicSequential.Stats stats;
            var items = this.ComputeDistances (piv, null, out stats);
            this.SortByDistance (items);
            for (int i = 0; i < items.Count; ++i) {
                this.order[i] = items[i].objID;
            }
        }

        public override int GetAnyItem ()
        {
            while (this.order.Count > 0) {
                var last = this.order.Count - 1;
                var objID = this.order [last];
                if (this.removed [objID]) {
                    this.order.RemoveAt (last);
                } else {
                    return objID;
                }
            }
            throw new IndexOutOfRangeException();
        }

        public override int Count {
            get {
                return this.docs.Count;
            }
        }

        public override IEnumerable<int> Iterate ()
        {
            return this.docs;
        }

		/// <summary>
		/// API build command
		/// </summary>
		public virtual void Build (MetricDB db, IList<int> sample = null)
        {
            this.DB = db;
            if (sample == null) {
                sample = RandomSets.GetRandomPermutation (this.DB.Count);
            } 
            var nsample = sample.Count;
            this.order = new List<int>(nsample);
            for (int i = nsample - 1; i >= 0; --i) {
                this.order.Add(sample[i]);
            }
            this.docs = new HashSet<int>();
            this.removed = new BitStream32();
            this.removed.Write(false, this.DB.Count);
			foreach (var s in sample) {
				this.docs.Add(s);
			}
		}
	}
}
