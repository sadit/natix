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
	public class ListIDiffs : ListGenerator<int>, IListI
	{
		public IIEncoder32 ENCODER;
		public BitStream32 DIFFS;
		public IRankSelect MARKS;
		public IList<int> ABSPOS;
		public IList<long> OFFSETS;
		public short BLOCKSIZE;

		public ListIDiffs()
		{
		}

		public override int Count {
			get {
				return (int) this.MARKS.Count;
			}
		}

		public virtual void Build (IList<int> inlist, short blocksize,
		                           BitmapFromBitStream marks_builder = null,
		                           IIEncoder32 encoder = null
		)
		{
			if (encoder == null) {
				encoder = new EliasDelta ();
			}
			if (marks_builder == null) {
				marks_builder = BitmapBuilders.GetGGMN_wt(12);
			}
			this.BLOCKSIZE = blocksize;
			this.ENCODER = encoder;
			int n = inlist.Count;
			this.DIFFS = new BitStream32();
			var marks = new BitStream32();
			this.OFFSETS = new List<long>();
			this.ABSPOS = new List<int>();
			if (inlist.Count > 0) {
				for (int i = 0; i < n; i++) {
					var u = inlist [i];
					if (i % this.BLOCKSIZE == 0) {
						marks.Write(true);
						this.ABSPOS.Add(u);
						this.OFFSETS.Add(this.DIFFS.CountBits);
						continue;
					}
					var d = u - inlist [i - 1];
					if (d == 0) {
						throw new ArgumentException ("ListIDiffs doesn't support equal consecutive items");
					}
					if (d < 0) {
						marks.Write(true);
						this.ABSPOS.Add (u);
						this.OFFSETS.Add(this.DIFFS.CountBits);
					} else {
						marks.Write(false);
						this.ENCODER.Encode(this.DIFFS, d);
					}
				}
			}
			this.MARKS = marks_builder(new FakeBitmap(marks));
		}

		public virtual void Load (BinaryReader Input)
		{
			this.ENCODER = IEncoder32GenericIO.Load(Input);
			this.BLOCKSIZE = Input.ReadInt16();
			this.MARKS = RankSelectGenericIO.Load (Input);
			this.ABSPOS = ListIGenericIO.Load(Input);
			this.OFFSETS = PrimitiveIO<long>.ReadFromFile(Input, this.ABSPOS.Count, null);
			this.DIFFS = new BitStream32();
			this.DIFFS.Load(Input);
		}

		public virtual void Save (BinaryWriter Output)
		{
			IEncoder32GenericIO.Save(Output, this.ENCODER);
			Output.Write((Int16) this.BLOCKSIZE);
			RankSelectGenericIO.Save(Output, this.MARKS);
			ListIGenericIO.Save(Output, this.ABSPOS);
			PrimitiveIO<long>.WriteVector(Output, this.OFFSETS);
			this.DIFFS.Save(Output);
		}

		public override int GetItem (int index)
		{
			return this.GetItem(index, new ContextListI());
		}

		public int GetItem (int index, ContextListI ctx)
		{
			// equality
			if (ctx.index == index) {
				return ctx.value;
			}
			var m = this.MARKS.Rank1 (index);
			// fast forward decoding
			if (ctx.index < index && m == ctx.block_id) {
				for (; ctx.index < index; ++ctx.index) {
					ctx.value += this.ENCODER.Decode(this.DIFFS, ctx.ctx);
				}
				return ctx.value;
			}
			ctx.Reset(this.OFFSETS[m-1]);
			ctx.block_id = m;
			ctx.value = this.ABSPOS[m-1];
			ctx.index = index;
			// if the value is the header
			if (this.MARKS.Access(index)) {
				return ctx.value;
			}
			var count = index - this.MARKS.Select1(m);
			var acc = 0;
			for (int i = 0; i < count; ++i) {
				acc += this.ENCODER.Decode(this.DIFFS, ctx.ctx);
			}
			ctx.value += acc;
			return ctx.value;
		}

		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}	
	}
}
