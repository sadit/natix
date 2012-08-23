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
	public class SkipNode2<T>
	{
		// public short level;
		public T data;
		public List<SkipNode2<T>> pointers;
		
		public SkipNode2 (short level)
		{
			// this.data = _data;
			// this.level = _level;
			this.pointers = new List<SkipNode2<T>> (level<<1);
			for (int i = 0; i < level; i++) {
				this.Push (null, null);
			}
		}
		
		public SkipNode2<T> get_forward (int i)
		{
			try {
				return this.pointers [(i << 1)];
			} catch (Exception e) {
				Console.WriteLine ("XXXX get_forward i: {0}, pointers: {1}, shifted: {2}", i, pointers.Count, i << 1);
				throw e;
			}
		}
		
		public SkipNode2<T> get_backward (int i)
		{
			return this.pointers [(i << 1) + 1];
		}
		
		public SkipNode2<T> set_forward (int i, SkipNode2<T> p)
		{
			return this.pointers [(i << 1)] = p;
		}
		
		public SkipNode2<T> set_backward (int i, SkipNode2<T> p)
		{
			return this.pointers [(i << 1) + 1] = p;
		}
		
		public void Push (SkipNode2<T> forward, SkipNode2<T> backward)
		{
			this.pointers.Add (forward);
			this.pointers.Add (backward);
		}
		
		public void Pop ()
		{
			this.pointers.RemoveAt (this.pointers.Count - 1);
			this.pointers.RemoveAt (this.pointers.Count - 1);
		}

		/*public SkipNode2 (T _data, short level)
		{
			this.data = _data;
			// this.level = _level;
			this.backward = new SkipNode2<T>[level];
			this.forward = new SkipNode2<T>[level];
		}*/
		
//		public bool IsFIRST {
//			get {
//				return this.get_backward(0) == null;
//			}
//		}
		
//		public bool IsLAST {
//			get {
//				return this.get_forward(0) == null;
//			}
//		}
		
		public int Level {
			get {
				return this.pointers.Count >> 1;
			}
		}
	}

	public class SkipList2AdaptiveContext<T>
	{
		public bool IsFinger;
		public SkipNode2<T> StartNode;

		public SkipList2AdaptiveContext ()
		{
			this.IsFinger = false;
			this.StartNode = null;
		}

		public SkipList2AdaptiveContext (bool isFinger, SkipNode2<T> startNode)
		{
			this.IsFinger = isFinger;
			this.StartNode = startNode;
		}
	}

	public class SkipList2<T>
	{
		short current_level;
		float prob;
		Comparison<T> cmp_fun;
		public SkipNode2<T> FIRST;
		public SkipNode2<T> LAST;
		Random rand;
		int n;
		
		
		public SkipList2 (double prob, Comparison<T> cmp_fun)
		{
			this.rand = new Random ();
			this.prob = (float)prob;
			this.cmp_fun = cmp_fun;
			this.current_level = 1;
			this.FIRST = new SkipNode2<T> (this.current_level);
			this.LAST = new SkipNode2<T> (this.current_level);
			this.FIRST.set_forward (0, this.LAST);
			this.LAST.set_backward (0, this.FIRST);
			this.n = 0;
		}
		
		public int Count {
			get {
				return this.n;
			}
		}
		
		SkipNode2<T> get_backward (SkipNode2<T> node, int index)
		{
			while (this.FIRST != node) {
				if (node.Level > index)
					break;
				node = node.get_backward (node.Level - 1);
			}
			return node;
		}

		static void set_forward (SkipNode2<T> node, int level, SkipNode2<T> _value)
		{
			node.set_forward (level, _value);
		}

		static void set_backward (SkipNode2<T> node, int level, SkipNode2<T> _value)
		{
			node.set_backward (level, _value);
		}
		
		protected SkipNode2<T> FindNode (T key, SkipNode2<T> s, int level)
		{
			int i;
			for (i = level; i >= 0; i--) {
				while (s.get_forward(i) != this.LAST && this.cmp_fun (s.get_forward(i).data, key) <= 0) {
					s = s.get_forward (i);
				}
			}
			return s;
		}

		protected SkipNode2<T> _FindNodeAdaptive (T key, SkipNode2<T> s, out int level)
		{
			int i;
			for (i = 0; i < this.current_level; i++) {
				do {
					if (s.get_forward(i) == this.LAST || this.cmp_fun (s.get_forward(i).data, key) > 0) {
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
		
		protected SkipNode2<T> _FindNodeAdaptiveReverse (T key, SkipNode2<T> s, out int level)
		{
			int i;
			for (i = 0; i < this.current_level; i++) {
				do {
					if (s.get_backward(i) == this.FIRST || this.cmp_fun (s.get_backward(i).data, key) <= 0) {
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
		
		protected SkipNode2<T> FindNodeAdaptive (T key, SkipNode2<T> s)
		{
			int level;
			s = this._FindNodeAdaptive (key, s, out level);
			return this.FindNode (key, s, level);
		}

		protected SkipNode2<T> FindNodeAdaptiveReverse (T key, SkipNode2<T> s)
		{
			int level;
			s = this._FindNodeAdaptiveReverse (key, s, out level);
			return this.FindNode (key, s, level);
		}

		public SkipNode2<T> Find (T key, SkipList2AdaptiveContext<T> ctx)
		{
			var s = this.FindNode (key, ctx);
			if (s == this.LAST || this.cmp_fun (key, s.data) != 0) {
				throw new KeyNotFoundException (key.ToString ());
			}
			return s;
		}
		
		public bool Contains (T key)
		{
			var s = this.FindNode (key, null);
			if (s == this.LAST || s == this.FIRST || this.cmp_fun (key, s.data) != 0) {
				return false;
			}
			return true;
		}
		
		virtual protected SkipNode2<T> FindNode (T key, SkipList2AdaptiveContext<T> ctx)
		{
			if (ctx == null) {
				return this.FindNode (key, this.FIRST, this.FIRST.Level - 1);
			}
			SkipNode2<T > s;
			if (ctx.IsFinger) {
				if (this.FIRST == ctx.StartNode) {
					s = this.FindNodeAdaptive (key, this.FIRST);
				} else if (this.cmp_fun (ctx.StartNode.data, key) > 0) {
					s = this.FindNodeAdaptiveReverse (key, ctx.StartNode);
				} else {
					s = this.FindNodeAdaptive (key, ctx.StartNode);
				}
				ctx.StartNode = s;
			} else {
				s = this.FindNodeAdaptive (key, this.FIRST);
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
			int level = this.FIRST.Level - 1;
			this.FIRST.Push (this.LAST, null);
			this.LAST.Push (null, this.FIRST);
			var prev = this.FIRST;
			var curr = prev.get_forward (level);
			while (curr != this.LAST) {
				var new_level = random_level ();
				if (new_level > 2) {
					curr.Push (this.LAST, prev);
					prev.set_forward (level + 1, curr);
					this.LAST.set_backward (level + 1, curr);
					prev = curr;
				}
				curr = curr.get_forward (level);
			}
			this.current_level++;
		}
		
		void decrease_log_n ()
		{
			int level = this.FIRST.Level - 1;
			var curr = this.FIRST;
			var next = curr.get_forward (level);
			do {
				curr.Pop ();
				curr = next;
				next = curr.get_forward (level);
			} while (curr != this.LAST);
			curr.Pop ();
			this.current_level--;
			// Console.WriteLine ("**** DECREASE current_level: {0}, n: {1}, first.level: {2}", this.current_level, this.Count, this.FIRST.Level);
			if (this.current_level == 0) {
				this.FIRST.Push (this.LAST, null);
				this.LAST.Push (null, this.FIRST);
				this.current_level = 1;
			}
		}

		public SkipNode2<T> Add (T new_data, SkipList2AdaptiveContext<T> ctx)
		{
			var level = this.random_level ();
			var prev = this.FindNode (new_data, ctx);
			var new_node = new SkipNode2<T> (level);
			new_node.data = new_data;
			// Console.WriteLine ("**** ADD data: {0}, n: {1}, first.level: {2}", new_data, this.Count, this.FIRST.Level);
			for (short k = 0; k < level; k++) {
				SkipNode2<T > forward;
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
				
		public SkipNode2<T> Remove (SkipNode2<T> s, SkipList2AdaptiveContext<T> ctx)
		{
			// Console.WriteLine ("**** DEL data: {0}, n: {1}, first.level: {2}", s.data, this.Count, this.FIRST.Level);
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
		
		public SkipNode2<T> Remove (T key, SkipList2AdaptiveContext<T> ctx)
		{
			var s = this.FindNode (key, ctx);
			if (this.FIRST == s || this.LAST == s || this.cmp_fun (key, s.data) != 0) {
				Console.WriteLine ("s.IsLAST: {0}, s.IsFIRST: {1}, key: {2}, s.data: {3}",
					s == this.LAST, s == this.FIRST, key, s.data);
				throw new KeyNotFoundException ();
			}
			return this.Remove (s, ctx);
		}
		
		public T GetFirst ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.FIRST.get_forward (0).data;
			}
		}

		public T RemoveFirst ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.FIRST.get_forward(0), null).data;
			}
		}

		public SkipNode2<T> RemoveFirstNode ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.FIRST.get_forward(0), null);
			}
		}

		public T GetLast ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.LAST.get_backward (0).data;
			}
		}

		public T RemoveLast ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.LAST.get_backward(0), null).data;
			}
		}

		public SkipNode2<T> RemoveLastNode ()
		{
			if (this.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty SkipList");
			} else {
				return this.Remove (this.FIRST.get_backward(0), null);
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
			var s = this.FIRST.get_forward (0);
			int i = 0;
			w.Write ("(n: {0}) ", this.n);
			w.Write ("{ ");
			while (s != LAST) {
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
	
		public IEnumerable<SkipNode2<T>> TraverseNodes ()
		{
			if (this.Count > 0) {
				var s = this.FIRST.get_forward (0);
				while (s != this.LAST) {
					yield return s;
					s = s.get_forward (0);
				}
			}
		}
		
		public IEnumerable< SkipNode2<T> > ReversalTraverseNodes ()
		{
			if (this.Count > 0) {
				var s = this.LAST.get_backward (0);
				while (s != this.FIRST) {
					yield return s;
					s = s.get_backward (0);
				}			
			}
		}
	}
}
