//
//  Copyright 2013  Eric Sadit Tellez Avila <donsadit@gmail.com>
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
using System.Collections.Generic;
using natix;
using natix.SimilaritySearch;
using natix.CompactDS;
using natix.SortingSearching;
using natix.Sets;
using System.IO;
using System.Collections;
using System.Threading;
using System.Text;

namespace imgsearchweb
{
	public class CoPhIR_HFP : ICoPhIR_Handler
	{
		public Index ImgIndex = null;
		string IndexName = "Index.knrseq.DB.CoPhIR-282-1M.sapir-100.knr=7.num_refs=2048";
		CoPhIR.PhotoInfo[] PhotoInfo;
		Random random = new Random ();

		public CoPhIR_HFP(Index index, string indexname, string photinfoname)
		{
			this.IndexName = indexname;
			this.ImgIndex = index;
			this.PhotoInfo = new CoPhIR.PhotoInfo[this.ImgIndex.DB.Count];
			using (var Input = new BinaryReader(File.OpenRead(photinfoname))) {
				CompositeIO<CoPhIR.PhotoInfo>.LoadVector (Input, this.PhotoInfo.Length, this.PhotoInfo);
			}
		}

		public string CurrentConfiguration ()
		{			
			var knrseq = this.ImgIndex as KnrSeqSearch;
			if (knrseq == null) {
				return String.Format (@"<ul><li>{0} {1}</li></ul>",
				                      this.IndexName, this.ToString());
			} else {
				return String.Format (@"<ul><li>{0} {1}</li><li> compression-method: {2} </li> <li>max-cand: {3}</li></ul>",
				                      this.IndexName, this.ToString(), knrseq.SEQ.ToString (), knrseq.MAXCAND);
			}

		}

		public IResult SearchKNN (int qid, int k)
		{
			try {
				var R = this.ImgIndex.SearchKNN (this.ImgIndex.DB[qid], k);
				return R;
			} catch (Exception error) {
				Console.WriteLine ("Exception in SearchKNN {0}", error);
				throw error;
			}
		}

		public int GetRandomQueryId ()
		{
			return (int)(this.random.NextDouble () * this.ImgIndex.DB.Count);
		}

		public string GetLink(int objID)
		{
			return this.PhotoInfo[objID].GetLink();
		}

		public string GetThumb(int objID, char suffix = 's')
		{
			return this.PhotoInfo [objID].GetThumb (suffix);
		}

	}
}
