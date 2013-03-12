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
	public class DynamicSequentialFixedOrder : DynamicSequential
	{
		protected HashSet<int> docs;
        protected int[] order;
        protected BitStream32 removed;
        protected int start_pos;

		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicSequentialFixedOrder ()
		{
		}

		public override void Remove (int objID)
        {
            this.docs.Remove(objID);
            this.removed[objID] = true;
		}

        public override int GetAnyItem ()
        {
            while (this.start_pos < this.order.Length) {
                var objID = this.order [this.start_pos];
                if (this.removed [objID]) {
                    ++this.start_pos;
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
            this.start_pos = 0;
            if (sample == null) {
                this.order = RandomSets.GetRandomPermutation (this.DB.Count);
            } else {
                var nsample = sample.Count;
                this.order = new int[nsample];
                sample.CopyTo(this.order, 0);
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
