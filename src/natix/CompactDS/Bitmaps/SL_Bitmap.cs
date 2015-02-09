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
//   Original filename: natix/CompactDS/Bitmaps/SL_Bitmap.cs
// 
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//
//namespace natix.CompactDS
//{
//	public class SL_BitmapNode
//	{
//		public int p;
//		public int r;
//		public List<int> stream;
//		
//		public SL_BitmapNode (int current_n)
//		{
//			this.p = 0;
//			this.r = 0;
//			// int lg = 1 + (int)Math.Ceiling (Math.Log (current_n + 1, 2));
//			this.stream = null;
//			// new List<int> ();
//		}
//	}
//	
//	public class SL_Bitmap : RankSelectBase
//	{
//		float prob;
//		// Comparison<T> cmp_fun;
//		public SkipNode<SL_BitmapNode> FIRST;
//		public SkipNode<SL_BitmapNode> LAST;
//		Random rand;
//		int n;
//
//		public SL_Bitmap (float prob)
//		{
//			this.rand = new Random ();
//			this.prob = prob;
//			// this.max_level = (short)Math.Ceiling (Math.Log (expected_n, 2) / Math.Log (1.0 / this.prob, 2));
//	
//			this.FIRST = new SkipNode<SL_BitmapNode> (true);
//			this.LAST = new SkipNode<SL_BitmapNode> (false);
//			this.FIRST.forward.Add (this.LAST);
//			this.n = 0;
//		}
//		
//		public override int Count {
//			get {
//				return this.n;
//			}
//		}
//		
////		public SkipNode<T> Find (T key)
////		{
////			var s = this.FindNode (key);
////			if (s.IsLAST || this.cmp_fun (key, s.data) != 0) {
////				throw new KeyNotFoundException (key.ToString ());
////			}
////			return s;
////		}
//		
//		protected SkipNode<SL_BitmapNode> FindNodePos (int POS, out int relpos)
//		{
//			var s = this.FIRST;
//			int accpos = 0;
//			for (int i = this.FIRST.Level - 1; i >= 0; i--) {
//				while (!s.forward[i].IsLAST && accpos + s.forward[i].data.p <= POS) {
//					accpos += s.forward [i].data.p;
//					s = s.forward [i];
//				}
//			}
//			relpos = accpos;
//			return s;
//		}
//
//		short GetRandomLevel()
//		{
//			short l = 1;
//			while (this.prob < this.rand.NextDouble() && l < this.FIRST.Level) {
//				l++;
//			}
//			return l;
//		}
//
//		public SkipNode<SL_BitmapNode> InsertAt (bool bit, int pos)
//		{
//			int i;
//			var s = this.FIRST;
//			if (this.FIRST.Level < Math.Ceiling (Math.Log (this.n + 1, 2))) {
//				this.FIRST.PushForwards (1, this.LAST);
//			}
//			var new_level = this.GetRandomLevel ();
//			var new_data = new SL_BitmapNode (this.Count);
//			var new_node = new SkipNode<SL_BitmapNode> (new_data);
//			new_node.PushForwards (new_level, null);
//			int accpos = 0;
//			for (i = this.FIRST.Level - 1; i >= 0; i--) {
//				int cmp = -1;
//				while (!s.forward[i].IsLAST) {
//					if (s.forward [i].data.p + accpos < pos) {
//						accpos += s.forward [i].data.p;
//						s = s.forward [i];
//					} else {
//						break;
//					}
//				}
//				if (i < new_level) {
//					new_node.forward [i] = s.forward [i];
//					s.forward [i] = new_node;
//				}
//			}
//			this.n++;
//			new_node.data.p = pos - accpos;
//			// new_node.data.r = new_rank_delta;
//			
//			while () {
//				
//			}
//			fix_forward_p_R ();
//			return new_node;
//		}
//				
//		public SkipNode<SL_BitmapNode> Remove (int POS)
//		{
//			int i;
//			bool deleted = false;
//			SkipNode< T > s = this.FIRST;
//			for (i = this.FIRST.Level - 1; i >= 0; i--) {
//				int cmp = -1;
//				while (!s.forward[i].IsLAST) {
//					cmp = this.cmp_fun (s.forward [i].data, key);
//					if (cmp < 0) {
//						s = s.forward [i];
//					} else {
//						break;
//					}
//				}
//				if (cmp == 0) {
//					s.forward [i] = s.forward [i].forward [i];
//					deleted = true;
//				}
//			}
//			if (deleted) {
//				this.n--;
//				if (this.FIRST.Level > Math.Ceiling (Math.Log (this.n, 2))) {
//					if (this.FIRST.Level > 1) {
//						this.FIRST.PopForwards ();
//					}
//				}
//				return s;
//			} else {
//				throw new KeyNotFoundException ();
//			}
//
//		}
//				
//		public SL_BitmapNode GetItem (int index)
//		{
//			int i = 0;
//			if (i >= this.n) {
//				throw new ArgumentOutOfRangeException (String.Format("i: {0} should be less than n: {1}", i, this.n));
//			}
//			foreach (var u in this.Traverse ()) {
//				if (i == index) {
//					return u;
//				}
//				i++;
//			}
//			throw new ArgumentOutOfRangeException ("- error it must not reach this!! -");
//		}
//		
//		public override string ToString ()
//		{
//			var w = new StringWriter ();
//			var s = this.FIRST.forward[0];
//			int i = 0;
//			w.Write ("(n: {0}) ", this.n);
//			w.Write ("{ ");
//			while (!s.IsLAST) {
//				w.Write ("(i: {0}, level: {1}, data: {2}), ", i, s.Level, s.data);
//				i++;
//				s = s.forward[0];
//			}
//			w.WriteLine ("<end>}");
//			return w.ToString ();
//		}
//
////		public IEnumerable<T> Traverse ()
////		{
////			foreach (var u in this.TraverseNodes ()) {
////				yield return u.data;
////			}
////		}
////
////		public IEnumerable<SkipNode<T>> TraverseNodes ()
////		{
////			// Console.WriteLine ("****=====> this.Count: {0}", this.Count);
////			if (this.Count > 0) {
////				var s = this.FIRST.forward [0];
////				while (!s.IsLAST) {
////					yield return s;
////					s = s.forward [0];
////				}
////			}
////		}
//	}
//}
