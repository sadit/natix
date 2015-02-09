//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Original filename: natix/SimilaritySearch/Indexes/MLSC_H8.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NDesk.Options;
using natix.CompactDS;

namespace natix.SimilaritySearch
{

	public class MLSC_H8 : MLSC
	{
		public MLSC_H8() : base()
		{
		}

		public void Build (MetricDB db, int sample_size, int num_instances, SequenceBuilder seq_builder = null)
		{
			this.DB = db;
			this.lsc_indexes = new LSC[num_instances];
			// IPermutation perm = null;
			for (int i = 0; i < num_instances; ++i) {
				var lsc = new LSC_H8 ();
				lsc.Build(db, sample_size, seq_builder);
				this.lsc_indexes[i] = lsc;
			}
		}
	}
}
