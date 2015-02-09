//
//  Copyright 2013  ericsadit
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;

namespace natix.SimilaritySearch
{
	public class CoPhIR : MetricDB
	{
		public struct PhotoInfo : ILoadSave
		{
			public byte farm; // 1 byte
			public ushort server; // 2 bytes
			public uint id; // 4 bytes
			public long secret; // 8 bytes

			public PhotoInfo(byte _farm, ushort _server, uint _id, long _secret)
			{
				this.farm = _farm;
				this.server = _server;
				this.id = _id;
				this.secret = _secret;
			}

			
			public void Load(BinaryReader Input)
			{
				this.farm = Input.ReadByte ();
				this.server = Input.ReadUInt16 ();
				this.id = Input.ReadUInt32 ();
				this.secret = Input.ReadInt64 ();
			}

			public void Save (BinaryWriter Output)
			{
				Output.Write (this.farm);
				Output.Write (this.server);
				Output.Write (this.id);
				Output.Write (this.secret);
			}

			public string GetLink ()
			{
				return "http://www.flickr.com/photo.gne?id=" + this.id;
			}

			public string GetThumb (char suffix = 's') 
			{
				return String.Format ("http://farm{0}.static.flickr.com/{1}/{2}_{3}_{4}.jpg",
				                      this.farm, this.server, this.id, this.secret.ToString("x"), suffix);
			}

		}

		public class CItem : ILoadSave
		{
			public short[] ScalableColor = new short[64];
			public short[] ColorLayout = new short[12];
			public short[] ColorStructure = new short[64];
			public short[] EdgeHistogram = new short[80];
			public short[] HomogeneousTexture = new short[62];
			public PhotoInfo Photo;

			public CItem()
			{
			}

			public CItem Clone()
			{
				var u = new CItem ();
				u.Photo = new PhotoInfo(this.Photo.farm, this.Photo.server, this.Photo.id, this.Photo.secret);
				this.ScalableColor.CopyTo (u.ScalableColor, 0);
				this.ColorLayout.CopyTo (u.ColorLayout, 0);
				this.ColorStructure.CopyTo (u.ColorStructure, 0);
				this.EdgeHistogram.CopyTo (u.EdgeHistogram, 0);
				this.HomogeneousTexture.CopyTo (u.HomogeneousTexture, 0);
				return u;
			}

			public CItem(XPathNavigator nav)
			{
				var farm = Byte.Parse(nav.GetAttribute("farm", ""));
				var server = UInt16.Parse(nav.GetAttribute("server", ""));
				var id = UInt32.Parse(nav.GetAttribute("id", ""));
				var secret = Int64.Parse(nav.GetAttribute("secret", ""), System.Globalization.NumberStyles.HexNumber);
				this.Photo = new PhotoInfo(farm, server, id, secret);
			}

			public void Load(BinaryReader Input)
			{
				PrimitiveIO<short>.LoadVector (Input, this.ScalableColor.Length, this.ScalableColor);
				PrimitiveIO<short>.LoadVector (Input, this.ColorLayout.Length, this.ColorLayout);
				PrimitiveIO<short>.LoadVector (Input, this.ColorStructure.Length, this.ColorStructure);
				PrimitiveIO<short>.LoadVector (Input, this.EdgeHistogram.Length, this.EdgeHistogram);
				PrimitiveIO<short>.LoadVector (Input, this.HomogeneousTexture.Length, this.HomogeneousTexture);
				this.Photo.Load (Input);
			}

			public void Save (BinaryWriter Output)
			{
				PrimitiveIO<short>.SaveVector (Output, this.ScalableColor);
				PrimitiveIO<short>.SaveVector (Output, this.ColorLayout);
				PrimitiveIO<short>.SaveVector (Output, this.ColorStructure);
				PrimitiveIO<short>.SaveVector (Output, this.EdgeHistogram);
				PrimitiveIO<short>.SaveVector (Output, this.HomogeneousTexture);
				this.Photo.Save (Output);
			}

			public struct OffsetLength
			{
				public int Offset;
				public int Length;
				public OffsetLength(int offset, int len)
				{
					this.Offset = offset;
					this.Length = len;
				}
			}

			public static Dictionary<string,OffsetLength> OFFSETS = new Dictionary<string,OffsetLength>() {
				// ColorLayoutType offsets
				// 0, 1, 2,  3,  4,  5,  6,  7,  8,  9, 10, 11
				// a, b, c, d1, d2, d3, d4, d5, e1, e2, f1, f2
				{"YDCCoeff", new OffsetLength(0, 1)},
				{"CbDCCoeff", new OffsetLength(1, 1)}, 
				{"CrDCCoeff", new OffsetLength(2, 1)},
				{"YACCoeff5", new OffsetLength(3, 5)},
				{"CbACCoeff2", new OffsetLength(8, 2)},
				{"CrACCoeff2", new OffsetLength(10,2)},
				// HomogeneousTextureType offsets
				{"Average", new OffsetLength(0,1)}, // 1
				{"StandardDeviation", new OffsetLength(1,1)}, // 1
				{"Energy", new OffsetLength(2, 30)}, // 30
				{"EnergyDeviation", new OffsetLength(2+30, 30)}, // 30
			};

			public void SetColorLayout(XPathNavigator nav)
			{
				int c = 0;
				foreach (XPathNavigator _nav in nav.Select("*")) {
					// tmp.Clear();
					string name = _nav.Name;
					var s = OFFSETS [name];
					c += s.Length;
					PrimitiveIO<short>.LoadVector (_nav.Value, this.ColorLayout, s.Offset, s.Length);
				}
				if (c != this.ColorLayout.Length) {
					throw new ArgumentOutOfRangeException ("Parsing ColorLayout");
				}
			}

			public void SetHomegeneousTexture(XPathNavigator nav)
			{
				int c = 0;
				foreach (XPathNavigator _nav in nav.Select("*")) {
					string name = _nav.Name;
					var s = OFFSETS [name];
					c += s.Length;
					PrimitiveIO<short>.LoadVector (_nav.Value, this.HomogeneousTexture, s.Offset, s.Length);
				}
				if (c != this.HomogeneousTexture.Length) {
					throw new ArgumentOutOfRangeException ("Parsing HomogeneousTexture");
				}
			}
		}

		public void Load (BinaryReader Input)
		{
			var len = Input.ReadInt32 ();
			this.Items = new List<CItem> (len);
			CompositeIO<CItem>.LoadVector (Input, len, this.Items);
		}

		public void Save (BinaryWriter Output)
		{
			Output.Write (this.Count);
			CompositeIO<CItem>.SaveVector (Output, this.Items);
		}

		static NumericInt16 Num = new NumericInt16 ();

		public double Dist(object _a, object _b)
		{
			++this.numdist;
			var a = _a as CItem;
			var b = _b as CItem;
			//Console.Error.WriteLine ("a: {0}, b: {1}", _a, _b);
			// TODO make the list of weights a parameter
			return 2.0 * Num.DistL1 (a.ScalableColor, b.ScalableColor) +
				3.0 * Num.DistL1 (a.ColorLayout, b.ColorLayout) +
				4.0 * Num.DistL1 (a.ColorStructure, b.ColorStructure) +
				2.0 * Num.DistL1 (a.EdgeHistogram, b.EdgeHistogram) +
				0.5 * Num.DistL1 (a.HomogeneousTexture, b.HomogeneousTexture);
		}

		long numdist = 0;
		string name = "";

		public long NumberDistances {
			get {
				return this.numdist;
			}
		}
		public string Name {
			get {
				return this.name;
			}
			set {
				this.name = value;
			}
		}
		public List<CItem> Items = new List<CItem>();

		public int Count
		{
			get {
				return this.Items.Count;
			}
		}

		public object this[ int i ]
		{
			get {
				return this.Items [i];
			}
		}

		public CoPhIR ()
		{
		}

		public void Build(IList<int> filenames)
		{
		}

		public object Parse(string filename)
		{
			var doc = new XPathDocument (filename);
			return this.Parse (doc);
		}

		public CItem Parse(XPathDocument doc)
		{
			var docnav = doc.CreateNavigator ();
			docnav.MoveToFollowing ("photo", "");
			var item = new CItem (docnav);
			docnav.MoveToFollowing ("Mpeg7", "");
			docnav.MoveToFollowing ("Image", "");
			foreach (XPathNavigator nav in docnav.Select("*")) {
				var _type = nav.GetAttribute ("type", "");
				//Console.WriteLine("type: {0}", _type);
				//Console.WriteLine(nav.OuterXml);
				switch (_type) {
				case "ScalableColorType":
					PrimitiveIO<short>.LoadVector (nav.Value, item.ScalableColor, 0, item.ScalableColor.Length);
					break;
				case "ColorStructureType":
					PrimitiveIO<short>.LoadVector (nav.Value, item.ColorStructure, 0, item.ColorStructure.Length);
					break;
				case "ColorLayoutType":
					item.SetColorLayout (nav);
					break;
				case "EdgeHistogramType":
					PrimitiveIO<short>.LoadVector (nav.Value, item.EdgeHistogram, 0, item.EdgeHistogram.Length);
					break;
				case "HomogeneousTextureType":
					item.SetHomegeneousTexture (nav);
					break;
				default:
					throw new ArgumentException (String.Format("Unknown node type '{0}'"));
				}
			}
			return item;
		}

		public void Add(XPathDocument doc)
		{
			this.Items.Add (this.Parse(doc));
		}
		public void Add(string filename)
		{
			var o = (CItem)this.Parse (filename);
			this.Items.Add (o);
		}

		public void Extend(CoPhIR other)
		{
			{
				var newsize = Math.Max (this.Items.Count * 2, this.Items.Count + other.Items.Count);
				if (newsize > this.Items.Capacity) {
					this.Items.Capacity = newsize;
				}	
			}
			foreach (var o in other.Items) {
				this.Items.Add (o);
			}
		}
	}
}

