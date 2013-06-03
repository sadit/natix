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
//   Original filename: natix/CompactDS/Lists/Unsorted/ListRL2.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix.CompactDS
{
	/// <summary>
	/// Encodes an array or permutation using a compact representation. Specially useful for lists that
	/// exhibit large runs. It does not support neither consecutive equal items nor negative items.
	/// </summary>
	public class ListIRS64 : ListGenerator<int>, ILoadSave
	{
		public Bitmap64 XLB;
		public int MAXVALUE;

		public ListIRS64()
		{
		}

		public override int Count {
			get {
				return (int)this.XLB.Count1;
			}
		}

		public virtual void Build (IList<int> inlist, int _maxvalue, BitmapFromList64 bitmap_builder = null)
		{
			if (bitmap_builder == null) {
				bitmap_builder = BitmapBuilders.GetDiffSet64 (63, new EliasDelta64());
			}
			int n = inlist.Count;
			var outlist = new List<long> (n);
			long base_value = 0;
			this.MAXVALUE = _maxvalue + 1;
			if (inlist.Count > 0) {
				outlist.Add (inlist [0]);
				for (int i = 1; i < n; i++) {
					var u = inlist [i];
					var d = u - inlist [i - 1];
					if (d == 0) {
						throw new ArgumentException ("ListIBitmap64 doesn't support equal consecutive items");
					}
					if (d < 0) {
						base_value += this.MAXVALUE;
					}
					outlist.Add (u + base_value);
				}
				this.XLB = bitmap_builder (outlist, outlist [outlist.Count - 1]+1);
			} else {
				this.XLB = bitmap_builder (outlist, this.MAXVALUE);
			}
		}

		public virtual void Load (BinaryReader Input)
		{
			this.XLB = GenericIO<Bitmap64>.Load(Input);
			this.MAXVALUE = Input.ReadInt32();
		}

		public virtual void Save (BinaryWriter Output)
		{
			GenericIO<Bitmap64>.Save(Output, this.XLB);
			Output.Write ((int)this.MAXVALUE);
		}

		public override int GetItem (int index)
		{
			var p = this.XLB.Select1(index+1);
			return (int)(p % this.MAXVALUE);
		}

		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}	
	}
}
