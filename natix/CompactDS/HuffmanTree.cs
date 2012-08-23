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
//   Original filename: natix/CompactDS/HuffmanTree.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using natix.SortingSearching;

namespace natix.CompactDS
{
	/// <summary>
	/// A node in the Huffman tree
	/// </summary>
	public class HuffmanNode
	{
		/// <summary>
		/// The parent.
		/// </summary>
		public HuffmanInner Parent;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.HuffmanNode"/> class.
		/// </summary>
		public HuffmanNode (HuffmanInner parent)
		{
			this.Parent = parent;
		}
		/// <summary>
		/// The length of the code, It performs O(H_0) operations in average
		/// </summary>
		public int Size {
			get {
				HuffmanNode n = this;
				int s = 0;
				while (n.Parent != null) {
					n = n.Parent;
					s++;
				}
				return s;
			}
		}
	}
	
	/// <summary>
	/// A leaf in the huffman tree
	/// </summary>
	public class HuffmanLeaf : HuffmanNode
	{
		/// <summary>
		/// The symbol.
		/// </summary>
		public int Symbol;
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.HuffmanLeaf"/> class.
		/// </summary>
		public HuffmanLeaf (int symbol) : base(null)
		{
			this.Symbol = symbol;
		}
	}
	
	/// <summary>
	/// An inner node in the huffman tree
	/// </summary>
	public class HuffmanInner : HuffmanNode
	{
		/// <summary>
		/// The reference to the left child
		/// </summary>
		public HuffmanNode Left;
		/// <summary>
		/// The right child
		/// </summary>
		public HuffmanNode Right;
		
		/*public HuffmanInner (int S, int F, HuffmanNode L, HuffmanNode R) : base(null)
		{
			this.Left = L;
			this.Right = R;
		}*/
		
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.HuffmanInner"/> class.
		/// </summary>
		public HuffmanInner (HuffmanNode L, HuffmanNode R) : base(null)
		{
			this.Left = L;
			this.Right = R;
			if (L != null) {
				L.Parent = this;
			}
			if (L != null) {
				R.Parent = this;
			}
			this.Parent = null;
		}	
	}
	
	/// <summary>
	/// Huffman inner node (only used at build time)
	/// </summary>
	public class HuffmanInnerBuild : HuffmanInner, IComparable<HuffmanInnerBuild>
	{
		static int NodeCounter = 0;
		// incredible huge huffman node, but simplifying the whole implementation
		// useful for small to medium alphabet
		// Since the huffman tree is explicit, we can perform manipulation of the tree
		// and implement adaptive huffman
		/// <summary>
		/// The accumulated frequency
		/// </summary>
		public int Freq;
		/// <summary>
		/// The symbol.
		/// </summary>
		public int Symbol;
		/// <summary>
		/// The time stamp (for equal frequences)
		/// </summary>
		public int TimeStamp;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.HuffmanInnerBuild"/> class.
		/// </summary>
		public HuffmanInnerBuild (int S, int F) : base(null, null)
		{
			this.Symbol = S;
			this.Freq = F;
			this.Parent = null;
			this.TimeStamp = NodeCounter++;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.HuffmanInnerBuild"/> class.
		/// </summary>
		public HuffmanInnerBuild (HuffmanInnerBuild L, HuffmanInnerBuild R) : base(L,R)
		{
			this.Freq = L.Freq + R.Freq;
			this.TimeStamp = NodeCounter++;
		}

		/// <summary>
		/// Compares two huffman nodes
		/// </summary>
		public int CompareTo (HuffmanInnerBuild node)
		{
			int cmp = this.Freq.CompareTo (node.Freq);
			if (cmp == 0) {
				return this.TimeStamp.CompareTo (node.TimeStamp);
			}
			return cmp;
		}
	}
	
	/// <summary>
	/// Huffman tree.
	/// </summary>
	public class HuffmanTree
	{
		/// <summary>
		/// The alphabet.
		/// </summary>
		public HuffmanLeaf[] Alphabet;
		/// <summary>
		/// The root.
		/// </summary>
		public HuffmanInner Root;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="natix.HuffmanTree"/> class.
		/// </summary>
		public HuffmanTree ()
		{
		}
		
		/// <summary>
		/// Build the huffman tree with the model (set of requencies aligned/indexed with the symbols)
		/// </summary>
		public void Build (IList<int> model)
		{
			this.Alphabet = new HuffmanLeaf[model.Count];
			this.Root = null;
			SplayTree<HuffmanInnerBuild> pqueue = new SplayTree<HuffmanInnerBuild> ((a, b) => a.CompareTo (b));
			for (int i = 0; i < model.Count; i++) {
				var node = new HuffmanInnerBuild (i, model [i]);
				// this.Alphabet[i] = node;
				pqueue.Add (node);
			}
			if (pqueue.Count == 0) {
				throw new ArgumentOutOfRangeException ("Empty data can't be compressed");
			}
			while (pqueue.Count > 1) {
				var first = pqueue.RemoveFirst ();
				var second = pqueue.RemoveFirst ();
				var newnode = new HuffmanInnerBuild (first, second);
				pqueue.Add (newnode);
			}
			this.Root = pqueue.RemoveFirst ();
			pqueue.Clear ();
			this.Root = (HuffmanInner)this.FinishBuild ((HuffmanInnerBuild)this.Root);
			this.Root.Parent = null;
		}
		
		/// <summary>
		/// Finishs the build.
		/// </summary>
		public HuffmanNode FinishBuild (HuffmanInnerBuild node)
		{
			if (node.Left == null) {
				var N = new HuffmanLeaf (node.Symbol);
				this.Alphabet [N.Symbol] = N;
				return N;
			} else {
				var L = this.FinishBuild ((HuffmanInnerBuild)node.Left);
				var R = this.FinishBuild ((HuffmanInnerBuild)node.Right);
				var N = new HuffmanInner (L, R);
				L.Parent = R.Parent = N;
				return N;
			}
		}
		
		/// <summary>
		/// Save the Huffman tree to the specified Output.
		/// </summary>
		public void Save (BinaryWriter Output)
		{
			Output.Write ((int)this.Alphabet.Length);
			this.SaveNode (Output, this.Root);
		}
		
		/// <summary>
		/// Saves a single node.
		/// </summary>
		public void SaveNode (BinaryWriter Output, HuffmanNode node)
		{
			var asLeaf = node as HuffmanLeaf;
			if (asLeaf != null) {
				// is it a leaf?
				Output.Write (true);
				Output.Write ((int)asLeaf.Symbol);
			} else {
				var asInner = node as HuffmanInner;
				Output.Write (false);
				this.SaveNode (Output, asInner.Left);
				this.SaveNode (Output, asInner.Right);
			}
		}
		
		/// <summary>
		/// Load a Huffman tree from the Input (to the current object)
		/// </summary>
		public void Load (BinaryReader Input)
		{
			var len = Input.ReadInt32 ();
			this.Alphabet = new HuffmanLeaf[len];
			this.Root = (HuffmanInner)this.LoadNode (Input);
			this.Root.Parent = null;
		}
		
		/// <summary>
		/// Loads a huffman node.
		/// </summary>
		public HuffmanNode LoadNode (BinaryReader Input)
		{
			if (Input.ReadBoolean ()) {
				var N = new HuffmanLeaf (Input.ReadInt32 ());
				this.Alphabet [N.Symbol] = N;
				return N;
			} else {
				var L = this.LoadNode (Input);
				var R = this.LoadNode (Input);
				var N = new HuffmanInner (L, R);
				L.Parent = R.Parent = N;
				return N;
			}
		}
		
		/// <summary>
		/// Encode the specified symbol and save it to the stream
		/// </summary>
		public int Encode (IBitStream stream, int symbol)
		{
			var s = this.Alphabet[symbol];
			if (s == null) {
				return 0;
			}
			var parent = s.Parent as HuffmanInner;
			int size = this._Encode (parent, stream, 1);
			stream.Write (parent.Right == s);
			return size;
		}
		
		int _Encode (HuffmanInner node, IBitStream stream, int size)
		{
			if (node.Parent == null) {
				return size;
			} else {
				size = this._Encode (node.Parent, stream, size);
				stream.Write (node.Parent.Right == node);
				++size;
				return size;
			}
		}
	}
}
