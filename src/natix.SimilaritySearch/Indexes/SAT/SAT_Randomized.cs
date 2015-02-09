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
    public class SAT_Randomized : SAT
    {
        Random rand;
        public SAT_Randomized ()
        {
        }

        public override void Build (MetricDB db, Random rand)
        {
            this.Build (db, rand, 32);
        }

        public void Build (MetricDB db, Random rand, int arity)
        {
            this.DB = db;
            var root_objID = rand.Next (0, this.DB.Count);
            this.root = new Node (root_objID);
            var _items = new List<int>();
            for (int i = 0; i < root_objID; ++i) _items.Add(i);
            for (int i = 1 + root_objID; i < this.DB.Count; ++i) _items.Add(i);
            this.rand = rand;
            DynamicSequential.Stats stats;
            var items = DynamicSequential.ComputeDistances(this.DB, _items, this.DB[root_objID], null, out stats);
            int count_step = 0;
            this.BuildNodeRandom(this.root, items, arity, ref count_step);
        }

        protected void BuildNodeRandom (Node node, IList<ItemPair> input_collection, int arity, ref int count_step)
        {
            ++count_step;
            if (count_step < 100 || count_step % 100 == 0) {
                Console.WriteLine ("======== SAT_Randomized build_node: {0}, arity: {1}, part-size: {2}, advance: {3}/{4}", node.objID, arity, input_collection.Count, count_step, this.DB.Count);
            }
            var partition = new List< IList<ItemPair> > ();
            int count_arity;
            for (count_arity = 0; count_arity < arity && count_arity < input_collection.Count; ++count_arity) {
                var i = this.rand.Next (count_arity, input_collection.Count);
                // swap
                var child_item = input_collection [i];
                input_collection [i] = input_collection [count_arity];
                input_collection [count_arity] = child_item;
                node.cov = Math.Max (node.cov, child_item.Dist);
                node.Children.Add( new Node(child_item.ObjID) );
                partition.Add ( new List<ItemPair> () );
            }
            for (int i = count_arity; i < input_collection.Count; ++i) {
                var curr_item = input_collection[i];
                node.cov = Math.Max (node.cov, curr_item.Dist);
                var curr_OBJ = this.DB [curr_item.ObjID];
                var closer = new TopK<int> (1);
                for (int child_ID = 0; child_ID < node.Children.Count; ++child_ID) {
                    var child_OBJ = this.DB [node.Children[child_ID].objID];
                    var d_child_curr = this.DB.Dist (child_OBJ, curr_OBJ);
                    closer.Push (d_child_curr, child_ID);
                }
                var p = closer.Items.GetFirst ();
                var closer_child_ID = p.Value;
                // var closer_child_objID = node.Children[closer_child_ID].objID;
                //Console.WriteLine("<X {0},{1}>", closer_child_ID, closer_child_objID);
				partition[closer_child_ID].Add(new ItemPair{ ObjID = curr_item.ObjID, Dist = p.Key});
            }
            for (int child_ID = 0; child_ID < node.Children.Count; ++child_ID) {
                //Console.WriteLine ("=== child objID: {0}, child_ID: {1}", node.Children[child_ID].objID, child_ID);
                this.BuildNodeRandom(node.Children[child_ID], partition[ child_ID ], arity, ref count_step);
            }
        }         
    }
}

