//
//  Copyright 2012  Eric Sadit Tellez Avila
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
using System.Collections;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
    public class ResultPushStats : IResult
    {
        public IResult R;
        public int count_push = 0;
        public int count_true_push = 0;

        public ResultPushStats (IResult R)
        {
            this.R = R;
        }

        public IEnumerator<ResultPair> GetEnumerator ()
        {
            return this.R.GetEnumerator();
        }
 
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return this.R.GetEnumerator();
        }

        public bool Push (int docid, double dist)
        {
            var b = this.R.Push (docid, dist);
            if (b) {
                this.count_true_push++;
            }
            this.count_push++;
            return b;
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

        public bool Ceiling {
            get {
                return this.R.Ceiling;
            }
        }

        public ResultPair First {
            get {
                return this.R.First;
            }
        }


        public ResultPair Last {
            get {
                return this.R.Last;
            }
        }

    }
}

