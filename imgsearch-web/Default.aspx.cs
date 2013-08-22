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


using natix;
using natix.SimilaritySearch;
using natix.CompactDS;
using natix.SortingSearching;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace imgsearchweb
{
	public partial class Default : System.Web.UI.Page
	{
		int defaultK = 30;
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			string qidstring = Request["Search"];
			if (qidstring != null) {
				SearchImage (int.Parse (qidstring));
			} else {
				if (Request["About"] == "true") {
					ShowAboutMessage ();
				} else if (Request["Histogram"] == "true") {
					ShowHistogram (Request["qurl"], Request["ourl"], Request["dist"]);
				} else {
					ShowRandomImages(this.defaultK);
				}
			}
		}

		public virtual void ShowAboutMessage ()
		{
			TextData.InnerHtml = @"
        <p>A simple demonstration of a Image Search by Content.</p>
		<p>
		The images are shown from <a href='http://www.flickr.com' title='Flickr'>Flickr</a> site.
		</p>
		<p>
		We use the <a href='http://github.com/sadit/natix>NATIX</a> library for indexing and searching the MPEG7 vectors
		of the <a href='http://cophir.isti.cnr.it/'>CoPhIR</a> database.
		</p>
		<p>Please notice that this site may contain adult content, because they were not filtered at all</p>
";
		}

		public virtual void ShowRandomImages (int K)
		{
			var R = new ResultTies (K, true);
			for (int i = 0; i < K; i++) {
				R.Push (Global.Cophir.GetRandomQueryId (), 0);
			}
			Welcome.InnerHtml = String.Format ("Showing {0} random seed images", R.Count);
			ShowImages (R, true, null);
		}
		
		List<float> HistDist = null;
		List<float> HistAcc = null;
		BinarySearch<float> HistBinSearch;
		public virtual void ShowHistogram (string qurl, string ourl, string dist)
		{
			Welcome.InnerHtml = "Nothing useful yet!";
			return;
			if (this.HistDist == null) {
				this.HistDist = new List<float> ();
				this.HistAcc = new List<float> ();
				this.HistBinSearch = new BinarySearch<float> ();
				int i = 0;
				float acc = 0;
				foreach (var line in File.ReadAllLines("Examples/imgsearch/hist.data")) {
					var M = line.Split();
					acc += float.Parse(M[1]);
					if (i % 50 == 0) {
						this.HistDist.Add(float.Parse(M[0]));
						this.HistAcc.Add(acc);
					}
				}
			}
			int pos;
			this.HistBinSearch.Search(float.Parse(dist), this.HistDist, out pos, 0, this.HistDist.Count);
			float pc = 100 * this.HistAcc[ pos ] / this.HistAcc[ this.HistAcc.Count - 1 ];
			Welcome.InnerHtml = string.Format (@"
				<table><tbody>
					<tr><td style='border: 1pt solid gray; padding: 10pt; text-align: center;'>
					<p>The image <br /><img src='{0}' /><br/> has a distance of {1} against<br /> <img src='{2}' /></p>
				</td><td style='border: 1pt solid gray; padding: 10pt; text-align: center;'>
					<p>Their relative closeness, taking into account the whole
					database, is in the percentile {3:0.00}%, meaning that the {4:0.00}% of
				the database is farther than it to the query.
					</p><p>Graphically we can interpole the value from the following histogram of distances
					<br />
					<img width='300' height='300' src='histcophir.jpg' /></p>
					The x axis is the distance. The y axis is for the average frequency computed using 200 random queries.

						</td></tr></tbody></table>
							", qurl, dist, ourl, pc, 100 - pc);
		}
		
		public virtual void SearchImage (int qid)
		{
			// int qid = c.GetRandomQueryId ();
			long startTicks = System.DateTime.Now.Ticks;
			var cophir = Global.Cophir;
			IResult R = cophir.SearchKNN (qid, this.defaultK);
			TimeSpan tspan = TimeSpan.FromTicks (System.DateTime.Now.Ticks - startTicks);
			string img = String.Format ("<a href='{0}' target='_blank'><img style='border: 0pt;' src='{1}' /></a>",
				cophir.GetLink (qid), cophir.GetThumb (qid, 's'));
			Welcome.InnerHtml = String.Format (@"
							<table style='font-size: smaller; margin-left: auto; margin-right: auto; width: 60%;'><tbody><tr>
							<td style='width: 20%'>{2} </td>
				<td style='width: 30%; vertical-align: top;'>Search time: {3:0.00} sec. Showing {0} images as result for query {1} </td>
				<td style='width: 50%; vertical-align: top;'>{4}</td>
				</tr></tbody></table>
					",
				R.Count, qid, img, tspan.TotalSeconds, cophir.CurrentConfiguration ());
			ShowImages (R, false, cophir.GetThumb (qid, 's'));
		}
		
		public void ShowImages (IResult R, bool showing_random, string qurl)
		{
			int Ic = 0;
			var cophir = Global.Cophir;
			//ImageData[] Idata = CoPhIR._ImageDataIndex.GetManyImageData (R, R.Count);
			StringWriter w = new StringWriter ();
			w.WriteLine ("<table style='font-size: smaller; width: 60%; margin-left: auto; margin-right: auto;'><tbody><tr>");
			foreach (var p in R) {
				if (Ic != 0 && (Ic % 6) == 0) {
					w.WriteLine ("</tr><tr style='padding: 0.5cm;'>");
				}
				Ic++;
				var urlsearch = "?Search=" + p.docid.ToString ();
				w.WriteLine ("<td class='thinborder' style='vertical-align: bottom; text-align: center; '>");
				w.WriteLine ("<a href='{0}' target='_blank'><img style='border: 0pt;' src='{1}' /></a>", cophir.GetLink (p.docid), cophir.GetThumb (p.docid, 's'));
				w.WriteLine ("<div style='padding: 0.1cm;'><a href='{0}'>Search similar</a>", urlsearch);
				if (!showing_random) {
					w.WriteLine("<div>distance: {0} (<a href='?Histogram=true&dist={0}&qurl={1}&ourl={2}'>More</a>)</div>",
						p.dist, qurl, cophir.GetThumb(p.docid, 's'));
				}
				w.WriteLine ("</div></td>");
			}
			w.WriteLine ("</tr></tbody></table>");
			Result.InnerHtml = w.ToString ();	
		}
	}
}

