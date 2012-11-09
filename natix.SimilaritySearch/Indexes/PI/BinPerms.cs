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
//   Original filename: natix/SimilaritySearch/Indexes/BinPerms.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;
using natix.SimilaritySearch;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The binary encoded permutations (BriefPermutations) index
	/// </summary>
	public class BinPerms : Perms
	{
		/// <summary>
		/// The name of the hamming index (external index)
		/// </summary>
		public Index IndexHamming;

		/// <summary>
		/// Indicates if the index will permute the center
		/// </summary>
		public bool permcenter;
		/// <summary>
		/// The module
		/// </summary>
		public int MOD;

		public override void Save (BinaryWriter Output)
		{
			Output.Write(this.DB.Name);
			Output.Write(this.permcenter);
			Output.Write(this.MOD);
			SpaceGenericIO.Save(Output, this.REFS, false);
			IndexGenericIO.Save(Output, this.IndexHamming);
		}

		public override void Load (BinaryReader Input)	
		{
			var dbname = Input.ReadString();
			this.DB = SpaceGenericIO.Load(dbname, true);
			this.permcenter = Input.ReadBoolean();
			this.MOD = Input.ReadInt32 ();
			this.REFS = SpaceGenericIO.Load(Input, false);
			this.IndexHamming = IndexGenericIO.Load(Input);
		}
		/// <summary>
		/// Constructor
		/// </summary>
		public BinPerms () : base()
		{
		}
		
		/// <summary>
		/// The length of the dimension in bytes (vector's length in bytes of the bit string)
		/// </summary>
		public virtual int GetDimLengthInBytes (int invlen)
		{
			return invlen >> 3;
		}

		/// <summary>
		/// The API Build method for BinPerms 
		/// </summary>
		public void Build (MetricDB db, MetricDB refs, int maxcand=1024, double mod=0.5, bool permcenter=true, Perms idxperms=null)
		{
			this.DB = db;
			this.REFS = refs;
			this.MAXCAND = maxcand;
			if (mod < 1) {
				this.MOD = (int)Math.Ceiling (mod * this.REFS.Count);
			} else {
				this.MOD = (int)mod;
			}
			this.permcenter = permcenter;
			var DATA = new List<IList<byte>>();
			if (idxperms == null) {
				// base.Build (name, spaceClass, spaceName, spacePerms, maxcand);
				int onepercent = 1 + (this.DB.Count / 100);
				for (int i = 0, sL = this.DB.Count; i < sL; i++) {
					if ((i % onepercent) == 0) {
						Console.WriteLine ("Generating brief permutations for {0}, advance {1:0.00}%", i, i * 100.0 / sL);
					}
					IList<Int16> inv = this.GetInverseBuild (i);
					DATA.Add (this.Encode(inv));
				}
			} else {
				for (int docid = 0; docid < this.DB.Count; docid++) {
					IList<Int16> inv = idxperms.GetComputedInverse (docid);
					DATA.Add(this.Encode(inv));
				}
			}
			var binperms = new BinH8Space();
			binperms.Build ("", DATA);
			var seq = new Sequential ();
			seq.Build(binperms);
			this.IndexHamming = seq;
		}

		/// <summary>
		/// Performs encoding of an object
		/// </summary>
		public IList<byte> Encode (object u)
		{
			return this.Encode (this.GetInverse (u));
		}
		/// <summary>
		/// Encode an inverse permutation into an encoded bit-string
		/// </summary>
		/// <param name="inv">
		/// Inverse permutation
		/// A <see cref="Int16[]"/>
		/// </param>
		/// <returns>
		/// Bit-string/Brief permutation
		/// A <see cref="System.Byte[]"/>
		/// </returns>
		public virtual IList<byte> Encode (IList<Int16> inv)
		{
			int len = this.GetDimLengthInBytes(inv.Count);
			byte[] res = new byte[len];
			if (this.permcenter) {
				int M = inv.Count / 4; // same
				for (int i = 0, c = 0; i < len; i++) {
					int b = 0;
					for (int bit = 0; bit < 8; bit++,c++) {
						int C = c;
						if ((((int)(C / M)) % 3) != 0) {
							C += M;
						}
						// Console.WriteLine ("C: {0}, Mod: {1}", C, this.mod);
						if (Math.Abs (inv[c] - C) > this.MOD) {
							b |= (1 << bit);
						}
					}
					res[i] = (byte)b;
				}
			} else {
				for (int i = 0, c = 0; i < len; i++) {
					int b = 0;
					for (int bit = 0; bit < 8; bit++,c++) {
						if (Math.Abs (inv[c] - c) > this.MOD) {
							b |= (1 << bit);
						}
					}
					res[i] = (byte)b;
				}

			}
			return res;
		}

		
		/// <summary>
		/// KNN Search in the index
		/// </summary>
		/// <param name="q">
		/// The query object
		/// </param>
		/// <param name="k">
		/// The number of nearest neighbors
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// The result set
		/// A <see cref="Result"/>
		/// </returns>
		public override IResult SearchKNN (object q, int k)
		{
			IList<byte> enc = this.Encode (q);
            ++this.internal_numdists;
			//Console.WriteLine ("EncQuery: {0}", BinaryHammingSpace.ToAsciiString (enc));
			var cand = this.IndexHamming.SearchKNN (enc, Math.Abs (this.MAXCAND));
			//Result candseq = this.indexHammingSeq.KNNSearch (enc, 10);
			//Math.Abs (this.Maxcand));
			//Result cand = this.indexHamming.Search (enc, 60);
			/*Result cand = new Result (Math.Abs (this.Maxcand));
			for (int docid = 0, bL = this.binperms.Length; docid < bL; docid++) {
				cand.Push (docid, this.binperms.Dist(this.binperms[docid], enc));
			}*/
			if (this.MAXCAND < 0) {
				return cand;
			}
			var res = this.DB.CreateResult(k, true);
			foreach (ResultPair p in cand) {
				res.Push(p.docid, this.DB.Dist(q, this.DB[p.docid]));
			}
	        return res;
		}
	}
}
