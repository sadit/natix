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
//   Original filename: natix/SimilaritySearch/Indexes/Sequential.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Reflection;
using NDesk.Options;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The sequential index
	/// </summary>
	public class Sequential : BasicIndex
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public Sequential ()
		{
		}

		/// <summary>
		/// API build command
		/// </summary>
		public virtual void Build (MetricDB db)
		{
			this.DB = db;
		}

		/// <summary>
		/// Search by range
		/// </summary>
		/// <param name="q">
		/// Query object 
		/// </param>
		/// <param name="radius">
		/// Radius <see cref="System.Double"/>
		/// </param>
		/// <returns>
		/// The result set <see cref="Result"/>
		/// </returns>
		public override IResult SearchRange (object q, double radius)
		{
			int L = this.DB.Count;
			var r = new Result (L);
			for (int docid = 0; docid < L; docid++) {
				double d = this.DB.Dist (q, this.DB[docid]);
				if (d <= radius) {
					r.Push (docid, d);
				}
			}
			return r;
		}
		
		/// <summary>
		/// KNN Search
		/// </summary>
		/// <param name="q">
		/// Query object 
		/// </param>
		/// <param name="k">
		/// The number of nearest neighbors 
		/// </param>
		/// <returns>
		/// The result set <see cref="IResult"/>
		/// </returns>
		public override IResult SearchKNN (object q, int k, IResult R)
		{
			int L = this.DB.Count;
			for (int docid = 0; docid < L; docid++) {
				double d = this.DB.Dist (q, this.DB[docid]);
				R.Push (docid, d);
			}
			return R;
		}

	}
}
