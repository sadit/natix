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
	/// A wavelet node (implementation with pointers/references)
	/// </summary>
	public class WaveletTree : IRankSelectSeq
	{
		// static INumericManager<T> Num = (INumericManager<T>)NumericManager.Get (typeof(T));
		IIEncoder32 Coder = null;
		// BitStream32 CoderStream;
		WaveletInner Root;
		IList<WaveletLeaf> Alphabet;
		
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
		
		public void Build (IIEncoder32 coder, int alphabet_size, IList<int> text)
		{
			this.Alphabet = new WaveletLeaf[alphabet_size];
			this.Root = new WaveletInner (null, true);
			this.Coder = coder;
			for (int i = 0; i < text.Count; i++) {
				this.Add (text [i]);
			}
			this.FinishBuild (this.Root);
		}

		void FinishBuild (WaveletInner node)
		{
			if (node == null) {
				return;
			}
			node.B = this.BitmapBuilder(node.B as FakeBitmap);
			this.FinishBuild (node.Left as WaveletInner);
			this.FinishBuild (node.Right as WaveletInner);
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
							var leaf = new WaveletLeaf (node, symbol);
							// this.Alphabet.Add (leaf);
							this.Alphabet [symbol] = leaf;
							node.Right = leaf;
						} else {
							(node.Right as WaveletLeaf).Increment ();
						}
					} else {
						if (node.Right == null) {
							node.Right = new WaveletInner (node, true);
						}
						node = node.Right as WaveletInner;
					}
				} else {
					if (numbits == b + 1) {
						if (node.Left == null) {
							var leaf = new WaveletLeaf (node, symbol);
							// this.Alphabet.Add (leaf);
							this.Alphabet [symbol] = leaf;
							node.Left = leaf;
						} else {
							(node.Left as WaveletLeaf).Increment ();
						}
					} else {
						if (node.Left == null) {
							node.Left = new WaveletInner (node, true);
						}
						node = node.Left as WaveletInner;
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
		
		void SaveNode (BinaryWriter Output, WaveletNode node)
		{
			var asInner = node as WaveletInner;
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
				var asLeaf = node as WaveletLeaf;
				Output.Write ((int)asLeaf.Count);
				Output.Write ((int)asLeaf.Symbol);
			}
		}
		
		public void Load (BinaryReader Input)
		{
			var size = Input.ReadInt32 ();
			this.Alphabet = new WaveletLeaf[size];
			this.Coder = IEncoder32GenericIO.Load (Input);
			// Console.WriteLine ("Input.Position: {0}", Input.BaseStream.Position);
			this.Root = this.LoadNode (Input, null) as WaveletInner;
		}
		
		WaveletNode LoadNode (BinaryReader Input, WaveletInner parent)
		{
			// Console.WriteLine ("xxxxxxxxx LoadNode");
			var isInner = Input.ReadBoolean ();
			if (isInner) {
				var node = new WaveletInner (parent, false);
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
				var leaf = new WaveletLeaf (parent, symbol, count);
				this.Alphabet[symbol] = leaf;
				return leaf;
			}
		}
		
		void Walk (WaveletInner node,
			Func<WaveletInner, object> preorder,
			Func<WaveletInner, object> inorder,
			Func<WaveletInner, object> postorder)
		{
			if (node == null) {
				return;
			}
			if (preorder != null) {
				preorder (node);
			}
			this.Walk (node.Left as WaveletInner, preorder, inorder, postorder);
			if (inorder != null) {
				inorder (node);
			}
			this.Walk (node.Right as WaveletInner, preorder, inorder, postorder);
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
						node = node.Right as WaveletInner;
					}
				} else {
					position = node.B.Rank0 (position) - 1;
					if (i + 1 < numbits) {
						node = node.Left as WaveletInner;
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
			WaveletInner node = symnode.Parent as WaveletInner;
			for (--numbits; numbits >= 0; numbits--) {
				bool b = coderstream [numbits];
				if (b) {
					rank = node.B.Select1 (rank) + 1;
				} else {
					rank = node.B.Select0 (rank) + 1;
				}
				node = node.Parent as WaveletInner;
			}
			return rank - 1;
		}

		public int Access (int position)
		{
			var node = this.Root;
			WaveletInner tmp;
			for (int i = 0; true; i++) {
				if (node.B.Access(position)) {
					position = node.B.Rank1 (position) - 1;
					tmp = node.Right as WaveletInner;
					if (tmp == null) {
						return (node.Right as WaveletLeaf).Symbol;
					} else {
						node = tmp;
					}
				} else {
					position = node.B.Rank0 (position) - 1;
					tmp = node.Left as WaveletInner;
					if (tmp == null) {
						return (node.Left as WaveletLeaf).Symbol;
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
	}
	
	public class WaveletNode
	{
		public WaveletNode Parent;
		public WaveletNode (WaveletNode parent)
		{
			this.Parent = parent;
		}
	}

	public class WaveletInner : WaveletNode
	{
		public IRankSelect B;
		public WaveletNode Left;
		public WaveletNode Right;

		public WaveletInner (WaveletInner parent, bool building) : base(parent)
		{
			if (building) {
				this.B = new FakeBitmap ();
			}
			this.Left = null;
			this.Right = null;
		}

	}

	public class WaveletLeaf : WaveletNode
	{

		public int Count;
		public int Symbol;

		public WaveletLeaf (WaveletInner parent, int symbol) : base(parent)
		{
			this.Count = 1;
			this.Symbol = symbol;
		}
		
		public WaveletLeaf (WaveletInner parent, int symbol, int count) : base(parent)
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