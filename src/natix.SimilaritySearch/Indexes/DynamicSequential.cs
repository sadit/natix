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
//   Original filename: natix/SimilaritySearch/Indexes/DynamicSequential.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NDesk.Options;
using natix;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The sequential index
	/// </summary>
	public abstract class DynamicSequential : BasicIndex
	{
        public struct Stats
        {
            public double min;
            public double max;
            public double mean;
            public double stddev;

            public Stats (double min, double max, double mean, double stddev)
            {
                this.min = min;
                this.max = max;
                this.mean = mean;
                this.stddev = stddev;
            }
        }

		/// <summary>
		/// Constructor
		/// </summary>
		public DynamicSequential ()
		{
		}

        public abstract void Remove (int docid);

		public void Remove (IEnumerable<int> docs)
		{	
			foreach (var docid in docs) {
				this.Remove(docid);
			}
		}

		public void Remove (IResult res)
		{	
			foreach (var p in res) {
				this.Remove(p.ObjID);
			}
		}

		/// <summary>
		/// Search by range
		/// </summary>
		public override IResult SearchRange (object q, double radius)
		{
			var r = new Result (this.Count);
			foreach (var docid in this.Iterate()) {
				double d = this.DB.Dist (q, this.DB[docid]);
				if (d <= radius) {
					r.Push (docid, d);
				}
			}
			return r;
		}
		
		/// <summary>
		/// KNN Search
		/// </summary>
		public override IResult SearchKNN (object q, int k, IResult R)
		{
			foreach (var docid in this.Iterate()) {
				double d = this.DB.Dist (q, this.DB[docid]);
				R.Push (docid, d);
			}
			return R;
		}

        public abstract IEnumerable<int> Iterate ();
        public abstract int Count {
            get;
        }

        public abstract int GetAnyItem ();

        public static List<ItemPair> ComputeDistances (MetricDB db, IEnumerable<int> sample, object piv, List<ItemPair> output)
        {
            Stats stats;
			int m, M;
            return ComputeDistances (db, sample, piv, output, out stats, out m, out M);
        }

		public static List<ItemPair> ComputeDistances (MetricDB db, IEnumerable<int> sample, object piv, List<ItemPair> output, out Stats stats)
		{
			int m, M;
			return ComputeDistances (db, sample, piv, output, out stats, out m, out M);
		}

        public static List<ItemPair> ComputeDistances (MetricDB db, IEnumerable<int> sample, object piv, List<ItemPair> output, out Stats stats, out int min_objID, out int max_objID)
        {
            if (output == null) {
                output = new List<ItemPair>();
            }
            //var L = new Item[this.DOCS.Count];
			max_objID = min_objID = -1;
	        stats = default(Stats);
            stats.min = double.MaxValue;
            stats.max = 0;
            double mean = 0;
            var count = 0;
            foreach (var objID in sample) {
                var dist = db.Dist(piv, db[objID]);
				mean += dist;
				output.Add (new ItemPair (objID, dist));
				if (dist < stats.min) {
					stats.min = dist;
					min_objID = objID;
				}
				if (dist > stats.max) {
					stats.max = dist;
					max_objID = objID;
				}
                ++count;
            }
            stats.mean = mean / count;
            double stddev = 0;
            foreach (var item in output) {
                var m = item.Dist - stats.mean;
                stddev += m * m;
            }
            stats.stddev = Math.Sqrt(stddev / count);
            return output;
        }

		public List<ItemPair> ComputeDistances (object piv, List<ItemPair> output, out Stats stats, out int min_objID, out int max_objID)
		{
			return DynamicSequential.ComputeDistances(this.DB, this.Iterate(), piv, output, out stats, out min_objID, out max_objID);
		}

        public List<ItemPair> ComputeDistances (object piv, List<ItemPair> output, out Stats stats)
        {
			int m, M;
            return DynamicSequential.ComputeDistances(this.DB, this.Iterate(), piv, output, out stats, out m, out M);
        }

        public List<ItemPair> ComputeDistances (object piv, List<ItemPair> output)
        {
            Stats stats;
			int m, M;
			return DynamicSequential.ComputeDistances(this.DB, this.Iterate(), piv, output, out stats, out m, out M);
        }

        public static void SortByDistance (List<ItemPair> output)
        {
            //output.Sort( (Item x, Item y) => x.dist.CompareTo(y.dist) );
            Sorting.Sort<ItemPair>(output, (ItemPair x, ItemPair y) => x.Dist.CompareTo(y.Dist));            
        }

		public void SearchExtremes (object q, IResult near, IResult far)
        {
            var _far = new Result (this.Count);
            foreach (var docid in this.Iterate()) {
                double d = this.DB.Dist (q, this.DB [docid]);
                if (!near.Push (docid, d)) {
                    _far.Push (docid, -d);
                }
            }
            foreach (var p in _far) {
                far.Push (p.ObjID, -p.Dist);
            }
		}


        public void DropCloseToMean (double near_radius, double far_radius, IResult near, IResult far, List<ItemPair> items)
        {
            foreach (var item in items) {
                if (item.Dist <= near_radius) {
                    near.Push (item.ObjID, item.Dist);
                } else if (item.Dist >= far_radius) {
                    far.Push (item.ObjID, item.Dist);
                }
            }
        }

        public void AppendKExtremes (IResult near, IResult far, List<ItemPair> items)
        {
            var _far = new Result (far.K);
            foreach (var item in items) {
                if (!near.Push (item.ObjID, item.Dist)) {
                    _far.Push (item.ObjID, -item.Dist);
                }
            }
            foreach (var p in _far) {
                far.Push (p.ObjID, -p.Dist);
            }
        }
	}
}
