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
        public struct Item : ILoadSave
        {
            public int objID;
            public float dist;

            public Item (int objID, double dist)
            {
                this.objID = objID;
                this.dist = (float)dist;
            }
            
            public void Load(BinaryReader Input)
            {
                this.objID = Input.ReadInt32 ();
                this.dist = Input.ReadSingle();
            }
            
            public void Save (BinaryWriter Output)
            {
                Output.Write (this.objID);
                Output.Write (this.dist);
            }
        }

        public struct Stats
        {
            public float min;
            public float max;
            public float mean;
            public float stddev;

            public Stats (double min, double max, double mean, double stddev)
            {
                this.min = (float) min;
                this.max = (float) max;
                this.mean = (float) mean;
                this.stddev = (float) stddev;
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
				this.Remove(p.docid);
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

        public List<Item> ComputeDistances (object piv, List<Item> output, out Stats stats)
        {
            if (output == null) {
                output = new List<Item>(this.Count);
            }
            //var L = new Item[this.DOCS.Count];
            stats = default(Stats);
            stats.min = float.MaxValue;
            stats.max = 0;
            double mean = 0;
            foreach (var objID in this.Iterate()) {
                var dist = (float)this.DB.Dist(piv, this.DB[objID]);
                mean += dist;
                output.Add( new Item(objID, dist) );
                stats.min = Math.Min (dist, stats.min);
                stats.max = Math.Max (dist, stats.max);
            }
            stats.mean = (float)(mean / this.Count);
            double stddev = 0;
            foreach (var item in output) {
                var m = item.dist - stats.mean;
                stddev += m * m;
            }
            stats.stddev = (float)Math.Sqrt(stddev / this.Count);
            return output;
        }

        public void SortByDistance (List<Item> output)
        {
            output.Sort( (Item x, Item y) => x.dist.CompareTo(y.dist) );
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
                far.Push (p.docid, -p.dist);
            }
		}


        public void DropCloseToMean (double near_radius, double far_radius, IResult near, IResult far, List<Item> items)
        {
            foreach (var item in items) {
                if (item.dist <= near_radius) {
                    near.Push (item.objID, item.dist);
                } else if (item.dist >= far_radius) {
                    far.Push (item.objID, item.dist);
                }
            }
        }

        public void AppendKExtremes (int K, IResult near, IResult far, List<Item> items)
        {
            var _far = new Result (this.Count);
            foreach (var item in items) {
                if (!near.Push (item.objID, item.dist)) {
                    _far.Push (item.objID, item.dist);
                }
            }
            foreach (var p in _far) {
                far.Push (p.docid, -p.dist);
            }
        }
	}
}
