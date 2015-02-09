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
//   Original filename: natix/SortingSearching/SkipList.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.SortingSearching
{

	public class SkipList<T>
	{
		float prob;
		Comparison<T> cmp_fun;
		public Node HEAD;
		public Node TAIL;
		Random rand;
		int n;
		
		public SkipList (double prob, Comparison<T> cmp_fun)
		{
			this.rand = new Random ();
			this.prob = (float)prob;
			this.cmp_fun = cmp_fun;
			// this.max_level = (short)Math.Ceiling (Math.Log (expected_n, 2) / Math.Log (1.0 / this.prob, 2));
	
			this.HEAD = new Node (true);
			this.TAIL = new Node (false);
			this.HEAD.forward.Add (this.TAIL);
			this.n = 0;
		}
		
		public int Count {
			get {
				return this.n;
			}
		}
		
		public Node Find (T key)
		{
			var s = this.FindNode (key);
			if (s.IsLAST || this.cmp_fun (key, s.data) != 0) {
				throw new KeyNotFoundException (key.ToString ());
			}
			return s;
		}
		
		protected Node FindNode (T key)
		{
			Node s = this.HEAD;
			int i;
			for (i = this.HEAD.Level - 1; i >= 0; i--) {
				while (!s.forward[i].IsLAST && this.cmp_fun (s.forward[i].data, key) <= 0) {
					s = s.forward [i];
				}
			}
			return s;
		}
		
		short random_level()
		{
			short l = 1;
			while (this.prob < this.rand.NextDouble() && l < this.HEAD.Level) {
				l++;
			}
			return l;
		}

		public Node Add (T new_data)
		{
			int i;
			Node s = this.HEAD;
			if (this.HEAD.Level < Math.Ceiling(Math.Log (this.n + 1, 2))) {
				this.HEAD.PushForwards (1, this.TAIL);
			}
			var new_level = this.random_level ();
			var new_node = new Node (new_data);
			new_node.PushForwards (new_level, null);
			for (i = this.HEAD.Level - 1; i >= 0; i--) {
				int cmp = -1;
				while (!s.forward[i].IsLAST) {
					cmp = this.cmp_fun (s.forward [i].data, new_data);
					if (cmp < 0) {
						s = s.forward [i];
					} else {
						break;
					}
				}
				if (i < new_level) {
					new_node.forward [i] = s.forward [i];
					s.forward [i] = new_node;
				}
			}
			this.n++;
			return new_node;
		}
				
		public Node Remove (T key)
		{
			int i;
			bool deleted = false;
			Node s = this.HEAD;
			for (i = this.HEAD.Level - 1; i >= 0; i--) {
				int cmp = -1;
				while (!s.forward[i].IsLAST) {
					cmp = this.cmp_fun (s.forward [i].data, key);
					if (cmp < 0) {
						s = s.forward [i];
					} else {
						break;
					}
				}
				if (cmp == 0) {
					s.forward [i] = s.forward [i].forward [i];
					deleted = true;
				}
			}
			if (deleted) {
				this.n--;
				if (this.HEAD.Level > Math.Ceiling (Math.Log (this.n, 2))) {
					if (this.HEAD.Level > 1) {
						this.HEAD.PopForwards ();
					}
				}
				return s;
			} else {
				throw new KeyNotFoundException ();
			}

		}
				
		public T GetItem (int index)
		{
			int i = 0;
			if (i >= this.n) {
				throw new ArgumentOutOfRangeException (String.Format("i: {0} should be less than n: {1}", i, this.n));
			}
			foreach (var u in this.Traverse ()) {
				if (i == index) {
					return u;
				}
				i++;
			}
			throw new ArgumentOutOfRangeException ("- error it must not reach this!! -");
		}
		
		public override string ToString ()
		{
			var w = new StringWriter ();
			var s = this.HEAD.forward[0];
			int i = 0;
			w.Write ("(n: {0}) ", this.n);
			w.Write ("{ ");
			while (!s.IsLAST) {
				w.Write ("(i: {0}, level: {1}, data: {2}), ", i, s.Level, s.data);
				i++;
				s = s.forward[0];
			}
			w.WriteLine ("<end>}");
			return w.ToString ();
		}

		public IEnumerable<T> Traverse ()
		{
			foreach (var u in this.TraverseNodes ()) {
				yield return u.data;
			}
		}

		public IEnumerable<Node> TraverseNodes ()
		{
			// Console.WriteLine ("****=====> this.Count: {0}", this.Count);
			if (this.Count > 0) {
				var s = this.HEAD.forward [0];
				while (!s.IsLAST) {
					yield return s;
					s = s.forward [0];
				}
			}
		}

        public class Node
        {
            public T data;
            public List< Node > forward;
            
            public Node (bool init_forward)
            {
                if (init_forward) {
                    this.forward = new List< Node >();
                } else {
                    this.forward = null;
                }
            }
            
            public Node (T _data)
            {
                this.data = _data;
                // this.level = _level;
                this.forward = new List< Node >();
            }
            
            public bool IsLAST {
                get {
                    return this.forward == null;
                }
            }
            
            public int Level {
                get {
                    return this.forward.Count;
                }
            }
            
            public void PushForwards (int levels, Node next)
            {
                for (int i = 0; i < levels; i++) {
                    this.forward.Add (next);
                }
            }
            
            public void PopForwards ()
            {
                if (this.IsLAST) {
                    return;
                }
                int count = this.forward.Count - 1;
                if (count == -1) {
                    return;
                }
                this.forward [count].PopForwards ();
                this.forward.RemoveAt (count);
            }
        }

	}
}
