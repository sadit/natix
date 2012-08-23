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
//   Original filename: natix/SortingSearching/SplayTree.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	public class SplayNode<T>
	{
		public SplayNode<T> Left;
		public SplayNode<T> Right;
		public SplayNode<T> Parent;
		public T Data;
		
		public SplayNode (T d, SplayNode<T> L, SplayNode<T> R, SplayNode<T> P)
		{
			this.Data = d;
			this.Left = L;
			this.Right = R;
			this.Parent = P;
		}
		
		public bool IsLeftChild ()
		{
			// we suppose we are not the root
			if (this.Parent == null) {
				return true;
			}
			return this.Parent.Left == this;
		}

		public bool IsRightChild ()
		{
			// we suppose we are not the root
			return !IsLeftChild ();
		}

		public string ToString (bool justData)
		{
			string L = "null";
			if (this.Left != null) {
				L = this.Left.Data.ToString ();
			}
			string R = "null";
			if (this.Right != null) {
				R = this.Right.Data.ToString ();
			}
			string P = "null";
			if (this.Parent != null) {
				P = this.Parent.Data.ToString ();
			}
			return String.Format ("(L: {0}) <- (D: {1}, P: {3}) -> (R: {2})", L , this.Data, R, P);
		}

		public override string ToString ()
		{
			string P = "null";
			if (this.Parent != null) {
				P = this.Parent.Data.ToString ();
			}
			return String.Format ("(L: {0}) <- (D: {1}, P: {3}) -> (R: {2})", this.Left, this.Data, this.Right, P);
		}

		public SplayNode<T> GetFirst ()
		{
			var S = this;
			while (S.Left != null) {
				S = S.Left;
			}
			return S;
		}
		public SplayNode<T> GetLast ()
		{
			var S = this;
			while (S.Right != null) {
				S = S.Right;
			}
			return S;
		}

		void SetAsChildOfParent (SplayNode<T> original)
		{
			if (this.Parent != null) {
				if (original.IsLeftChild ()) {
					this.Parent.Left = this;
				} else {
					this.Parent.Right = this;
				}
			}
		}

		SplayNode<T> RemoveMinOfMax ()
		{
			var m = this.GetFirst ();
			if (m.Right == null) {
				if (m.Parent != null) {
					if (m.IsLeftChild ()) {
						m.Parent.Left = null;
					} else {
						m.Parent.Right = null;
					}
				}
			} else {
				// maintaining structure
				m.Right.Parent = m.Parent;
				m.Right.SetAsChildOfParent (m);				
			}
			return m;
		}
		
		SplayNode<T> ReplaceByChildAndSplay (SplayNode<T> replacement)
		{
			replacement.Parent = this.Parent;
			replacement.SetAsChildOfParent (this);
			replacement.Splay ();
			return replacement;
		}
		
		SplayNode<T> RemoveLeafAndSplayParent ()
		{
			var p = this.Parent;
			if (p != null) {
				if (this.IsLeftChild ()) {
					p.Left = null;
				} else {
					p.Right = null;
				}
				p.Splay ();
			}
			return p;
		}
		
		public SplayNode<T> RemoveAndSplay ()
		{
			// Console.WriteLine ("====> Data: {0}, LeftNull: {1}, RightNull: {2}", this.Data, this.Left == null, this.Right == null);
			if (this.Right == null) {
				if (this.Left == null) {
					return this.RemoveLeafAndSplayParent ();
				} else {
					return this.ReplaceByChildAndSplay (this.Left);
				}
			}
			if (this.Left == null) {
				if (this.Right == null) {
					return this.RemoveLeafAndSplayParent ();
				} else {
					return this.ReplaceByChildAndSplay (this.Right);
				}
			}
			var r = this.Right.RemoveMinOfMax ();
			r.Parent = this.Parent;
			r.SetAsChildOfParent (this);
			// this is for convenience
			this.Parent = r;
			r.Left = this.Left;
			if (r.Left != null) {
				r.Left.Parent = r;
			}
			r.Right = this.Right;
			if (r.Right != null) {
				r.Right.Parent = r;
			}
			r.Splay ();
			return r;
		}
		
		public void RotateLeft ()
		{
			SplayNode<T> _Parent = this.Parent;
			SplayNode<T> _Right = this.Right;
			// this.Right should be not null
			if (_Parent != null) {
				if (this.IsLeftChild ()) {
					_Parent.Left = _Right;
				} else {
					_Parent.Right = _Right;
				}
			}
			_Right.Parent = _Parent;
			this.Right = _Right.Left;
			if (_Right.Left != null) {
				_Right.Left.Parent = this;
			}
			_Right.Left = this;
			this.Parent = _Right;
		}

		public void RotateRight ()
		{
			SplayNode<T> _Parent = this.Parent;
			SplayNode<T> _Left = this.Left;
			// this.Left should be not null
			if (_Parent != null) {
				if (this.IsRightChild ()) {
					_Parent.Right = _Left;
				} else {
					_Parent.Left = _Left;
				}
			}
			_Left.Parent = _Parent;
			this.Left = _Left.Right;
			if (_Left.Right != null) {
				_Left.Right.Parent = this;
			}
			_Left.Right = this;
			this.Parent = _Left;
		}
	
		public void PrintTree ()
		{
			var S = this;
			while (S.Parent != null) {
				S = S.Parent;
			}
			Console.WriteLine ("****** PRINT - TREE: {0}", S.ToString ());
		}
		
		public void Splay ()
		{
			while (this.Parent != null) {
				// Console.WriteLine ("****** INNER ITERATION - DATA: {0}", this.ToString (true));
				// this.PrintTree (); 				
				if (this.Parent.Parent == null) {
					if (this.IsLeftChild ()) {
						// zig
						// Console.WriteLine ("XXXXXX zig");
						// this.PrintTree ();
						this.Parent.RotateRight ();
					} else {
						// zag
						// Console.WriteLine ("XXXXXX zag");
						// this.PrintTree ();
						this.Parent.RotateLeft ();
					}
					break;
				}
				if (this.IsLeftChild ()) {
					if (this.Parent.IsLeftChild ()) {
						// zig-zig
						this.Parent.Parent.RotateRight ();
						this.Parent.RotateRight ();
					} else {
						// zig-zag
						// p = LeftChild
						// p.p = RightChild
						this.Parent.RotateRight ();
						this.Parent.RotateLeft ();
					}
				} else {
					if (this.Parent.IsLeftChild ()) {
						// zig-zag
						// p = RightChild
						// p.p = LeftChild
						this.Parent.RotateLeft ();
						this.Parent.RotateRight ();
					} else {
						// zig-zig
						this.Parent.Parent.RotateLeft ();
						this.Parent.RotateLeft ();
					}
				}
			}
			// Console.WriteLine ("======> COMPLETE SPLAY - TREE: {0}", this);
		}
		public void Walk (Func<T, int> preorder, Func<T, int> inorder, Func<T, int> postorder)
		{
			if (preorder != null) {
				preorder (this.Data);
			}
			if (this.Left != null) {
				this.Left.Walk (preorder, inorder, postorder);
			}
			if (inorder != null) {
				inorder (this.Data);
			}
			if (this.Right != null) {
				this.Right.Walk (preorder, inorder, postorder);
			}
			if (postorder != null) {
				postorder (this.Data);
			}
		}
	}
	
	public class SplayTree<T> : ICollection<T>
	{
		SplayNode<T> Root;
		Comparison<T> Cmp;
		public int count;
		
		public int Count {
			get {
				return count;
			}
		}
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		public void Clear ()
		{
			this.Root = null;
			this.count = 0;
		}
		
		public void CopyTo (T[] array, int arrayIndex)
		{
			throw new NotSupportedException ();
		}

		public IEnumerator<T> GetEnumerator ()
		{
			List<T> L = new List<T> (this.Count);
			if (this.Root != null) {
				this.Root.Walk (null, (T d) =>
				{
					L.Add (d);
					return 0;
				}, null);
			}
			return L.GetEnumerator ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((SplayTree<T>)this).GetEnumerator ();
		}
		
		public void Walk (Func<T, int> preorder, Func<T, int> inorder, Func<T, int> postorder)
		{
			if (this.Root != null) {
				this.Root.Walk (preorder, inorder, postorder);
			}
		}

		public SplayTree (Comparison<T> cmp)
		{
			this.Cmp = cmp;
			this.count = 0;
			this.Root = null;
		}
		
		public void Add (T data)
		{
			var N = new SplayNode<T> (data, null, null, null);
			this.count++;
			if (this.Root == null) {
				this.Root = N;
				return;
			}
			int cmp;
			var S = this.FindNode (data, out cmp);
			if (cmp == 0) {
				N.Parent = S;
				N.Left = S.Left;
				S.Left = N;
				if (N.Left != null) {
					N.Left.Parent = N;
				}				
			} else if (cmp < 0) {
				S.Left = N;
			} else {
				S.Right = N;
			}
			N.Parent = S;
			N.Splay ();
			this.Root = N;
		}
		
		public bool Contains (T data)
		{
			if (this.Root == null) {
				return false;
			}
			int cmp;
			var S = this.FindNode (data, out cmp);
			S.Splay ();
			this.Root = S;
			return cmp == 0;
		}

		public bool Remove (T data)
		{
			int cmp;
			if (this.Root == null) {
				return false;
			}
			var S = this.FindNode (data, out cmp);
			if (cmp == 0) {
				this.Root = S.RemoveAndSplay ();
				this.count--;
				return true;
			}
			return false;
		}

		public T RemoveFirst ()
		{
			if (this.Root == null) {
				throw new ArgumentOutOfRangeException ("Empty SplayTree");
			}
			var R = this.Root.GetFirst ();
			this.Root = R.RemoveAndSplay ();
			this.count--;
			return R.Data;
		}

		public T RemoveLast ()
		{
			if (this.Root == null) {
				throw new ArgumentOutOfRangeException ("Empty SplayTree");
			}
			var R = this.Root.GetLast ();
			this.Root = R.RemoveAndSplay ();
			this.count--;
			return R.Data;
		}

		SplayNode<T> FindNode (T data, out int CMP)
		{
			var S = this.Root;
			int cmp = 0;
			while (true) {
				// cmp = data.CompareTo(S.Data);
				cmp = this.Cmp (data, S.Data);
				CMP = cmp;
				if (cmp == 0) {
					break;
				} else if (cmp < 0) {
					if (S.Left == null) {
						break;
					} else {
						S = S.Left;
					}
				} else {
					if (S.Right == null) {
						break;
					} else {
						S = S.Right;
					}
				}
			}
			return S;
		}
		
		public T GetLastWithSplay ()
		{
			if (this.Root == null) {
				throw new ArgumentOutOfRangeException ("Empty SplayTree");
			}
			var N = this.Root.GetLast ();
			N.Splay ();
			this.Root = N;
			return N.Data;
		}
		
		public T GetLast ()
		{
			if (this.Root == null) {
				throw new ArgumentOutOfRangeException ("Empty SplayTree");
			}
			return this.Root.GetLast ().Data;
		}

		public T GetFirstWithSplay ()
		{
			if (this.Root == null) {
				throw new ArgumentOutOfRangeException ("Empty SplayTree");
			}
			var N = this.Root.GetFirst ();
			N.Splay ();
			this.Root = N;
			return N.Data;
		}

		public T GetFirst ()
		{
			if (this.Root == null) {
				throw new ArgumentOutOfRangeException ("Empty SplayTree");
			}
			return this.Root.GetFirst ().Data;
		}
				
		public override string ToString ()
		{
			return string.Format("[SplayTree: {0}]", this.Root);
		}
	}
}
