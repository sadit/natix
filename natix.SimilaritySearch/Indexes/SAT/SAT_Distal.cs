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
using natix.CompactDS;
using natix;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
    public class SAT_Distal : SAT
    {
        public SAT_Distal ()
        {
        }

        protected override void SortItems (List<ItemPair> items)
        {
            DynamicSequential.SortByDistance (items);
            var middle = items.Count >> 1;
            for (int left = 0; left < middle; ++left) {
                var right = items.Count - 1 - left;
                var tmp = items[left];
                items[left] = items[right];
                items[right] = tmp;
            }
            //items.Reverse(); // mono's implementation has a weird bug, it change the values
        }
    }
}

