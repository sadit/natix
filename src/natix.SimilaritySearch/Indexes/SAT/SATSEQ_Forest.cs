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
using System.Threading.Tasks;

namespace natix.SimilaritySearch
{
    public class SATSEQ_Forest : BasicIndex
    {
        public IList<SATSEQ_ApproxSearch> forest;

        public SATSEQ_Forest () : base()
        {
        }

        public override void Load (BinaryReader Input)
        {
            base.Load (Input);
            var len = Input.ReadInt32();
            this.forest = CompositeIO<SATSEQ_ApproxSearch>.LoadVector(Input, len, null);
        }

        public override void Save (BinaryWriter Output)
        {
            base.Save (Output);
            Output.Write(this.forest.Count);
            CompositeIO<SATSEQ_ApproxSearch>.SaveVector(Output, this.forest);
        }

        public virtual void Build (IList<SAT> _forest, int max_trees)
        {
            this.DB = _forest[0].DB;
            this.forest = new SATSEQ_ApproxSearch[max_trees];
            for (int i = 0; i < max_trees; ++i) {
                this.forest[i] = new SATSEQ_ApproxSearch();
                this.forest[i].Build(_forest[i]);
            }
        }

        public virtual Action ClosureBuildOne (int i, Random rand)
        {
            return delegate() {
                //var sat = new SAT_Random();
                //sat.Build (this.DB, rand);
                var sat = new SAT_Randomized();
                sat.Build (this.DB, rand, 64);
                this.forest[i] = new SATSEQ_ApproxSearch();
                this.forest[i].Build(sat);
            };
        }

        public virtual void Build (MetricDB db, int num_trees, Random rand)
        {
            this.DB = db;
            this.forest = new SATSEQ_ApproxSearch[num_trees];
            var action_list = new Action[num_trees];
            var seed = rand.Next();
            for (int i = 0; i < num_trees; ++i) {
                action_list[i] = this.ClosureBuildOne(i, RandomSets.GetRandom(seed + i));
            }
            ParallelOptions ops = new ParallelOptions();
            ops.MaxDegreeOfParallelism = -1;
            Parallel.ForEach(action_list, (a) => a.Invoke());
        }


        public override IResult SearchKNN (object q, int K, IResult res)
        {
            var R = new ResultCheckDuplicates (res);
            foreach (var sat in this.forest) {
                sat.SearchKNN(q, K, R);
            }
            return res;
        }

        public override IResult SearchRange (object q, double radius)
        {
            var R = new ResultRange(radius, this.DB.Count);
            return this.SearchKNN(q, this.DB.Count, R);
        }
    }
}

