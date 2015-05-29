//
//  Copyright 2015  Eric S. Tellez <eric.tellez@infotec.com.mx>
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class VPT : BasicIndex
	{
		class Node : ILoadSave
		{
			public int refID;
			public Node left;
			public Node right;
			public double median;

			public Node()
			{
			}
	
			public Node(int[] items, MetricDB db, Random rand)
			{
				
				if (items.Length == 1) {
					this.refID = items[0];
					this.median = 0;
					return;
				}

				double[] D = new double[items.Length];
				this.refID = items[rand.Next(0, items.Length)];

				for (int i = 0; i < D.Length; ++i) {
					D[i] = db.Dist(db[items[i]], db[this.refID]);
				}

				Sorting.Sort(D, items);
				this.refID = items[0];  // adjusting in case of two identical items

				int m = (D.Length + 1) / 2;

				this.median = D[m];

				var _left = new int[m - 1];
				var _right = new int[items.Length - _left.Length - 1];

				for (int i = 0; i < _left.Length; ++i) {
					_left[i] = items[i + 1];
				}

				for (int i = 0; i < _right.Length; ++i) {
					_right[i] = items[m + i];
				}

				// items will be present for all its children, so we should care about wasting memory
				D = null;
				items = null; // it cannot be free since it exists for its parent

				if (_left.Length > 0) {
					this.left = new Node(_left, db, rand);
				}
				_left = null;
				if (_right.Length > 0) {
					this.right = new Node(_right, db, rand);
				}
			}


			public void Save(BinaryWriter Output)
			{
				Output.Write (this.refID);
				Output.Write (this.median);
				if (this.left == null) {
					Output.Write (false);
				} else {
					Output.Write (true);
					this.left.Save (Output);
				}
				if (this.right == null) {
					Output.Write (false);
				} else {
					Output.Write (true);
					this.right.Save (Output);
				}
			}

			public void Load(BinaryReader Input)
			{
				this.refID = Input.ReadInt32 ();
				this.median = Input.ReadDouble ();
				if (Input.ReadBoolean ()) {
					this.left.Load (Input);
				}
				if (Input.ReadBoolean ()) {
					this.right.Load (Input);
				}
			}

			public void SearchKNN(object q, IResult res, MetricDB db)
			{
				var d = db.Dist (db [this.refID], q);
				res.Push (this.refID, d);

				if (this.left != null && d - res.CoveringRadius <= this.median) {
					this.left.SearchKNN (q, res, db);
				}

				if (this.right != null && this.median <= d + res.CoveringRadius) {
					this.right.SearchKNN (q, res, db);
				}
			}
		}

		Node root;

		public VPT () : base()
		{
		}

		public override void Load (BinaryReader Input)
		{
			base.Load (Input);
			this.root.Load (Input);
		}

		public override void Save (BinaryWriter Output)
		{
			base.Save (Output);
			this.root.Save (Output);
		}

		/// <summary>
		/// Build the index
		/// </summary>
		public virtual void Build (MetricDB db, Random rand)
		{
			this.DB = db;
			var n = this.DB.Count;
			var items = RandomSets.GetIdentity (n);
			this.root = new Node (items, db, rand);
		}

		/// <summary>
		/// KNN search.
		/// </summary>
		public override IResult SearchKNN (object q, int K, IResult res)
		{
			this.root.SearchKNN (q, res, this.DB);
			return res;
		}
	}
}

