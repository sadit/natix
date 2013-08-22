//
//  Copyright 2013  ericsadit
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

namespace imgsearchweb
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Web;
	using System.Web.SessionState;
	using System.IO;
	using natix;
	using natix.SimilaritySearch;
	public class Global : System.Web.HttpApplication
	{
		public static ICoPhIR_Handler Cophir;
		public static Object LoadLock = new object ();
		protected void Application_Start (Object sender, EventArgs e)
		{
			var dataroot = "/Users/ericsadit/data-cophir282/";
			SpaceGenericIO.NormalizePath = (string name) =>  dataroot + Path.GetFileName (name);
			IndexGenericIO.NormalizePath = (string name) =>  dataroot + Path.GetFileName (name);

			if (false) {
//				var indexname = "Index.eptable-optimize.numgroups=4.max_pivots=1000.beta=0.8.DB.CoPhIR-282-1M.sapir-100";
//				var index = IndexGenericIO.Load (indexname);
				var indexname = "Index.knrseq.DB.CoPhIR-282-1M.sapir-100.knr=7.num_refs=2048";
				var index = new KnrSeqSearchFootrule( (KnrSeqSearch) IndexGenericIO.Load (indexname) );
//				var index = new KnrSeqSearchJaccard( (KnrSeqSearch) IndexGenericIO.Load (indexname) );
				index.MAXCAND = 60000;
				index.MAXCAND = -100;
				Cophir = new CoPhIR_Original (index, indexname);
			} else {
				var indexname = "Index.knrseqsearch.num_refs=2048.knr=7.HFP-512.DB.CoPhIR-282-1M.sapir-100";
				// var index = new KnrSeqSearchJaccard( (KnrSeqSearch) IndexGenericIO.Load (indexname) );
				var index = new KnrSeqSearchFootrule( (KnrSeqSearch) IndexGenericIO.Load (indexname) );
				index.MAXCAND = 60000;
				index.MAXCAND = -100;
//				var indexname = "Index.eptable-optimize.numgroups=4.max_pivots=1000.beta=0.8.HFP-512.DB.CoPhIR-282-1M.sapir-100";
//				var index = IndexGenericIO.Load (indexname);
			    Cophir = new CoPhIR_HFP(index, indexname, dataroot + "PhotoInfo-DB.CoPhIR-282-1M.sapir-100");
				// Cophir = new CoPhIR_HFP("Index.knrseqsearch.num_refs=2048.knr=7.HFP-256.DB.CoPhIR-282-1M.sapir-100", "PhotoInfo-DB.CoPhIR-282-1M.sapir-100", 60000);
				// Cophir = new CoPhIR_HFP("Index.knrseqsearch.num_refs=2048.knr=7.HFP-128.DB.CoPhIR-282-1M.sapir-100", "PhotoInfo-DB.CoPhIR-282-1M.sapir-100", 60000);
			}
		}

//		protected void Application_Start (Object sender, EventArgs e)
//		{
//		}
		protected void Session_Start (Object sender, EventArgs e)
		{
		}

		protected void Application_BeginRequest (Object sender, EventArgs e)
		{
		}

		protected void Application_EndRequest (Object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest (Object sender, EventArgs e)
		{
		}

		protected void Application_Error (Object sender, EventArgs e)
		{
		}

		protected void Session_End (Object sender, EventArgs e)
		{
		}

		protected void Application_End (Object sender, EventArgs e)
		{
		}
	}
}

