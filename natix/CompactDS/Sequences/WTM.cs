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
	/// A wavelet node (implementation with pointers/references)
	/// </summary>
	public class WT_M : IRankSelectSeq
	{
		// static INumericManager<T> Num = (INumericManager<T>)NumericManager.Get (typeof(T));
		IIEncoder32 Coder = null;
		// BitStream32 CoderStream;
		WTM_Inner Root;
		IList<WTM_Leaf> Alphabet;
		short bits_per_symbol;

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
	
		public WT_M ()
		{
		}
		
		public void Build (IIEncoder32 coder, int alphabet_size, short bits_per_symbol, IList<int> text, SequenceBuilder seq_builder = null)
		{
			this.bits_per_symbol = bits_per_symbol;
			short arity = (short)(1 << bits_per_symbol);
			this.Alphabet = new WTM_Leaf[alphabet_size];
			this.Root = new WTM_Inner (arity, null, true);
			this.Coder = coder;
			for (int i = 0; i < text.Count; i++) {
				this.Add (text [i]);
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
			node.SEQ = seq_builder ((node.SEQ as FakeSeq).SEQ, sigma);
			foreach (var child in node.CHILDREN) {
				this.FinishBuild(child as WTM_Inner, seq_builder, sigma);
			}
		}
		
		protected void Add (int symbol)
		{
			var coderstream = new BitStream32 ();
			coderstream.Clear ();
			this.Coder.Encode(coderstream, symbol);
			var ctx = new BitStreamCtx (0);
			// this.CoderStream.Seek (0);
			int numbits = (int)coderstream.CountBits;
			var node = this.Root;
			for (int b = 0; b < numbits; b+=this.bits_per_symbol) {
				//var bitcode = coderstream.Read (ctx);
				int code = (int)coderstream.Read (this.bits_per_symbol, ctx);
				(node.SEQ as FakeSeq).Add (code);
				if (numbits == b + this.bits_per_symbol) {
					var leaf = node.CHILDREN[code] as WTM_Leaf;
					if (leaf == null) {
						leaf = new WTM_Leaf(node, symbol);
						this.Alphabet [symbol] = leaf;
					} else {
						leaf.Increment();
					}
					node.CHILDREN[code] = leaf;
					break;
				} else {
					var inner = node.CHILDREN[code] as WTM_Inner;
					if (inner == null) {
						inner = new WTM_Inner((short)node.CHILDREN.Length, node, true);
					} else {
						node.CHILDREN[code] = inner;
					}
					node = inner;
				}
			}
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.Alphabet.Count);
			Output.Write ((short)this.bits_per_symbol);
			IEncoder32GenericIO.Save (Output, this.Coder);
			// Console.WriteLine ("Output.Position: {0}", Output.BaseStream.Position);
			this.SaveNode (Output, this.Root);
		}

		public void Load (BinaryReader Input)
		{
			var size = Input.ReadInt32 ();
			this.bits_per_symbol = Input.ReadInt16();
			this.Alphabet = new WTM_Leaf[size];
			this.Coder = IEncoder32GenericIO.Load (Input);
			// Console.WriteLine ("Input.Position: {0}", Input.BaseStream.Position);
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
				Output.Write ((int)asLeaf.Count);
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
				var count = Input.ReadInt32 ();
				var symbol = Input.ReadInt32 ();
				// Console.WriteLine ("--leaf> count: {0}, symbol: {1}", count, symbol);
				var leaf = new WTM_Leaf (parent, symbol, count);
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

		public List<int> GetMiniString (int symbol)
		{
			var coderstream = new BitStream32 ();
			this.Coder.Encode (coderstream, symbol);
			int numbits = (int) coderstream.CountBits;
			var ctx = new BitStreamCtx (0);
			var ministring = new List<int>();
			for (int i = 0; i < numbits; i+= this.bits_per_symbol) {
				int code = (int)coderstream.Read (this.bits_per_symbol, ctx);
				ministring.Add(code);
			}
			return ministring;
		}

		public int Rank (int symbol, int position)
		{
			var ministring = this.GetMiniString(symbol);
			var node = this.Root;
			var mlen = ministring.Count;
			for (int i = 0; i < mlen; ++i) {
				var code = ministring[i];
				position = node.SEQ.Rank (code, position) - 1;
				if (i+1 < mlen) {
					node = node.CHILDREN[code] as WTM_Inner;
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
			if (symnode == null || symnode.Count < rank) {
				return -1;
			}
			WTM_Inner node = symnode.Parent as WTM_Inner;
			var ministring = this.GetMiniString (symbol);
			ministring.Reverse();
			var mlen = ministring.Count;
			for (int i = 0; i < mlen; i++) {
				var code = ministring[i];
				rank = node.SEQ.Select(code, rank) + 1;
				node = node.Parent as WTM_Inner;
			}
			return rank - 1;
		}

		public int Access (int position)
		{
			var node = this.Root;
			WTM_Inner tmp;
			for (int i = 0; true; i++) {
				var code = node.SEQ.Access(position);
				position = node.SEQ.Rank (code, position) - 1;
				tmp = node.CHILDREN[code] as WTM_Inner;
				if (tmp == null) {
					return (node.CHILDREN[code] as WTM_Leaf).Symbol;
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

			public WTM_Inner (short arity, WTM_Inner parent, bool building) : base(parent)
			{
				if (building) {
					this.SEQ = new FakeSeq();
				}
				this.CHILDREN = new WTM_Node[ arity ];
			}			
		}
		
		public class WTM_Leaf : WTM_Node
		{
			public int Count;
			public int Symbol;
			
			public WTM_Leaf (WTM_Inner parent, int symbol) : base(parent)
			{
				this.Count = 1;
				this.Symbol = symbol;
			}
			
			public WTM_Leaf (WTM_Inner parent, int symbol, int count) : base(parent)
			{
				this.Count = count;
				this.Symbol = symbol;
			}
			
			public void Increment ()
			{
				this.Count++;
			}
		}
	}

}