//
//  Copyright 2012  Eric Sadit Tellez Avila
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
	public class CompactPivotsSEQRANS : CompactPivotsLRANS
	{

		public CompactPivotsSEQRANS () : base()
		{
		}

		public Sequence[] SEQ;
				
		public override void Load_DIST (BinaryReader Input)
		{
			this.SEQ = new Sequence[this.PIVS.Count];
			for (int i = 0; i < this.PIVS.Count; ++i) {
				this.SEQ[i] = GenericIO<Sequence>.Load(Input);
			}
			this.EmulateDIST();
		}
		
		public override void Save_DIST (BinaryWriter Output)
		{
			for (int i = 0; i < this.PIVS.Count; ++i) {
				GenericIO<Sequence>.Save (Output, this.SEQ[i]);
			}
		}
		
		public override void Build (LAESA idx, int num_pivs, int num_rings, ListIBuilder list_builder)
		{
			throw new NotSupportedException("This method should not be used on this specilized class");
		}
		
		public virtual void Build (LAESA idx, int num_pivs, int num_rings, SequenceBuilder seq_builder = null)
		{
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqPlain (32);
			}
			base.Build (idx, num_pivs, num_rings, null);
			this.SEQ = new Sequence[num_pivs];
			var build_one_pivot = new Action<int>(delegate(int p) {
				var D = this.DIST[p];
				this.SEQ[p] = seq_builder(D, this.MAX_SYMBOL+1);
				if (p % 10 == 0 || p + 1 == num_pivs) {
					Console.Write ("== advance: {0}/{1}, ", p, num_pivs);
					if (p % 100 == 0 || p + 1 == num_pivs) {
						Console.WriteLine ();
					}
				}

			});
			Parallel.For(0, num_pivs, build_one_pivot);
			this.EmulateDIST();
		}

		protected void EmulateDIST ()
		{
			for (int i = 0; i < this.SEQ.Length; ++i) {
				var L = new ListRankSelectSeq ();
				L.Build (this.SEQ [i]);
				this.DIST [i] = L;
			}
		}
	}
}

