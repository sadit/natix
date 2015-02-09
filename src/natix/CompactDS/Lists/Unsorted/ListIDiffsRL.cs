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
	public class ListIDiffsRL : ListGenerator<int>, ILoadSave
	{
		public IIEncoder32 ENCODER;
		public BitStream32 DIFFS;
		public Bitmap MARKS;
		public IList<int> ABSPOS;
		public IList<long> OFFSETS;
		public short BLOCKSIZE;

		public ListIDiffsRL()
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
			var run_len = 0;
			if (inlist.Count > 0) {
				for (int i = 0; i < n; i++) {
					var u = inlist [i];
					if (i % this.BLOCKSIZE == 0) {
						this.commit_run(ref run_len);
						marks.Write(true);
						this.ABSPOS.Add(u);
						this.OFFSETS.Add(this.DIFFS.CountBits);
						continue;
					}
					var d = u - inlist [i - 1];
					if (d == 0) {
						throw new ArgumentException ("ListIDiffsRL doesn't support equal consecutive items");
					}
					if (d < 0) {
						this.commit_run(ref run_len);
						marks.Write(true);
						this.ABSPOS.Add (u);
						this.OFFSETS.Add(this.DIFFS.CountBits);
					} else {
						marks.Write(false);
						if (d==1) {
							run_len++;
						} else {
							this.commit_run(ref run_len);
							this.ENCODER.Encode(this.DIFFS, d);
						}
					}
				}
			}
			this.commit_run(ref run_len);
			this.MARKS = marks_builder(new FakeBitmap(marks));
		}

		protected void commit_run (ref int run_len)
		{
			if (run_len > 0) {
				this.ENCODER.Encode(this.DIFFS, 1);
				this.ENCODER.Encode(this.DIFFS, run_len);
				run_len = 0;
			}
		}

		public virtual void Load (BinaryReader Input)
		{
			this.ENCODER = IEncoder32GenericIO.Load(Input);
			this.BLOCKSIZE = Input.ReadInt16();
			this.MARKS = GenericIO<Bitmap>.Load (Input);
			this.ABSPOS = ListIGenericIO.Load(Input);
			this.OFFSETS = PrimitiveIO<long>.LoadVector(Input, this.ABSPOS.Count, null);
			this.DIFFS = new BitStream32();
			this.DIFFS.Load(Input);
		}

		public virtual void Save (BinaryWriter Output)
		{
			IEncoder32GenericIO.Save(Output, this.ENCODER);
			Output.Write((Int16) this.BLOCKSIZE);
			GenericIO<Bitmap>.Save(Output, this.MARKS);
			ListIGenericIO.Save(Output, this.ABSPOS);
			PrimitiveIO<long>.SaveVector(Output, this.OFFSETS);
			this.DIFFS.Save(Output);
		}

		public override int GetItem (int index)
		{
			return this.GetItem(index, new ContextListI());
		}

		protected int Decode (ContextListI ctx)
		{
			if (ctx.run_len > 0) {
				ctx.run_len--;
				return 1;
			} else {
				var d = this.ENCODER.Decode (this.DIFFS, ctx.ctx);
				if (d == 1) {
					ctx.run_len = this.ENCODER.Decode(this.DIFFS, ctx.ctx) - 1;
				}
				return d;
			}
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
				while (ctx.index < index) {
					ctx.value += this.Decode(ctx);
					ctx.index++;
					if (ctx.index == index) {
						break;
					}
					if (ctx.run_len > 0) {
						if (ctx.index + ctx.run_len <= index) {
							ctx.value += ctx.run_len;
							ctx.index += ctx.run_len;
							ctx.run_len = 0;
						} else {
							var step = ctx.index + ctx.run_len - index;
							ctx.value += step;
							ctx.index += step;
							ctx.run_len -= step;
						}
					}
				}
				return ctx.value;
			}
			ctx.Reset(this.OFFSETS[m-1]);
			ctx.block_id = m;
			ctx.value = this.ABSPOS[m-1];
			// if the value is the header
			if (this.MARKS.Access(index)) {
				ctx.index = index;
				return ctx.value;
			}
			ctx.index = this.MARKS.Select1(m);
			while (ctx.index < index) {
				ctx.value += this.Decode(ctx);
				ctx.index++;
				if (ctx.index == index) {
					break;
				}
				if (ctx.run_len > 0) {
					//Console.WriteLine ("A index: {0}, value: {1}, runlen: {2}, query index: {3}", ctx.index, ctx.value, ctx.run_len, index);
					if (ctx.index + ctx.run_len <= index) {
						//Console.WriteLine ("B index: {0}, value: {1}, runlen: {2}", ctx.index, ctx.value, ctx.run_len);
						ctx.value += ctx.run_len;
						ctx.index += ctx.run_len;
						ctx.run_len = 0;
					} else {				
						//Console.WriteLine ("C index: {0}, value: {1}, runlen: {2}", ctx.index, ctx.value, ctx.run_len);
						var step = Math.Min(index - ctx.index, ctx.run_len);
						ctx.value += step;
						ctx.index += step;
						ctx.run_len -= step;
					}
					//Console.WriteLine ("Z index: {0}, value: {1}, runlen: {2}", ctx.index, ctx.value, ctx.run_len);
				}
			}
			return ctx.value;
		}

		public override void SetItem (int index, int u)
		{
			throw new NotImplementedException ();
		}	
	}
}
