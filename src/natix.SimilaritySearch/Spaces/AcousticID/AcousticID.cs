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
using System.IO;
using System.Collections.Generic;
using natix.CompactDS;

namespace natix.SimilaritySearch
{
	public class AcousticID : MetricDB
	{
		public struct AcousticItem {
			public string id;
			public int len;
			public int[] fp;
		}

		List<AcousticItem> Items;
		int Dim;

		public AcousticID () : base()
		{
		}

		/// <summary>
		/// Get/Set (and load) the database name
		/// </summary>
		public string Name {
			get;
			set;
		}

		public object this [int index] {
			get {
				return this.Items [index];
			}
		}

		protected long numdist;

		public long NumberDistances {
			get {
				return this.numdist;
			}
		}
	
		public int Count {
			get {
				return this.Items.Count;
			}
		}


		public virtual void Load (BinaryReader Input)
		{
			this.Name = Input.ReadString ();
			this.Dim = Input.ReadInt32 ();
			int n = Input.ReadInt32 ();

			this.Items = new List<AcousticItem> ();

			for (int i = 0; i < n; ++i) {
				var id = Input.ReadString ();
				var len = Input.ReadInt32 ();
				var fp = new int[this.Dim];
				PrimitiveIO<int>.LoadVector (Input, this.Dim, fp);
				var item = new AcousticItem () {
					id = id,
					len = len,
					fp = fp
				};
				this.Items.Add (item);
			}
		}

		public virtual void Save(BinaryWriter Output)
		{
			Output.Write(this.Name);
			Output.Write(this.Dim);
			Output.Write (this.Items.Count);

			for (int i = 0; i < this.Items.Count; ++i) {
				var item = this.Items [i];
				Output.Write (item.id);
				Output.Write (item.len);
				PrimitiveIO<int>.SaveVector (Output, item.fp);
			}
		}

		static int k0 = "FILE=".Length;
		static int k1 = "DURATION=".Length;
		static int k2 = "FINGERPRINT=".Length;


		public void Build(string filename, int dim)
		{
			this.Name = filename;
			this.Dim = dim;
			this.Items = new List<AcousticItem> ();

			using (var input = File.OpenText(filename)) {
				int recID = 0;
				while (!input.EndOfStream) {
					var audioID = input.ReadLine ().Substring (k0);
					var duration = int.Parse (input.ReadLine ().Substring (k1));
					var fp = this.ParseVector (input.ReadLine ().Substring (k2));
					this.Items.Add (new AcousticItem () {
						id = audioID,
						len = duration,
						fp = fp
					});
					++recID;
					if (recID % 1000 == 0) {
						Console.WriteLine ("== object {0}, now: {1}", recID, DateTime.Now);
					}
				}
			}
		}

		public int Add(object a)
		{
			var s = a as string;
			var arr = s.Split ('\n');

			var audioID = arr[0].Substring (k0);
			var duration = int.Parse (arr[1].Substring (k1));
			var fp = this.ParseVector (arr[2].Substring (k2));
			this.Items.Add (new AcousticItem () {
				id = audioID,
				len = duration,
				fp = fp
			});
			return this.Items.Count - 1;
		}

		public int[] ParseVector(string vstring)
		{
			var array = vstring.Split(',');
			var vec = new int[this.Dim];

			for (int i = 0; i < this.Dim && i < array.Length; ++i) {
				vec [i] = int.Parse (array [i]);
			}
			return vec;
		}

		/// <summary>
		/// Returns a vector from an string
		/// </summary>
		public object Parse (string s)
		{
			if (s.StartsWith ("FILE=")) {
				s = s.Split ('\n') [2];
				s = s.Substring(k2).Trim();
			}
			return this.ParseVector (s);
		}

		/// <summary>
		/// Distance wrapper for any P-norm
		/// </summary>
		public virtual double Dist (object a, object b)
		{
			++this.numdist;
			var d = 0;

			var xa = (int[])a;
			var xb = (int[])b;

			for (int i = 0; i < this.Dim; ++i) {
				uint x = (uint) (xa[i] ^ xb[i]);
				d += Bits.PopCount8 [x & 255];
				d += Bits.PopCount8 [(x >> 8) & 255];
				d += Bits.PopCount8 [(x >> 16) & 255];
				d += Bits.PopCount8 [x >> 24];
			}
			return d;
		}
	}
}
