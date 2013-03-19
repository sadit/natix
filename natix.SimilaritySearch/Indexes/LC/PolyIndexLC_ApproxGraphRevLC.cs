//
//  Copyright 2013  Eric Sadit Tellez Avila
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
using natix.Sets;
using natix.CompactDS;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
    public class PolyIndexLC_ApproxGraphRevLC : PolyIndexLC_ApproxGraph
    {
        public PolyIndexLC_ApproxGraphRevLC () : base()
        {
        }

        public PolyIndexLC_ApproxGraphRevLC (int repeat, int failtimes) : base(repeat, failtimes)
        {
        }

        protected override Action BuildOneClosure (IList<LC_RNN> output, int i, MetricDB db, int numcenters, Random rand, SequenceBuilder seq_builder)
        {
            var action = new Action(delegate () {
                var lc = new LC_IRNN();
                lc.Build (db, numcenters, rand, seq_builder);
                output[i] = lc;
            });
            return action;
        }
    }
}