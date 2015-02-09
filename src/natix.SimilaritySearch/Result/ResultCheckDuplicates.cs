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
//   Original filename: natix/SimilaritySearch/Result/Result.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The result set
	/// </summary>
	public class ResultCheckDuplicates : IResult
	{
        HashSet<int> inserted = new HashSet<int>();
        IResult R;

        public ResultCheckDuplicates (IResult res)
        {
            this.R = res;
        }

        public IEnumerator<ItemPair> GetEnumerator ()
        {
            return this.R.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return this.R.GetEnumerator();
        }

        
        public bool Push (int docid, double dist)
        {
            if (this.inserted.Contains (docid)) {
                return false;
            }
            if (this.R.Push (docid, dist)) {
                this.inserted.Add (docid);
                return true;
            } else {
                return false;
            }
        }
        
        public int K {
            get {
                return this.R.K;
            }
        }
        
        public int Count {
            get {
                return this.R.Count;
            }
        }
        
        public double CoveringRadius {
            get {
                return this.R.CoveringRadius;
            }
        }
        
        public ItemPair First {
            get {
                return this.R.First;
            }
        }
        
        
        public ItemPair Last {
            get {
                return this.R.Last;
            }
        }

    }
}
