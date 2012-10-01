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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// A multiary wavelet tree (implementation with pointers/references)
	/// </summary>
	public class WTM : IRankSelectSeq
	{
		// static INumericManager<T> Num = (INumericManager<T>)NumericManager.Get (typeof(T));
		ISymbolCoder SymbolCoder;
		// BitStream32 CoderStream;
		WTM_Inner Root;
		IList<WTM_Leaf> Alphabet;
		
		public int Sigma {
			get {
				return this.Alphabet.Count;
			}
		}
		
		public int N {
			get {
				return this.Root.SEQ.Count;
			}
		}
		
		public WTM ()
		{
		}

		public void Build (IList<int> text, int alphabet_size, ISymbolCoder symbol_split = null, SequenceBuilder seq_builder = null)
		{
			if (symbol_split == null) {
				symbol_split = new EqualSizeCoder(4, alphabet_size-1);
			}
			this.SymbolCoder = symbol_split;
			var list = this.SymbolCoder.Encode(0, null);
			var numbits = list[0].numbits;
			var arity = (short)(1 << numbits);
			this.Alphabet = new WTM_Leaf[alphabet_size];
			this.Root = new WTM_Inner (arity, null, true);
			for (int i = 0; i < text.Count; i++) {
				this.Add (text [i], list);
			}
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqPlain(arity);
			}
			this.FinishBuild (this.Root, seq_builder, arity);
		}
		
		void FinishBuild (WTM_Inner node, SequenceBuilder seq_builder, int sigma)
		{
			if (node == null) {
				return;
			}
			var s = node.SEQ as FakeSeq;
			node.SEQ = seq_builder (s.SEQ, s.Sigma);
			foreach (var child in node.CHILDREN) {
				this.FinishBuild(child as WTM_Inner, seq_builder, sigma);
			}
		}
		
		protected void Add (int symbol, List<WTM_Symbol> list)
		{
			list.Clear();
			list = this.SymbolCoder.Encode (symbol, list);
			var node = this.Root;
			var plen = list.Count;
			for (int i = 0; i < plen; ++i) {
				var code = list[i];
				(node.SEQ as FakeSeq).Add (code.symbol);
				if (i+1 == plen) {
					var leaf = node.CHILDREN[code.symbol] as WTM_Leaf;
					if (leaf == null) {
						leaf = new WTM_Leaf(node, symbol);
						this.Alphabet[symbol] = leaf;
//					} else {
//						leaf.Increment();
					}
					node.CHILDREN[code.symbol] = leaf;
				} else {
					var inner = node.CHILDREN[code.symbol] as WTM_Inner;
					if (inner == null) {
						//inner = new WTM_Inner((short)node.CHILDREN.Length, node, true);
						inner = new WTM_Inner(1 << code.numbits, node, true);
					}
					node.CHILDREN[code.symbol] = inner;
					node = inner;
				}
			}
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.Alphabet.Count);
			SymbolCoderGenericIO.Save(Output, this.SymbolCoder);
			// Console.WriteLine ("Output.Position: {0}", Output.BaseStream.Position);
			this.SaveNode (Output, this.Root);
		}
		
		public void Load (BinaryReader Input)
		{
			var size = Input.ReadInt32 ();
			this.SymbolCoder = SymbolCoderGenericIO.Load (Input);
			this.Alphabet = new WTM_Leaf[size];
			this.Root = this.LoadNode (Input, null) as WTM_Inner;
		}
		
		void SaveNode (BinaryWriter Output, WTM_Node node)
		{
			var asInner = node as WTM_Inner;
			if (asInner != null) {
				// isInner?
				Output.Write (true);
				var arity = asInner.CHILDREN.Length;
				Output.Write ((short)arity);
				RankSelectSeqGenericIO.Save(Output, asInner.SEQ);
				for (int i = 0; i < arity; ++i) {
					var child = asInner.CHILDREN[i];
					if (child == null) {
						Output.Write (false);
					} else {
						Output.Write(true);
						this.SaveNode(Output, child);
					}
				}
			} else {
				Output.Write (false);
				var asLeaf = node as WTM_Leaf;
				// Output.Write ((int)asLeaf.Count);
				Output.Write ((int)asLeaf.Symbol);
			}
		}
		
		WTM_Node LoadNode (BinaryReader Input, WTM_Inner parent)
		{
			// Console.WriteLine ("xxxxxxxxx LoadNode");
			var isInner = Input.ReadBoolean ();
			if (isInner) {
				var arity = Input.ReadInt16 ();
				var node = new WTM_Inner (arity, parent, false);
				node.SEQ = RankSelectSeqGenericIO.Load(Input);
				node.CHILDREN = new WTM_Node[arity];
				for (int i = 0; i < arity; ++i) {
					if (Input.ReadBoolean()) {
						node.CHILDREN[i] = this.LoadNode(Input, node);
					}
				}
				return node;
			} else {
				// var count = Input.ReadInt32 ();
				var symbol = Input.ReadInt32 ();
				// Console.WriteLine ("--leaf> count: {0}, symbol: {1}", count, symbol);
				//var leaf = new WTM_Leaf (parent, symbol, count);
				var leaf = new WTM_Leaf (parent, symbol);
				this.Alphabet[symbol] = leaf;
				return leaf;
			}
		}
		
		void Walk (WTM_Inner node,
		           Func<WTM_Inner, object> preorder,
		           //Func<WInner, object> inorder,
		           Func<WTM_Inner, object> postorder)
		{
			if (node == null) {
				return;
			}
			if (preorder != null) {
				preorder (node);
			}
			foreach (var child in node.CHILDREN) {
				this.Walk (child as WTM_Inner, preorder, postorder);
			}
			/*if (inorder != null) {
				inorder (node);
			}*/
			
			if (postorder != null) {
				postorder (node);
			}
		}
		
		public int Count {
			get { return (int)this.Root.SEQ.Count; }
		}
		
		public int this[int index] {
			get { return this.Access (index); }
		}
		
		public int Rank (int symbol, int position)
		{
			if (position < 0) {
				return 0;
			}
			var ministring = this.SymbolCoder.Encode(symbol, null);
			var node = this.Root;
			var mlen = ministring.Count;
			for (int i = 0; i < mlen; ++i) {
				var code = ministring[i];
				try {
					position = node.SEQ.Rank (code.symbol, position) - 1;
				} catch (Exception e) {
					Console.WriteLine("i: {0}, position: {1}, code: {2}, mlen: {3}, sigma: {4}, len-seq: {5}", i, position, code, mlen, node.SEQ.Sigma, node.SEQ.Count);
					throw e;
				}
				if (i+1 < mlen) {
					node = node.CHILDREN[code.symbol] as WTM_Inner;
				}
				if (node == null) {
					return 0;
				}
			}
			return position + 1;
		}
		
		public int Select (int symbol, int rank)
		{
			var symnode = this.Alphabet [symbol];
			//if (symnode == null || symnode.Count < rank) {
			if (symnode == null || rank == 0) {
				return -1;
			}
			WTM_Inner node = symnode.Parent as WTM_Inner;
			var ministring = this.SymbolCoder.Encode (symbol, null);
			ministring.Reverse();
			var mlen = ministring.Count;
			for (int i = 0; i < mlen; i++) {
				var code = ministring[i];
				rank = node.SEQ.Select(code.symbol, rank) + 1;
				node = node.Parent as WTM_Inner;
			}
			return rank - 1;
		}
		
		public int Access (int position)
		{
			// Console.WriteLine("=== Access position: {0}", position);
			var node = this.Root;
			WTM_Inner tmp;
			var codes = new List<int>();
			for (int i = 0; true; i++) {
				var code = node.SEQ.Access(position);
				codes.Add(code);
				position = node.SEQ.Rank (code, position) - 1;
				tmp = node.CHILDREN[code] as WTM_Inner;
				if (tmp == null) {
					var symcode = this.SymbolCoder.Decode(codes);
					// var sym = (node.CHILDREN[code] as WTM_Leaf).Symbol;
					// Console.WriteLine ("   symbol: {0}", sym);
					// return sym;
					// if (sym != symcode) {
					//	throw new Exception("symcode and sym are not equal");
					// }
					return symcode;
				} else {
					node = tmp;
				}
			}
		}
		
		public IRankSelect Unravel (int symbol)
		{
			return new UnraveledSymbol (this, symbol);
		}
		
		//  Helping classes
		public class WTM_Node
		{
			public WTM_Node Parent;
			public WTM_Node (WTM_Node parent)
			{
				this.Parent = parent;
			}
		}
		
		public class WTM_Inner : WTM_Node
		{
			public IRankSelectSeq SEQ;
			public WTM_Node[] CHILDREN;
			
			public WTM_Inner (int arity, WTM_Inner parent, bool building) : base(parent)
			{
				if (building) {
					this.SEQ = new FakeSeq(arity);
				}
				this.CHILDREN = new WTM_Node[ arity ];
			}			
		}
		
		public class WTM_Leaf : WTM_Node
		{
			// public int Count;
			public int Symbol;
			
			public WTM_Leaf (WTM_Inner parent, int symbol) : base(parent)
			{
				// this.Count = 1;
				this.Symbol = symbol;
			}
			
			/*public WTM_Leaf (WTM_Inner parent, int symbol, int count) : base(parent)
			{
				// this.Count = count;
				this.Symbol = symbol;
			}*/
//			
//			public void Increment ()
//			{
//				// this.Count++;
//			}
		}
	}	
}