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
using natix.SimilaritySearch;
using System.Threading.Tasks;

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
			SpaceGenericIO.SmartSave(Output, this.REFS);
			IndexGenericIO.Save(Output, this.IndexHamming);
		}

		public override void Load (BinaryReader Input)	
		{
			var dbname = Input.ReadString();
			this.DB = SpaceGenericIO.Load(dbname, true, true);
			this.permcenter = Input.ReadBoolean();
			this.MOD = Input.ReadInt32 ();
			this.REFS = SpaceGenericIO.SmartLoad(Input, false);
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

		public void Build(MetricDB db, int num_refs, int maxcand=1024, double mod=0.5, bool permcenter=true)
		{
			var ss = new SampleSpace ("", db, num_refs);
			this.Build (db, ss, maxcand, mod, permcenter);
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
			var DATA = new List<byte[]>();
			if (idxperms == null) {
				// base.Build (name, spaceClass, spaceName, spacePerms, maxcand);
				int onepercent = 1 + (this.DB.Count / 100);
				for (int docID = 0; docID < this.DB.Count; ++docID) DATA.Add (null);
				int I = 0;

				var build_one = new Action<int> ((int docID) => {
					if ((I % onepercent) == 0) {
						Console.WriteLine ("Generating {0}, db: {1}, num_refs: {2}, docID: {3}, advance {4:0.00}%, timestamp: {5}",
							this, db.Name, refs.Count, I, I * 100.0 / DATA.Count, DateTime.Now);
					}
					var inv = this.ComputeInverse (docID);
					DATA[docID] = this.Encode(inv);
					++I;
				});
				var ops = new ParallelOptions ();
				ops.MaxDegreeOfParallelism = -1;
				Parallel.For (0, this.DB.Count, ops, build_one);
			} else {
				for (int docid = 0; docid < this.DB.Count; docid++) {
					var inv = idxperms.GetComputedInverse (docid);
					DATA.Add(this.Encode(inv));
				}
			}
			var binperms = new MemMinkowskiVectorDB<byte> ();
			binperms.Build ("", DATA, 1);
			var seq = new Sequential ();
			seq.Build(binperms);
			this.IndexHamming = seq;
		}

		/// <summary>
		/// Performs encoding of an object
		/// </summary>
		public byte[] Encode (object u)
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
		public virtual byte[] Encode (Int16[] inv)
		{
			int len = this.GetDimLengthInBytes(inv.Length);
			byte[] res = new byte[len];
			if (this.permcenter) {
				int M = inv.Length / 4; // same
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
			var enc = this.Encode (q);
			this.internal_numdists += this.REFS.Count;
			//Console.WriteLine ("EncQuery: {0}", BinaryHammingSpace.ToAsciiString (enc));
			var cand = this.IndexHamming.SearchKNN (enc, Math.Abs (this.MAXCAND));
			if (this.MAXCAND < 0) {
				return cand;
			}
			var res = new Result(k);
			foreach (ItemPair p in cand) {
				res.Push(p.ObjID, this.DB.Dist(q, this.DB[p.ObjID]));
			}
	        return res;
		}
	}
}
