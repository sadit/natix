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
//   Original filename: natix/CompactDS/Text/ITextIndex.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	public interface ITextIndex
	{
		int N {
			get;
		}
		int Sigma {
			get;
		}
		int CountOcc (IList<int> q);
		IList<int> Locate (IList<int> q);
		IList<int> Extract (int start_pos, int end_pos);
		void Save(string basename);
		void Load (string basename);
	}
}

