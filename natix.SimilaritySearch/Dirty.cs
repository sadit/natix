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
//   Original filename: natix/SimilaritySearch/Dirty.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace natix.SimilaritySearch
{
	/// <summary>
	/// This class makes the dirty job (serialize and deserialize objects) and other necessary but dirty jobs
	/// </summary>
	public class Dirty
	{
		static Type[] safe_storage_types = new Type[] {
			typeof(int),
			typeof(uint),
			typeof(byte),
			typeof(sbyte),
			typeof(short),
			typeof(ushort),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(string),
			typeof(char)
		};
		
		public static bool MustSave (object v)
		{
			var type = v.GetType ();
			foreach (var safe_type in safe_storage_types) {
				if (type == safe_type) {
					return true;
				}
			}
			return false;
		}
		
		public static bool MustSaveType (Type type)
		{
			foreach (var safe_type in safe_storage_types) {
				if (type == safe_type) {
					return true;
				}
			}
			return false;
		}
		
		public static void SaveIndexXml (string name, Index obj)
		{
			var m = File.CreateText (name);
			XmlWriter w = new XmlTextWriter (m);
			w.WriteStartDocument ();
			w.WriteRaw ("\n");
			w.WriteStartElement ("IndexObject");
			w.WriteRaw ("\n");
			foreach (var v in obj.GetType ().GetFields ()) {
				if (v.IsStatic && !v.IsPublic && !v.IsLiteral && !MustSaveType (v.FieldType)) {
					continue;
				}
				// var _value = v.GetValue (obj);
				w.WriteStartElement (v.Name);
				try {
					w.WriteValue (v.GetValue (obj));
				} catch (Exception e) {
					Console.WriteLine ("Serializing field '{0}'", v.Name);
					Console.WriteLine (e.StackTrace);
					throw e;
				}
				w.WriteEndElement ();
				w.WriteRaw ("\n");
			}
			foreach (var p in obj.GetType ().GetProperties ()) {
				if (p.CanRead && p.CanWrite && !p.PropertyType.IsGenericType && MustSaveType (p.PropertyType)) {
					w.WriteStartElement (p.Name);
					var _value = p.GetValue (obj, null);
					w.WriteValue (_value);
					w.WriteEndElement ();
					w.WriteRaw ("\n");
				}
			}
			w.WriteEndElement ();
			w.WriteRaw ("\n");
			w.WriteEndDocument ();
			// throw new NotImplementedException ();
			m.Close ();
			// IndexLoader.Load (name);
		}
		public static Index LoadIndexXml (string name, Type type)
		{
			TextReader m = File.OpenText (name);
			XmlReader R = new XmlTextReader (m);
			// Type type = obj.GetType ();
			Index obj = (Index)Activator.CreateInstance (type);
			R.ReadStartElement ();
			while (!R.EOF) {
				R.Read ();
				if (R.NodeType != XmlNodeType.Element) {
					continue;
				}
				var pname = R.Name;
				// Console.WriteLine (pname);
				object v = null;
				var prop = type.GetProperty (pname);
				var field = type.GetField (pname);
				if (prop != null) {
					// v = R.ReadContentAs (prop.PropertyType, null);
					// v = R.ReadElementContentAsString ();
					v = R.ReadElementContentAs (prop.PropertyType, null);
					prop.SetValue (obj, v, null);
					//Console.WriteLine ("PROPERTY*** pname: {0}, {1}, val: {2}", pname, prop.ReflectedType, v);
				} else if (field != null) {
					// Console.WriteLine ("FIELD****** pname: {0}, field-reflector: {1}, field-type: {2}", pname, field.ReflectedType, field.FieldType);
					v = R.ReadElementContentAs (field.FieldType, null);
					field.SetValue (obj, v);
				} else {
					throw new ArgumentException (String.Format ("Property or field {0} was not found for type", pname));
				}
			}
			// Console.WriteLine (obj.GetType().Name);
			m.Close ();
			return obj;
		}

		/// <summary>
		/// Serialize using XML
		/// </summary>
		public static void SerializeXml (string name, object obj)
		{
			FileStream m = new FileStream (name, FileMode.Create);
			XmlSerializer s = new XmlSerializer (obj.GetType ());
			s.Serialize (m, obj);
			m.Close ();
		}
		
		/// <summary>
		/// Deserialize XML
		/// </summary>

		public static object DeserializeXml (string name, Type t)
		{
			FileStream m = new FileStream (name, FileMode.Open);
			XmlSerializer s = new XmlSerializer (t);
			object o = s.Deserialize (m);
			m.Close ();
			return o;
		}
		/// <summary>
		/// Serialize binary objects
		/// </summary>
		public static void SerializeBinary (string name, object o)
		{
			FileStream f = new FileStream (name, FileMode.Create);
			SerializeBinary (f, o);
			f.Close ();
		}
		/// <summary>
		/// Serialize to binary
		/// </summary>
		public static void SerializeBinary (FileStream f, object o)
		{
			IFormatter b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
			b.Serialize (f, o);
		}

		/// <summary>
		/// Deserialize to binary object
		/// </summary>
		public static object DeserializeBinary (string name)
		{
			FileStream f = new FileStream (name, FileMode.Open);
			object o = DeserializeBinary (f);
			f.Close ();
			return o;
		}
		
		/// <summary>
		/// Serialize to binary
		/// </summary>
		public static object DeserializeBinary (FileStream f)
		{
			IFormatter b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
			return b.Deserialize (f);
			
		}

		/// <summary>
		/// Get the type by name for vectors
		/// </summary>
		public static Type VectorType (string name)
		{
			TypeCode code = (TypeCode)Enum.Parse (typeof(TypeCode), name);
			return VectorType (code);
		}
		
		/// <summary>
		/// Get the type by code id for vectors
		/// </summary>
		public static Type VectorType (TypeCode code)
		{
			switch (code) {
			case TypeCode.Double:
				return typeof(Double);
			case TypeCode.Single:
				return typeof(Single);
			case TypeCode.UInt16:
				return typeof(UInt16);
			case TypeCode.UInt32:
				return typeof(UInt32);
			case TypeCode.Int16:
				return typeof(Int16);
			case TypeCode.Int32:
				return typeof(Int32);
			default:
				throw new NotImplementedException (String.Format("There's not implementation for VectorType {0}", code));
			}
		}

		public static string CombineRelativePath(string basePath, string anotherPath)
		{
			return Path.Combine(Path.GetDirectoryName(basePath), anotherPath);
		}
		/// <summary>
		/// Compute the relative path from the index to another file.
		/// </summary>
		public static string ComputeRelativePath (string basePath, string anotherPath)
		{
			// standardizing paths
			var I = Path.GetFullPath (basePath);
			var A = Path.GetFullPath (anotherPath);
			var ListI = I.Split (Path.DirectorySeparatorChar);
			var ListA = A.Split (Path.DirectorySeparatorChar);
			var m = Math.Min (ListI.Length, ListA.Length);
			int i = 0;
			while (i < m) {
				// Console.WriteLine ("ListI[i] = {0}, ListA[i] = {1}, i = {2}", ListI[i], ListA[i], i);
				if (ListI[i] == ListA[i]) {
					i++;
				} else {
					break;
				}
			}
			string sep = Path.DirectorySeparatorChar.ToString ();
			// Console.WriteLine ("xxxxx sep {0}", sep);
			// I = String.Join (sep, ListI, i, ListI.Length - i);
			A = String.Join (sep, ListA, i, ListA.Length - i);
			m = ListI.Length - i;
			while (m > 1) {
				A = Path.Combine ("..", A);
				m--;
			}
			// Console.WriteLine ("*******> I: {0}, A: {1}", I, A);
			return A;
		}
	}
}
