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

	public class SkipList2<T>
	{
		short current_level;
		float prob;
		Comparison<T> cmp_fun;
		public Node HEAD;
		public Node TAIL;
		Random rand;
		int n;		
		
		public SkipList2 (double prob, Comparison<T> cmp_fun)
		{
			this.rand = new Random ();
			this.prob = (float)prob;
			this.cmp_fun = cmp_fun;
			this.current_level = 1;
			this.HEAD = new Node (this.current_level);
			this.TAIL = new Node (this.current_level);
			this.HEAD.set_forward (0, this.TAIL);
			this.TAIL.set_backward (0, this.HEAD);
			this.n = 0;
		}
		
		public int Count {
			get {
				return this.n;
			}
		}
		
		Node get_backward (Node node, int index)
		{
			while (this.HEAD != node) {
				if (node.Level > index)
					break;
				node = node.get_backward (node.Level - 1);
			}
			return node;
		}

		static void set_forward (Node node, int level, Node _value)
		{
			node.set_forward (level, _value);
		}

		static void set_backward (Node node, int level, Node _value)
		{
			node.set_backward (level, _value);
		}
		
		protected Node FindNode (T key, Node s, int level)
		{
			int i;
			for (i = level; i >= 0; i--) {
				while (s.get_forward(i) != this.TAIL && this.cmp_fun (s.get_forward(i).data, key) <= 0) {
					s = s.get_forward (i);
				}
			}
			return s;
		}

		protected Node _FindNodeAdaptive (T key, Node s, out int level)
		{
			int i;
			for (i = 0; i < this.current_level; i++) {
				do {
					if (s.get_forward(i) == this.TAIL || this.cmp_fun (s.get_forward(i).data, key) > 0) {
						level = i;
						return s;
					} else {
						s = s.get_forward(i);
					}
				} while (i + 1 == s.Level);
			}
			level = i - 1;
			return s;
		}
		
		protected Node _FindNodeAdaptiveReverse (T key, Node s, out int level)
		{
			int i;
			for (i = 0; i < this.current_level; i++) {
				do {
					if (s.get_backward(i) == this.HEAD || this.cmp_fun (s.get_backward(i).data, key) <= 0) {
						level = i;
						return s.get_backward(i);
					} else {
						s = s.get_backward(i);
					}
				} while (i + 1 == s.Level);
			}
			level = i - 1;
			return s.get_backward(i);
		}
		
		protected Node FindNodeAdaptive (T key, Node s)
		{
			int level;
			s = this._FindNodeAdaptive (key, s, out level);
			return this.FindNode (key, s, level);
		}

		protected Node FindNodeAdaptiveReverse (T key, Node s)
		{
			int level;
			s = this._FindNodeAdaptiveReverse (key, s, out level);
			return this.FindNode (key, s, level);
		}

		public Node Find (T key, AdaptiveContext ctx)
		{
			var s = this.FindNode (key, ctx);
			if (s == this.TAIL || this.cmp_fun (key, s.data) != 0) {
				throw new KeyNotFoundException (key.ToString ());
			}
			return s;
		}

		public bool Contains (T key)
		{
			var s = this.FindNode (key, null);
			if (s == this.TAIL || s == this.HEAD || this.cmp_fun (key, s.data) != 0) {
				return false;
			}
			return true;
		}
		
		virtual public Node FindNode (T key, AdaptiveContext ctx)
		{
			if (ctx == null) {
				return this.FindNode (key, this.HEAD, this.HEAD.Level - 1);
			}
			Node s;
			if (ctx.IsFinger) {
				if (this.HEAD == ctx.StartNode) {
					s = this.FindNodeAdaptive (key, this.HEAD);
				} else if (this.cmp_fun (ctx.StartNode.data, key) > 0) {
					s = this.FindNodeAdaptiveReverse (key, ctx.StartNode);
				} else {
					s = this.FindNodeAdaptive (key, ctx.StartNode);
				}
				ctx.StartNode = s;
			} else {
				s = this.FindNodeAdaptive (key, this.HEAD);
			}
			return s;
		}
		
		short random_level ()
		{
			short l = 1;
			while (l < this.current_level && this.prob < this.rand.NextDouble()) {
				l++;
			}
			return l;
		}
		
		// whenever log_n increases then the current level must be increased
		// and the links should be adjusted
		void increase_log_n ()
		{
			//int level = this.HEAD.Level - 1;
			this.HEAD.Push (this.TAIL, null);
			this.TAIL.Push (null, this.HEAD);
//			var prev = this.HEAD;
//			var curr = prev.get_forward (level);
//			while (curr != this.TAIL) {
//				var new_level = random_level ();
//				if (new_level > 2) {
//					curr.Push (this.TAIL, prev);
//					prev.set_forward (level + 1, curr);
//					this.TAIL.set_backward (level + 1, curr);
//					prev = curr;
//				}
//				curr = curr.get_forward (level);
//			}
			this.current_level++;
		}

		protected virtual void decrease_log_n ()
		{
			int level = this.HEAD.Level - 1;
			var curr = this.HEAD;
			var next = curr.get_forward (level);
			do {
				curr.Pop ();
				curr = next;
				next = curr.get_forward (level);
			} while (curr != this.TAIL);
			curr.Pop ();
			this.current_level--;
			if (this.current_level == 0) {
				this.HEAD.Push (this.TAIL, null);
				this.TAIL.Push (null, this.HEAD);
				this.current_level = 1;
			}
		}

		public Node Add (T new_data, AdaptiveContext ctx)
		{
			var level = this.random_level ();
			var prev = this.FindNode (new_data, ctx);
			var new_node = new Node (level);
			new_node.data = new_data;
			// Console.WriteLine ("**** ADD data: {0}, n: {1}, first.level: {2}", new_data, this.Count, this.FIRST.Level);
			for (short k = 0; k < level; k++) {
				Node forward;
				var backward = this.get_backward (prev, k);
				forward = backward.get_forward (k);
				set_forward (backward, k, new_node);
				set_forward (new_node, k, forward);
				set_backward (new_node, k, backward);
				set_backward (forward, k, new_node);
			}
			this.n++;
			if (this.n > (1 << this.current_level)) {
				this.increase_log_n ();
			}
			return new_node;
		}
				
		public Node Remove (Node s, AdaptiveContext ctx)
		{
			if (ctx != null && ctx.IsFinger) {
				ctx.StartNode = s.get_backward (0);
			}
			for (int i = 0; i < s.Level; i++) {
				set_forward (s.get_backward (i), i, s.get_forward (i));
				set_backward (s.get_forward (i), i, s.get_backward (i));
			}
			this.n--;
			if (this.n < (1 << (this.current_level - 1))) {
				this.decrease_log_n ();
			}
			return s;
		}
		
		public Node Remove (T key, AdaptiveContext ctx)
		{
			var s = this.FindNode (key, ctx);
			if (this.HEAD == s || this.TAIL == s || this.cmp_fun (key, s.data) != 0) {
				Console.WriteLine ("s.IsLAST: {0}, s.IsFIRST: {1}, key: {2}, s.data: {3}",
					s == this.TAIL, s == this.HEAD, key, s.data);
				throw new KeyNotFoundException ();
			}
			return this.Remove (s, ctx);
		}
		
		public T GetFirst ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.HEAD.get_forward (0).data;
			}
		}

		public T RemoveFirst ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.HEAD.get_forward(0), null).data;
			}
		}

		public Node RemoveFirstNode ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.HEAD.get_forward(0), null);
			}
		}

		public T GetLast ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.TAIL.get_backward (0).data;
			}
		}

		public T RemoveLast ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.TAIL.get_backward(0), null).data;
			}
		}

		public Node RemoveLastNode ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.HEAD.get_backward(0), null);
			}
		}

		public virtual T GetItem (int index)
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
			var s = this.HEAD.get_forward (0);
			int i = 0;
			w.Write ("(n: {0}) ", this.n);
			w.Write ("{ ");
			while (s != TAIL) {
				w.Write ("(i: {0}, level: {1}, data: {2}), ", i, s.Level, s.data);
				i++;
				s = s.get_forward(0);
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
		public IEnumerable<T> ReversalTraverse ()
		{
			foreach (var u in this.ReversalTraverseNodes ()) {
				yield return u.data;
			}
		}
	
		public IEnumerable<Node> TraverseNodes ()
		{
			if (this.Count > 0) {
				var s = this.HEAD.get_forward (0);
				while (s != this.TAIL) {
					yield return s;
					s = s.get_forward (0);
				}
			}
		}
		
		public IEnumerable< Node > ReversalTraverseNodes ()
		{
			if (this.Count > 0) {
				var s = this.TAIL.get_backward (0);
				while (s != this.HEAD) {
					yield return s;
					s = s.get_backward (0);
				}			
			}
		}

        ////
        public class Node
        {
            // public short level;
            public T data;
            public List<Node> pointers;
            
            public Node (short level)
            {
                this.pointers = new List<Node> (level<<1);
                for (int i = 0; i < level; i++) {
                    this.Push (null, null);
                }
            }
            
            public Node get_forward (int level)
            {
                try {
                    return this.pointers [(level << 1)];
                } catch (Exception e) {
                    Console.WriteLine ("XXXX get_forward i: {0}, pointers: {1}, shifted: {2}", level, pointers.Count, level << 1);
                    throw e;
                }
            }
            
            public Node get_backward (int level)
            {
                return this.pointers [(level << 1) + 1];
            }
            
            public Node set_forward (int level, Node p)
            {
                return this.pointers [(level << 1)] = p;
            }
            
            public Node set_backward (int level, Node p)
            {
                return this.pointers [(level << 1) + 1] = p;
            }
            
            public void Push (Node forward, Node backward)
            {
                this.pointers.Add (forward);
                this.pointers.Add (backward);
            }
            
            public void Pop ()
            {
                this.pointers.RemoveAt (this.pointers.Count - 1);
                this.pointers.RemoveAt (this.pointers.Count - 1);
            }

            public int Level {
                get {
                    return this.pointers.Count >> 1;
                }
            }
        }
        
        public class AdaptiveContext
        {
            public bool IsFinger;
            public Node StartNode;
            
            public AdaptiveContext ()
            {
                this.IsFinger = false;
                this.StartNode = null;
            }
            
            public AdaptiveContext (bool isFinger, Node startNode)
            {
                this.IsFinger = isFinger;
                this.StartNode = startNode;
            }
        }

	}
}
