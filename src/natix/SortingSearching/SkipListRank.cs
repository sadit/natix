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
//   Original filename: natix/SortingSearching/SkipList2.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.SortingSearching
{

	public class SkipListRank<T>
	{
        public class DataRank
        {
            public T Data;
            public int DeltaRank;

            public DataRank(T data) 
            {
                this.Data = data;
                this.DeltaRank = 0;
            }

            public override string ToString ()
            {
                return string.Format ("[Data: {0}, DeltaRank: {1}]", this.Data, this.DeltaRank);
            }
        }

        public class SkipList2X : SkipList2<DataRank>
        {
            public SkipList2X (double prob, Comparison<DataRank> comp) : base(prob, comp) {}
            protected override void decrease_log_n ()
            {
                return;
            }
        }
        public SkipList2X SKIPLIST;

		public SkipListRank (Comparison<T> cmp_fun)
		{
            this.SKIPLIST = new SkipList2X(0.5, (DataRank a, DataRank b) => cmp_fun(a.Data, b.Data));
            // this.SKIPLIST.OnDecreaseLogN = null;
		}
		
        public SkipList2<DataRank>.Node Add (T data)
        {
            var node = this.SKIPLIST.Add (new DataRank (data), null);
            if (this.SKIPLIST.Count == 1) {
                node.data.DeltaRank = 1;
                return node;
            }
            this.FixRank(node);
            var next = node.get_forward(0);
            while (next != this.SKIPLIST.TAIL) {
                if (next.Level <= node.Level) {
                    this.FixRank(next);
                } else {
                    next.data.DeltaRank += 1;
                }
                var level = next.Level;
                while (level == next.Level && next != this.SKIPLIST.TAIL) {
                    next = next.get_forward(level-1);
                }
            }
            return node;
        }

        protected void FixRank (SkipList2<DataRank>.Node node)
        {
            var previous_node = node.get_backward (0);
            var backward_node = node.get_backward (node.Level-1);
            if (previous_node == backward_node) {
                node.data.DeltaRank = 1;
            } else {
                node.data.DeltaRank = 1 + this.Rank (previous_node, backward_node);
            }
        }

        public SkipList2<DataRank>.Node Remove(T data)
        {
            var node = this.SKIPLIST.Remove(new DataRank(data), null);
            var next = node.get_forward(0);
            while (next != this.SKIPLIST.TAIL) {
                if (next.Level <= node.Level) {
                    this.FixRank(next);
                } else {
                    next.data.DeltaRank -= 1;
                }
                var level = next.Level;
                while (level == next.Level && next != this.SKIPLIST.TAIL) {
                    next = next.get_forward(level-1);
                }
            }
            return node;
        }

        public int Rank (SkipList2<DataRank>.Node node)
        {
            return this.Rank (node, this.SKIPLIST.HEAD);
        }

        public int Rank (SkipList2<DataRank>.Node node, SkipList2<DataRank>.Node reference_node)
        {
            if (node == reference_node) {
                return 0;
            }
            var rank = node.data.DeltaRank;
            var prev = node.get_backward (node.Level - 1);
            while (prev != reference_node) {
                rank += prev.data.DeltaRank;
                prev = prev.get_backward (prev.Level - 1);
            }
            return rank;
        }

        public SkipList2<DataRank>.Node Find(T data)
        {
            return this.SKIPLIST.Find(new DataRank(data), null);
        }

        public int Count {
            get {
                return this.SKIPLIST.Count;
            }
        }
	}
}
