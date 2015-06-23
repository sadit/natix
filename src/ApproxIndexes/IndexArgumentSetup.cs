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

namespace ApproxIndexes
{
	public class IndexArgumentSetup
	{
		public string PREFIX = "data-";
		public string DATABASE = null;
		public string BINARY_DATABASE = null;
		public string QUERIES = null;
		public int CORES = -1;
		public int SPAWN = 1;
		public double QARG = -30;
		//		public bool ExecuteParameterless = false;
		public bool ExecuteSearch = true; // a false value means "only create indexes"

		public bool ExecuteSequential = false;

		public List<int> OPTSEARCH_RESTARTS = new List<int> ();
		public List<int> OPTSEARCH_NEIGHBORS = new List<int> ();
		public List<int> OPTSEARCH_BEAMSIZE = new List<int> ();

		public List<int> KNR_NUMREFS = new List<int>();
		public List<int> KNR_KBUILD = new List<int>();
		public List<int> KNR_KSEARCH = new List<int>();
		public List<double> KNR_MAXCANDRATIO = new List<double>();

	
		public List<int> LSHFloatVector_INDEXES = new List<int>();
		public List<int> LSHFloatVector_SAMPLES = new List<int>();

		public List<double> NeighborhoodHash_ExpectedRecall = new List<double>();
		public List<int> NeighborhoodHash_MaxInstances = new List<int>();

		public IndexArgumentSetup()
		{
		}
	}
}