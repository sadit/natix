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
//   Original filename: natix/CompactDS/Bitmaps/SArrayHGGMN.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace natix.CompactDS
{
	/// <summary>
	/// sarray supporting constant Select0 in H. ALENEX 2007. Okanohara & Sadakane. Practical Rank & Select.
	/// </summary>
	public class SArrayHGGMN : SArray
	{
		override protected void CreateH (IBitStream BH, short Brank, int Bselect)
		{
			var _H = new GGMN ();
			_H.Build (BH, Brank);
			// _H.Build (BH, Brank, Bselect);
			this.H = _H;
		}
		
		override protected void LoadH (BinaryReader br)
		{
			this.H = new GGMN ();
			this.H.Load (br);
		}
	}
}
