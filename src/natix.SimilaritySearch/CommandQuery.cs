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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// The pair to be searched
	/// </summary>
	public struct CommandQuery
	{
		/// <summary>
		/// Query Type
		/// </summary>
		public bool QTypeIsRange;
		/// <summary>
		/// Query Argument 
		/// </summary>
		public double  QArg;
		/// <summary>
		/// Query object in string representation. Used only if QObj is null
		/// </summary>
		public string QRaw;
		/// <summary>
		/// The query object. If it is null then QRaw will be parsed.
		/// </summary>
		public object QObj;
		/// <summary>
		/// Constructor
		/// </summary>

		public CommandQuery (string qraw, double qarg, bool qtypeisrange, object qobj = null)
		{
			this.QRaw = qraw;
			this.QArg = qarg;
			this.QTypeIsRange = qtypeisrange;
			this.QObj = qobj;
		}	

		public double EncodeQTypeQArgInSign ()
		{
			if (this.QTypeIsRange) {
				return Math.Abs (this.QArg);
			} else {
				if (this.QArg > 0) {
					return -this.QArg;
				} else {
					return this.QArg;
				}
			}
		}
	}
}
