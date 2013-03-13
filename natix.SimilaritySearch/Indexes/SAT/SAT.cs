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
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
    public class SAT : BasicIndex, ILoadSave
    {
        public class Node : ILoadSave
        {
            public IList<Node> Children;
            public int objID;
            public double cov;

            public Node()
            {
                this.Children = new List<Node>();
            }

            public Node(int objID)
            {
                this.objID = objID;
                this.cov = 0;
                this.Children = new List<Node>();
            }

            public void Load (BinaryReader Input)
            {
                this.objID = Input.ReadInt32 ();
                this.cov = Input.ReadDouble ();
                var len = Input.ReadInt32 ();
                this.Children = new Node[len];
                if (len > 0) {
                    CompositeIO<Node>.LoadVector (Input, this.Children.Count, this.Children);
                }
            }

            public void Save (BinaryWriter Output)
            {
                Output.Write (this.objID);
                Output.Write (this.cov);
                Output.Write (this.Children.Count);
                if (this.Children.Count > 0) {
                    CompositeIO<Node>.SaveVector (Output, this.Children);
                }
            }
        }

        public Node root;

        public SAT ()
        {
        }

        public override void Load (BinaryReader Input)
        {
            base.Load (Input);
            this.root = new Node();
            this.root.Load(Input);
        }

        public override void Save (BinaryWriter Output)
        {
            base.Save(Output);
            this.root.Save(Output);
        }

        public override IResult SearchKNN (object q, int K, IResult res)
        {
            if (this.root == null) {
                return res;
            }
            var rand = new Random();
            double min = double.MaxValue;
            for (int i = 0; i < 128; ++i) {
                ++this.internal_numdists;
                var obj = this.DB[ rand.Next(0, this.DB.Count) ];
                min = Math.Min(this.DB.Dist(q, obj), min);
            }
            this.SearchRangeNode (this.DB.Dist(q, this.DB[this.root.objID]), this.root, q, min, res);
            return res;
        }


        public override IResult SearchRange (object q, double radius)
        {
            var res = new Result (this.DB.Count);
            if (this.root != null) {
                this.SearchRangeNode (this.DB.Dist(q, this.DB[this.root.objID]), this.root, q, radius, res);
            }
            //this.SEARCH_COUNT++;
            //if (this.SEARCH_COUNT > 10) throw new Exception("PLEASE DEBUG IT");
            return res;
        }

        //int SEARCH_COUNT = 0;

        protected void SearchRangeNode (double dist, Node node, object q, double radius, IResult res)
        {
            // var dist = this.DB.Dist (this.DB [node.objID], q);
//            Console.WriteLine ("dist: {0}, node: {1}, res: {2}", dist, node, res);
//            Console.WriteLine ("num-children: {0}, cov: {1}", node.Children.Count, node.cov);
            if (dist <= radius) {
                res.Push (node.objID, dist);
            }
            if (node.Children.Count > 0 && dist <= radius + node.cov) {
                var D = new double[node.Children.Count];
                var closer_child = node.Children[0];
                var closer_dist = this.DB.Dist(q, this.DB[closer_child.objID]);
                D[0] = closer_dist;
                for (int i = 1; i < D.Length; ++i) {
                    var child = node.Children[i];
                    D[i] = this.DB.Dist(q, this.DB[child.objID]);
                    if (D[i] < closer_dist) {
                        closer_dist = D[i];
                        closer_child = child;
                    }
                }
                for (int i = 0; i < D.Length; ++i) {
                    if (D[i] <= closer_dist + 2 * radius) {
                        this.SearchRangeNode(D[i], node.Children[i], q, radius, res);
                    }
                }
            }
        }

        public virtual void Build (MetricDB db)
        {
            this.DB = db;
            this.root = new Node( 0 );
            var _items = RandomSets.GetExpandedRange(1, this.DB.Count);
            DynamicSequential.Stats stats;
            var items = DynamicSequential.ComputeDistances(this.DB, _items, this.DB[0], null, out stats);
            int count_step = 0;
            this.BuildNode(this.root, items, ref count_step);
        }

        protected virtual void SortItems (List<DynamicSequential.Item> items)
        {
            DynamicSequential.SortByDistance (items);
        }

        protected virtual void BuildNode (Node node, List<DynamicSequential.Item> items, ref int count_step)
        {
            //Console.WriteLine("======== BUILD NODE: {0}", node.objID);
            ++count_step;
            if (count_step < 100 || count_step % 100 == 0) {
                Console.WriteLine ("======== SAT build_node: {0}, count_step: {1}/{2}", node.objID, count_step, this.DB.Count);
            }
            var partition = new List< List< DynamicSequential.Item > > ();
            this.SortItems (items);
            foreach (var item in items) {
                node.cov = Math.Max (node.cov, item.dist);
                object currOBJ;
                currOBJ = this.DB [item.objID];
                var closer = new Result (1);
                closer.Push (-1, item.dist);
                for (int child_ID = 0; child_ID < node.Children.Count; ++child_ID) {
                    var child = node.Children [child_ID];
                    var childOBJ = this.DB [child.objID];
                    var d_child_curr = this.DB.Dist (childOBJ, currOBJ); 
                    closer.Push (child_ID, d_child_curr);
                }
                {
                    var child_ID = closer.First.docid;
                    var closer_dist = closer.First.dist;
                    if (child_ID == -1) {
                        var new_node = new Node (item.objID);
                        node.Children.Add (new_node);
                        partition.Add (new List<DynamicSequential.Item> ());
                    } else {
                        partition [child_ID].Add (new DynamicSequential.Item (item.objID, closer_dist));
                    }
                }
            }
            //Console.WriteLine("===== add children");
            for (int child_ID = 0; child_ID < node.Children.Count; ++child_ID) {
                this.BuildNode ( node.Children[child_ID], partition[child_ID], ref count_step );
                partition[child_ID] = null;
            }
        }         
    }
}

