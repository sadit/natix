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
            public float cov;

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
                this.cov = Input.ReadSingle ();
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
            var sets = new Dictionary< int, IList<int> >();
            this.root = new Node( 0 );
            sets[0] = RandomSets.GetExpandedRange(1, this.DB.Count);
            int count_step = 0;
            this.BuildNode(this.root, sets, ref count_step);
        }

        protected virtual void SortItems (List<DynamicSequential.Item> items)
        {
            DynamicSequential.SortByDistance (items);
        }

        protected virtual void BuildNode (Node node, Dictionary<int, IList<int> > sets, ref int count_step)
        {
            //Console.WriteLine("======== BUILD NODE: {0}", node.objID);
            ++count_step;
            if (count_step < 100 || count_step % 100 == 0) {
                Console.WriteLine ("======== SAT build_node: {0}, count_step: {1}/{2}", node.objID, count_step, this.DB.Count);
            }
            DynamicSequential.Stats stats;
            var items = DynamicSequential.ComputeDistances(this.DB, sets[node.objID], this.DB[node.objID], null, out stats);
            sets.Remove(node.objID);
            //var items = idxseq.ComputeDistances (this.DB [node.objID], null, out stats);
            this.SortItems(items);
            foreach (var item in items) {
                node.cov = Math.Max (node.cov, item.dist);
                var currOBJ = this.DB [item.objID];
                TopK<Node> closer = new TopK<Node> (1);
                closer.Push (item.dist, node);
                foreach (var child in node.Children) {
                    var childOBJ = this.DB [child.objID];
                    var d_child_curr = this.DB.Dist (childOBJ, currOBJ); 
                    closer.Push (d_child_curr, child);
                }
                var p = closer.Items.GetFirst ();
                var closer_node = p.Value;
                if (closer_node.objID == node.objID) {
                    var new_node = new Node (item.objID);
                    node.Children.Add (new_node);
                   // Console.WriteLine("** add: {0}", new_node.objID);
                    sets.Add(new_node.objID, new List<int>());
                } else {
                    sets[closer_node.objID].Add(item.objID);
                }
            }
            //Console.WriteLine("===== add children");
            foreach (var child in node.Children) {
                this.BuildNode(child, sets, ref count_step);
            }
        }         
    }
}

