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
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
    public class SAT : BasicIndex, ILoadSave
    {
        public class Node : ILoadSave
        {
            public List<Node> Children;
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

            public virtual void Load (BinaryReader Input)
            {
                this.objID = Input.ReadInt32 ();
                this.cov = Input.ReadDouble ();
                var len = Input.ReadInt32 ();
                this.Children = new List<Node>(len);
                if (len > 0) {
                    CompositeIO<Node>.LoadVector (Input, len, this.Children);
                }
            }

            public virtual void Save (BinaryWriter Output)
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
            /*var rand = new Random();
            double min = double.MaxValue;
            for (int i = 0; i < 128; ++i) {
                ++this.internal_numdists;
                var obj = this.DB[ rand.Next(0, this.DB.Count) ];
                min = Math.Min(this.DB.Dist(q, obj), min);
            }
            this.SearchRangeNode (this.DB.Dist(q, this.DB[this.root.objID]), this.root, q, min, res);
            return res;
            */

			var dist = this.DB.Dist(q, this.DB[this.root.objID]);
			res.Push (this.root.objID, dist);
//			Console.WriteLine ("CHILDREN-count: {0}", this.root.Children.Count);
			if (this.root.Children.Count > 0 && dist <= res.CoveringRadius + this.root.cov) {
				this.SearchKNNNode (this.root, q, res);
			}
//			Console.WriteLine ("\n ROOT: {0}", this.root.objID);
//			if (this.S == null) { 
//				this.S = new Sequential ();
//				S.Build (this.DB);
//			}
//			var R = S.SearchKNN (q, K);
//			if (R.First.docid != res.First.docid) {
//				Console.WriteLine ("XXXXX seq {0} != sat {1} XXXXXX", R, res);
//			}
            return res;
        }
//		Sequential S;

        protected virtual void SearchKNNNode (Node node, object q, IResult res)
        {
            // res.Push (node.objID, dist);
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
				var child = node.Children[i];
				res.Push(child.objID, D[i]);
				var radius = res.CoveringRadius;
				if (child.Children.Count > 0
				    && D[i] <= radius + child.cov
				    && D[i] <= closer_dist + radius + radius) {
				   
					this.SearchKNNNode(child, q, res);
                }
            }
        }

//        protected virtual void SearchRangeNode (Node node, object q, double radius, IResult res)
//        {
//            // var dist = this.DB.Dist (this.DB [node.objID], q);
//            //            Console.WriteLine ("dist: {0}, node: {1}, res: {2}", dist, node, res);
//            //            Console.WriteLine ("num-children: {0}, cov: {1}", node.Children.Count, node.cov);
//			var D = new double[node.Children.Count];
//			var closer_child = node.Children[0];
//			var closer_dist = this.DB.Dist(q, this.DB[closer_child.objID]);
//			D[0] = closer_dist;
//			for (int i = 1; i < D.Length; ++i) {
//				var child = node.Children[i];
//				D[i] = this.DB.Dist(q, this.DB[child.objID]);
//				if (D[i] < closer_dist) {
//					closer_dist = D[i];
//					closer_child = child;
//				}
//			}
//			for (int i = 0; i < D.Length; ++i) {
//				var child = node.Children[i];
//				if (D[i] <= radius) {
//					res.Push (child.objID, D[i]);
//				}
//				if (child.Children.Count > 0
//				    && D[i] <= closer_dist + 2 * radius
//				    && D[i] <= radius + child.cov ) {
//					this.SearchRangeNode(child, q, radius, res);
//				}
//			}
//        }

        public virtual void Build (MetricDB db, Random rand)
        {
            this.DB = db;
            var root_objID = rand.Next (0, this.DB.Count);
            this.root = new Node (root_objID);
            var _items = new List<int>(this.DB.Count - 1);
            for (int docID = 0; docID < root_objID; ++docID) {
				_items.Add (docID);
			}
            for (int docID = 1 + root_objID; docID < this.DB.Count; ++docID) {
				_items.Add (docID);
			}
            var items = DynamicSequential.ComputeDistances(this.DB, _items, this.DB[root_objID], null);
            //int count_step = 0;
            //this.BuildNode(this.root, items, ref count_step, 1);
			this.BuildNode(this.root, items, 1);
        }

        protected virtual void SortItems (List<ItemPair> items)
        {
            DynamicSequential.SortByDistance (items);
        }

		int count_step = 0;
        protected virtual void BuildNode (Node node, List<ItemPair> items, int depth)// ref int count_step, int depth)
        {
            //Console.WriteLine("======== BUILD NODE: {0}", node.objID);
            ++count_step;
            if (count_step < 100 || count_step % 100 == 0) {
				Console.WriteLine ("======== SAT {4} build_node: {0}, count_step: {1}/{2}, items_count: {3}, timestamp: {5}, db: {6}",
				                   node.objID, count_step, this.DB.Count, items.Count, this, DateTime.Now, this.DB.Name);
            }
            var partition = new List< List< ItemPair > > ();
            //var cache = new Dictionary<int,double> (items.Count);
            this.SortItems (items);
            var pool = new List<int> (items.Count);
            foreach (var item in items) {
                node.cov = Math.Max (node.cov, item.Dist);
                object currOBJ;
                currOBJ = this.DB [item.ObjID];
                var closer = new Result (1);
                closer.Push (-1, item.Dist);
                for (int child_ID = 0; child_ID < node.Children.Count; ++child_ID) {
                    var child = node.Children [child_ID];
                    var childOBJ = this.DB [child.objID];
                    var d_child_curr = this.DB.Dist (childOBJ, currOBJ);
                    closer.Push (child_ID, d_child_curr);
                }
                {
                    var child_ID = closer.First.ObjID;
                    // var closer_dist = closer.First.dist;
                    if (child_ID == -1) {
                        var new_node = new Node (item.ObjID);
                        node.Children.Add (new_node);
                        partition.Add (new List<ItemPair> ());
                    } else {
//                        partition [child_ID].Add (new DynamicSequential.Item (item.objID, closer_dist));
                        pool.Add (item.ObjID);
                    }
                }
            }

            foreach (var objID in pool) {
                var closer = new Result (1);
                for (int child_ID = 0; child_ID < node.Children.Count; ++child_ID) {
                    var child = node.Children [child_ID];
                    var childOBJ = this.DB [child.objID];
                    var d_child_curr = this.DB.Dist (childOBJ, this.DB[objID]);
                    closer.Push (child_ID, d_child_curr);
                }
                {
                    var child_ID = closer.First.ObjID;
                    var closer_dist = closer.First.Dist;
					partition[child_ID].Add (new ItemPair {ObjID = objID, Dist = closer_dist});
                }
            }
            pool = null;
            //Console.WriteLine("===== add children");
//			Action<int> build_node = delegate (int child_ID) {
//				this.BuildNode (node.Children [child_ID], partition [child_ID], depth + 1);
//				partition [child_ID] = null;
//			};
//			var ops = new ParallelOptions();
//			ops.MaxDegreeOfParallelism = -1;
//			Parallel.For (0, node.Children.Count, build_node);
			for (int child_ID = 0; child_ID < node.Children.Count; ++child_ID) {
				//this.BuildNode (node.Children [child_ID], partition [child_ID], ref count_step, depth + 1);
				this.BuildNode (node.Children [child_ID], partition [child_ID], depth + 1);
				partition [child_ID] = null;
			}
        }         
    }
}

