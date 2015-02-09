//
//  Copyright 2012,2013,2014  Eric Sadit Tellez Avila
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
	public class LAESA : PivotsAbstract
	{

		public LAESA ()
		{
		}
		
		public void Build (LAESA idx, int num_pivs)
		{
			this.DB = idx.DB;
			var S = new int[num_pivs];
			this.DIST = new List<double>[num_pivs];
			int I = 0;
			for (int pivID = 0; pivID < num_pivs; ++pivID) {
				S[pivID] = pivID;
				this.DIST[pivID] = idx.DIST[pivID];
				I++;
			}
			this.PIVS = new SampleSpace("", idx.PIVS, S);
		}

		public void Build (MetricDB db, int num_pivs, int NUMBER_TASKS=-1)
		{
			this.Build(db, new SampleSpace("", db, num_pivs), NUMBER_TASKS);
		}
		
	}
}

