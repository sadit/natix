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
//   Original filename: natix/CompactDS/Sequences/WaveletTree.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// A wavelet tree (implementation with pointers/references)
	/// </summary>
	public class WaveletTree : IRankSelectSeq
	{
		// static INumericManager<T> Num = (INumericManager<T>)NumericManager.Get (typeof(T));
		IIEncoder32 Coder = null;
		// BitStream32 CoderStream;
		WT_Inner Root;
		IList<WT_Leaf> Alphabet;
		
		public int Sigma {
			get {
				return this.Alphabet.Count;
			}
		}
		
		public int N {
			get {
				return this.Root.B.Count;
			}
		}
		
		public BitmapFromBitStream BitmapBuilder {
			get;
			set;
		}
		
		public WaveletTree ()
		{
			// this.CoderStream = new BitStream32 (8);
			this.BitmapBuilder = BitmapBuilders.GetGGMN_wt (16);
		}
		
		public void Build (IList<int> text, int alphabet_size, IIEncoder32 coder = null)
		{
			this.Alphabet = new WT_Leaf[alphabet_size];
			this.Root = new WT_Inner (null, true);
			if (coder == null) {
				coder = new BinaryCoding(ListIFS.GetNumBits(alphabet_size-1));
			}
			this.Coder = coder;
			for (int i = 0; i < text.Count; i++) {
				this.Add (text [i]);
			}
			this.FinishBuild (this.Root);
		}

		void FinishBuild (WT_Inner node)
		{
			if (node == null) {
				return;
			}
			node.B = this.BitmapBuilder(node.B as FakeBitmap);
			this.FinishBuild (node.Left as WT_Inner);
			this.FinishBuild (node.Right as WT_Inner);
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
			for (int b = 0; b < numbits; b++) {
				var bitcode = coderstream.Read (ctx);
				(node.B as FakeBitmap).Write (bitcode);
				if (bitcode) {
					if (numbits == b + 1) {
						if (node.Right == null) {
							var leaf = new WT_Leaf (node, symbol);
							// this.Alphabet.Add (leaf);
							this.Alphabet [symbol] = leaf;
							node.Right = leaf;
						} else {
							(node.Right as WT_Leaf).Increment ();
						}
					} else {
						if (node.Right == null) {
							node.Right = new WT_Inner (node, true);
						}
						node = node.Right as WT_Inner;
					}
				} else {
					if (numbits == b + 1) {
						if (node.Left == null) {
							var leaf = new WT_Leaf (node, symbol);
							// this.Alphabet.Add (leaf);
							this.Alphabet [symbol] = leaf;
							node.Left = leaf;
						} else {
							(node.Left as WT_Leaf).Increment ();
						}
					} else {
						if (node.Left == null) {
							node.Left = new WT_Inner (node, true);
						}
						node = node.Left as WT_Inner;
					}
				}
			}
		}
		
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.Alphabet.Count);
			IEncoder32GenericIO.Save (Output, this.Coder);
			// Console.WriteLine ("Output.Position: {0}", Output.BaseStream.Position);
			this.SaveNode (Output, this.Root);
		}
		
		void SaveNode (BinaryWriter Output, WT_Node node)
		{
			var asInner = node as WT_Inner;
			if (asInner != null) {
				// isInner?
				Output.Write (true);
				RankSelectGenericIO.Save (Output, asInner.B);
				// since it uses pointers the extra-space by booleans is a better reflect of the
				// memory usage than a compact representation of the node
				if (asInner.Left == null) {
					Output.Write (false);
				} else {
					Output.Write (true);
					SaveNode (Output, asInner.Left);
				}
				if (asInner.Right == null) {
					Output.Write (false);
				} else {
					Output.Write (true);
					SaveNode (Output, asInner.Right);
				}
			} else {
				Output.Write (false);
				var asLeaf = node as WT_Leaf;
				Output.Write ((int)asLeaf.Count);
				Output.Write ((int)asLeaf.Symbol);
			}
		}
		
		public void Load (BinaryReader Input)
		{
			var size = Input.ReadInt32 ();
			this.Alphabet = new WT_Leaf[size];
			this.Coder = IEncoder32GenericIO.Load (Input);
			// Console.WriteLine ("Input.Position: {0}", Input.BaseStream.Position);
			this.Root = this.LoadNode (Input, null) as WT_Inner;
		}
		
		WT_Node LoadNode (BinaryReader Input, WT_Inner parent)
		{
			// Console.WriteLine ("xxxxxxxxx LoadNode");
			var isInner = Input.ReadBoolean ();
			if (isInner) {
				var node = new WT_Inner (parent, false);
				node.B = RankSelectGenericIO.Load (Input);
				var hasLeft = Input.ReadBoolean ();
				if (hasLeft) {
					node.Left = this.LoadNode (Input, node);
				}
				var hasRight = Input.ReadBoolean ();
				if (hasRight) {
					node.Right = this.LoadNode (Input, node);
				}
				return node;
			} else {
				var count = Input.ReadInt32 ();
				var symbol = Input.ReadInt32 ();
				// Console.WriteLine ("--leaf> count: {0}, symbol: {1}", count, symbol);
				var leaf = new WT_Leaf (parent, symbol, count);
				this.Alphabet[symbol] = leaf;
				return leaf;
			}
		}
		
		void Walk (WT_Inner node,
			Func<WT_Inner, object> preorder,
			Func<WT_Inner, object> inorder,
			Func<WT_Inner, object> postorder)
		{
			if (node == null) {
				return;
			}
			if (preorder != null) {
				preorder (node);
			}
			this.Walk (node.Left as WT_Inner, preorder, inorder, postorder);
			if (inorder != null) {
				inorder (node);
			}
			this.Walk (node.Right as WT_Inner, preorder, inorder, postorder);
			if (postorder != null) {
				postorder (node);
			}
		}
		
		public int Count {
			get { return (int)this.Root.B.Count; }
		}

		public int this[int index] {
			get { return this.Access (index); }
		}

		public int Rank (int symbol, int position)
		{
			var coderstream = new BitStream32 ();
			this.Coder.Encode (coderstream, symbol);
			//this.CoderStream.Seek (0);
			var ctx = new BitStreamCtx (0);
			int numbits = (int)coderstream.CountBits;
			var node = this.Root;
			for (int i = 0; i < numbits; i++) {
				bool b = coderstream.Read (ctx);
				if (b) {
					position = node.B.Rank1 (position) - 1;
					if (i + 1 < numbits) {
						node = node.Right as WT_Inner;
					}
				} else {
					position = node.B.Rank0 (position) - 1;
					if (i + 1 < numbits) {
						node = node.Left as WT_Inner;
					}
				}
				if (node == null) {
					return 0;
				}
			}
			return position + 1;
		}

		public int Select (int symbol, int rank)
		{
			var coderstream = new BitStream32 ();
			this.Coder.Encode (coderstream, symbol);
			int numbits = (int)coderstream.CountBits;
			var symnode = this.Alphabet [symbol];
			if (symnode == null || symnode.Count < rank) {
				return -1;
			}
			WT_Inner node = symnode.Parent as WT_Inner;
			for (--numbits; numbits >= 0; numbits--) {
				bool b = coderstream [numbits];
				if (b) {
					rank = node.B.Select1 (rank) + 1;
				} else {
					rank = node.B.Select0 (rank) + 1;
				}
				node = node.Parent as WT_Inner;
			}
			return rank - 1;
		}

		public int Access (int position)
		{
			var node = this.Root;
			WT_Inner tmp;
			for (int i = 0; true; i++) {
				if (node.B.Access(position)) {
					position = node.B.Rank1 (position) - 1;
					tmp = node.Right as WT_Inner;
					if (tmp == null) {
						return (node.Right as WT_Leaf).Symbol;
					} else {
						node = tmp;
					}
				} else {
					position = node.B.Rank0 (position) - 1;
					tmp = node.Left as WT_Inner;
					if (tmp == null) {
						return (node.Left as WT_Leaf).Symbol;
					} else {
						node = tmp;
					}
				}
			}
		}
		
		public IRankSelect Unravel (int symbol)
		{
			return new UnraveledSymbol (this, symbol);
		}


		// auxiliar classes
		public class WT_Node
		{
			public WT_Node Parent;
			public WT_Node (WT_Node parent)
			{
				this.Parent = parent;
			}
		}
		
		public class WT_Inner : WT_Node
		{
			public IRankSelect B;
			public WT_Node Left;
			public WT_Node Right;
			
			public WT_Inner (WT_Inner parent, bool building) : base(parent)
			{
				if (building) {
					this.B = new FakeBitmap ();
				}
				this.Left = null;
				this.Right = null;
			}
			
		}
		
		public class WT_Leaf : WT_Node
		{
			
			public int Count;
			public int Symbol;
			
			public WT_Leaf (WT_Inner parent, int symbol) : base(parent)
			{
				this.Count = 1;
				this.Symbol = symbol;
			}
			
			public WT_Leaf (WT_Inner parent, int symbol, int count) : base(parent)
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