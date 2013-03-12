////
////  Copyright 2013  Eric Sadit Tellez Avila
////
////    Licensed under the Apache License, Version 2.0 (the "License");
////    you may not use this file except in compliance with the License.
////    You may obtain a copy of the License at
////
////        http://www.apache.org/licenses/LICENSE-2.0
////
////    Unless required by applicable law or agreed to in writing, software
////    distributed under the License is distributed on an "AS IS" BASIS,
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
////    See the License for the specific language governing permissions and
////    limitations under the License.
//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using natix.CompactDS;
//using natix.SortingSearching;
//
//namespace natix.SimilaritySearch
//{
//    public class SAT : BasicIndex, ILoadSave
//    {
//        public class Node : ILoadSave
//        {
//            public Node[] Children;
//            public int objID;
//            public float cov;
//
//            public Node()
//            {
//            }
//
//            public void Load (BinaryReader Input)
//            {
//                this.objID = Input.ReadInt32 ();
//                this.cov = Input.ReadSingle();
//                this.Children = new Node[Input.ReadInt32 ()];
//                CompositeIO<Node>.LoadVector(Input, this.Children.Length, this.Children);
//            }
//
//            public void Save (BinaryWriter Output)
//            {
//                Output.Write(this.objID);
//                Output.Write(this.cov);
//                Output.Write (this.Children.Length);
//                CompositeIO<Node>.SaveVector(Output, this.Children);
//            }
//        }
//
//        public Node root;
//
//        public SAT ()
//        {
//        }
//
//        public override void Load (BinaryReader Input)
//        {
//            base.Load (Input);
//            this.root = default(Node);
//            this.root.Load(Input);
//        }
//
//        public override void Save (BinaryWriter Output)
//        {
//            base.Save(Output);
//            this.root.Save(Output);
//        }
//
//        public override IResult SearchKNN (object q, int K, IResult res)
//        {
//            throw new System.NotImplementedException ();
//        }
//
//        public override IResult SearchRange (object q, double radius)
//        {
//            var res = new Result(this.DB.Count);
//            if (this.root == null) return res;
//            foreach (this.root.Children) {
//            }
//        }
//    }
//}
//
