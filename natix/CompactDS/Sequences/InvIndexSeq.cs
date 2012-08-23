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
//   Original filename: natix/CompactDS/Sequences/InvIndexSeq.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
		
namespace natix.CompactDS
{

	public class InvIndexSeq : IRankSelectSeq
	{
		/// <summary>
		/// Inverted index
		/// </summary>
		// public IList< IList< int > > InvIndex;
		public IList<IRankSelect> InvIndex;
		/// <summary>
		/// The size in words (entities) of the text
		/// </summary>
		public int N;
		
		public int Count {
			get {
				return this.N;
			}
		}
		
		public int Sigma {
			get {
				return this.InvIndex.Count;
			}
		}
		
		public BitmapFromList BitmapBuilder {
			get;
			set;
		}
		
		public InvIndexSeq ()
		{
			this.BitmapBuilder = BitmapBuilders.GetSArray ();
		}

		/// <summary>
		/// Builds the index for the sequence
		/// </summary>
		public void Build (IList<int> sequence, int alphabet_size)
		{
			var invindex = new IList<int>[alphabet_size];
			for (int i = 0; i < alphabet_size; i++) {
				invindex [i] = new List<int> ();
			}
			int pos = 0;
			foreach (var c in sequence) {
				invindex [c].Add (pos);
				pos++;
			}
			pos = 0;
			this.N = sequence.Count;
			this.InvIndex = new IRankSelect[alphabet_size];
			for (int i = 0; i < alphabet_size; i++) {
				if (i % 1000 == 0) {
					Console.WriteLine ("*** compressing invlist {0}/{1}", i, alphabet_size);
				}
				this.InvIndex [i] = this.BitmapBuilder (invindex [i]);
				invindex [i] = null;
			}
			invindex = null;
		}
		
		public int Access (int pos)
		{
			for (int i = 0; i < this.InvIndex.Count; i++) {
				var invlist = this.InvIndex [i];
				if (invlist.Count > pos && invlist.Access (pos)) {
					return i;
				}
			}
			throw new ArgumentOutOfRangeException ();
		}
		
		public int Rank (int symbol, int pos)
		{
			if (pos < 0) {
				return 0;
			}
			var invlist = this.InvIndex[symbol];
			pos = Math.Min (invlist.Count - 1, pos);
			return invlist.Rank1 (pos);
		}
		
		public int Select (int symbol, int rank)
		{
			if (rank < 1) {
				return -1;
			}
			return this.InvIndex[symbol].Select1 (rank);
		}
		
		public IRankSelect Unravel (int symbol)
		{
			return this.InvIndex[symbol];
		}		

		// **** IO methods

		/// <summary>
		/// Save the index
		/// </summary>
		public void Save (BinaryWriter Output)
		{
			int vocsize = this.InvIndex.Count;
			Output.Write ((int)this.N);
			Output.Write ((int)vocsize);
			foreach (var L in this.InvIndex) {
				RankSelectGenericIO.Save (Output, L);
			}
		}

		public void Load (BinaryReader Input)
		{
			this.N = Input.ReadInt32 ();
			int vocsize = Input.ReadInt32 ();
			this.InvIndex = new IRankSelect[vocsize];
			for (int i = 0; i < vocsize; i++) {
				this.InvIndex[i] = RankSelectGenericIO.Load (Input);
			}
		}
	}
}
