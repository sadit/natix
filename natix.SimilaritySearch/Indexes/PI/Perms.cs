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
	public class GenPerms<GType> : BasicIndex
	{	
		/// <summary>
		/// The space for permutants
		/// </summary>
		protected MetricDB REFS;

		/// <summary>
		/// The inverses of the permutations
		/// </summary>
		protected VectorSpace<GType> INVPERMS;

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.REFS = SpaceGenericIO.Load(Input, false);
			this.INVPERMS = (VectorSpace<GType>)SpaceGenericIO.Load(Input, false);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			SpaceGenericIO.Save(Output, this.REFS, false);
			Console.WriteLine("==== SAVING INVPERMS: {0}, count: {1}", this.INVPERMS, this.INVPERMS.Count);
			SpaceGenericIO.Save(Output, this.INVPERMS, false);
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
		/// Returns the internal vector space with the inverted permutations
		/// </summary>
		/// <returns>
		/// The inverted permutations
		/// </returns>
		public VectorSpace<GType> GetInvPermsVectorSpace ()
		{
			return this.INVPERMS;
		}

		/// <summary>
		///  Get the computed inverse (stored in invperms)
		/// </summary>
		public IList<GType> GetComputedInverse (int docid)
		{
			return (IList<GType>)this.INVPERMS[docid];
		}
	
		/// <summary>
		/// The current search cost object for the index
		/// </summary>
		public override SearchCost Cost {
			get {
				return new SearchCost (this.REFS.NumberDistances, this.DB.NumberDistances);
			} 
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		public GenPerms () : base()
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
			var VECS = new IList<GType>[this.DB.Count];
			for (int i = 0, sL = this.DB.Count; i < sL; i++) {
				if ((i % onepercent) == 0) {
					Console.WriteLine ("Generating permutations for {0}, advance {1:0.00}%", i, i * 100.0 / sL);
				}
				VECS[i] = this.GetInverseBuild (i);
			}
			var INVS = new MinkowskiVectorSpace<GType>();
			INVS.Build("", VECS, 2);
			this.INVPERMS = INVS;
		}

		/// <summary>
		/// Compute the inverse for the Build method
		/// </summary>
		protected virtual IList<GType> GetInverseBuild (int docid)
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
		public IList<GType> GetDistances (object obj)
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
		public IList<GType> GetInverseRaw (IList<GType> seq)
		{
			IList<GType> inv = new GType[seq.Count];
			var num = this.Num;
			for (int i = 0; i < inv.Count; i++) {
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
		public IList<GType> GetInverse (object obj)
		{
			return this.GetInverseRaw (this.GetDistances (obj));
		}
		
		/// <summary>
		/// Search by radius
		/// </summary>
		public override IResult SearchRange (object q, double radius)
		{
			return this.FilterByRadius (this.SearchKNN (q, Math.Abs (this.MAXCAND)), radius);
		}

		/// <summary>
		/// KNN searches.
		/// </summary>
		public override IResult SearchKNN (object q, int k, IResult res)
		{
			IList<GType> qinv = this.GetInverse (q);
			var cand = this.INVPERMS.CreateResult (Math.Abs (this.MAXCAND), false);
			for (int docid = 0; docid < this.INVPERMS.Count; docid++) {
				cand.Push (docid, this.INVPERMS.Dist ((IList<GType>)this.INVPERMS[docid], qinv));
			}
			// cand = this._OrderingFunctions.Filter (this, q, qinv, cand);
			if (this.MAXCAND < 0) {
				return cand;
			}
			foreach (ResultPair p in cand) {
				res.Push(p.docid, this.DB.Dist(q, this.DB[p.docid]));
			}
	        return res;
		}
	}
	
	/// <summary>
	///  The basic permutation class, using 16 bit signed integers
	/// </summary>
	public class Perms : GenPerms<Int16>
	{
		public Perms () : base()
		{
		}
	}
	
	/// <summary>
	/// A simpler permutation class using byte integers
	/// </summary>
	public class Perms8 : GenPerms<byte>
	{
		public Perms8 () : base()
		{
		}
	}
}
