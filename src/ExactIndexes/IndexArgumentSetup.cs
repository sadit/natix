//
//   Copyright 2014 Eric S. Tellez <eric.tellez@infotec.com.mx>
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

using System;
using System.Collections.Generic;

namespace ExactIndexes
{
	public class IndexArgumentSetup
	{
		public string PREFIX = "data-";
		public string DATABASE = null;
		public string BINARY_DATABASE = null;
		public string QUERIES = null;
		public int CORES = -1;
		public double QARG = -1;
		public bool ExecuteParameterless = false;
		public bool ExecuteSearch = true; // a false value means "only create indexes"

		public List<int> LAESA = new List<int>();
		public List<int> BNC = new List<int> ();
		public List<int> SPA = new List<int> ();
		public List<int> EPT = new List<int> ();
		public List<int> MILC = new List<int> ();
		public List<int> KVP = new List<int> ();
		public int KVP_Available = 1024;

		public List<int> LC = new List<int> ();
		public List<double> SSS = new List<double> ();
		public int SSS_max = 512;
	
		public IndexArgumentSetup()
		{
		}
	}
}