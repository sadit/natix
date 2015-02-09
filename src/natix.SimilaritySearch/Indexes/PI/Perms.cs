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
//   Original filename: natix/SimilaritySearch/Indexes/Perms.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using NDesk.Options;

using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The index of permutations
	/// </summary>
	/// <remarks>
	/// The permutation index with a full representation of permutations.
	/// 
	/// This is an approximate index.
	/// </remarks>
	public class GenericPerms<GType> : BasicIndex where GType : struct
	{	
		/// <summary>
		/// The space for permutants
		/// </summary>
		public MetricDB REFS;

		/// <summary>
		/// The inverses of the permutations
		/// </summary>
		public MemVectorDB<GType> INVPERMS;

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.REFS = SpaceGenericIO.SmartLoad(Input, false);
			this.INVPERMS = (MemVectorDB<GType>)SpaceGenericIO.SmartLoad(Input, false);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.SmartSave(Output, this.REFS);
			Console.WriteLine("==== SAVING INVPERMS: {0}, count: {1}", this.INVPERMS, this.INVPERMS.Count);
			SpaceGenericIO.SmartSave(Output, this.INVPERMS);
		}

		/// <summary>
		/// Numeric manager for GType
		/// </summary>
		/// 
		protected INumeric<GType> Num = (INumeric<GType>)(Numeric.Get (typeof(GType)));

		/// <summary>
		///  Get/Set the maximum number of candidates
		/// </summary>
		public int MAXCAND {
			get;
			set;
		}

		/// <summary>
		///  Get the computed inverse (stored in invperms)
		/// </summary>
		public IList<GType> GetComputedInverse (int docid)
		{
			return (IList<GType>)this.INVPERMS[docid];
		}
	
		/// <summary>
		/// Constructor
		/// </summary>
		public GenericPerms () : base()
		{
		}
		
		/// <summary>
		/// API method to build a Perms Index
		/// </summary>
		public void Build (MetricDB db, MetricDB refs, int maxcand = 1024)
		{
			this.DB = db;
			this.REFS = refs;
			this.MAXCAND = maxcand;
			this.INVPERMS = null;
			int onepercent = 1 + (this.DB.Count / 100);
			var VECS = new List<GType[]>(this.DB.Count);
			for (int docID = 0; docID < this.DB.Count; ++docID) VECS.Add (null);
			int I = 0;
			var build_one = new Action<int> ((int docID) => {
				if ((I % onepercent) == 0) {
					Console.WriteLine ("Generating {0}, db: {1}, num_refs: {2}, docID: {3}, advance {4:0.00}%",
						this, db.Name, refs.Count, I, I * 100.0 / VECS.Count);
				}
				VECS[docID] = this.ComputeInverse (docID);
				++I;
			});
			System.Threading.Tasks.Parallel.For (0, this.DB.Count, build_one);
			Console.WriteLine ("=== creating underlying vector space");
			var INVS = new MemMinkowskiVectorDB<GType>();
			INVS.Build("", VECS, 2);
			this.INVPERMS = INVS;
		}

		public void Build (MetricDB db, int sample_size, int maxcand = 1024)
		{
			var ss = new SampleSpace ("", db, sample_size);
			this.Build (db, ss, maxcand);
		}
		/// <summary>
		/// Compute the inverse for the Build method
		/// </summary>
		protected virtual GType[] ComputeInverse (int docid)
		{
			return this.GetInverse (this.DB[docid]);
		}

		/// <summary>
		/// Compute the distances from permutants to the object
		/// </summary>
		/// The object
		/// </param>
		/// <returns>
		/// The computed distances
		/// </returns>
		public GType[] GetDirect (object obj)
		{
			
			double[] D = new double[this.REFS.Count];
			GType[] I = new GType[D.Length];
			for (int i = 0; i < D.Length; i++) {
				D[i] = this.DB.Dist (obj, this.REFS[i]);
				I[i] = this.Num.FromInt(i);
			}
			Sorting.Sort<double, GType> (D, I);
			return I;
		}

		/// <summary>
		///  Compute the inverse from an already computed permutation
		/// </summary>
		public GType[] GetInverseRaw (GType[] seq)
		{
			var inv = new GType[seq.Length];
			var num = this.Num;
			for (int i = 0; i < inv.Length; i++) {
				inv[num.ToInt(seq[i])] = num.FromInt(i);
			}
			return inv;
		}
		
		/// <summary>
		/// Compute the inverse for an object
		/// </summary>
		/// <param name="obj">
		/// The object
		/// </param>
		/// <returns>
		/// The computed inverse
		/// </returns>
		public GType[] GetInverse (object obj)
		{
			return this.GetInverseRaw (this.GetDirect (obj));
		}
		
		/// <summary>
		/// KNN searches.
		/// </summary>
		public override IResult SearchKNN (object q, int k, IResult res)
		{
			IList<GType> qinv = this.GetInverse (q);
            this.internal_numdists+= this.REFS.Count;
			var cand = new Result(Math.Abs(this.MAXCAND));
			for (int docID = 0; docID < this.INVPERMS.Count; docID++) {
				var obj = this.INVPERMS [docID];
				cand.Push (docID, this.INVPERMS.Dist (obj, qinv));
			}
			// cand = this._OrderingFunctions.Filter (this, q, qinv, cand);
			if (this.MAXCAND < 0) {
				return cand;
			}
			foreach (ItemPair p in cand) {
				res.Push(p.ObjID, this.DB.Dist(q, this.DB[p.ObjID]));
			}
	        return res;
		}
	}
	
	/// <summary>
	///  The basic permutation class, using 16 bit signed integers
	/// </summary>
	public class Perms : GenericPerms<Int16>
	{
		public Perms () : base()
		{
		}
	}
	
	/// <summary>
	/// A simpler permutation class using byte integers
	/// </summary>
	public class Perms8 : GenericPerms<byte>
	{
		public Perms8 () : base()
		{
		}
	}
}
