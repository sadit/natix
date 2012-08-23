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
//   Original filename: natix/Numeric/Numeric.cs
// 
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{

	/// <summary>
	/// Creates a numeric manager from the specified type
	/// </summary>
	public class Numeric
	{
		/// <summary>
		/// Get a numeric manager for the given numeric type
		/// </summary>
		public static INumeric<T> Get<T> ()
		{
			return (INumeric<T>)Get (typeof(T));
		}
		/// <summary>
		/// Get a numeric manager for the given numeric type
		/// </summary>
		public static object Get (Type type)
		{
			TypeCode code = Type.GetTypeCode (type);
			switch (code) {
			case TypeCode.Char:
				return new NumericChar ();
			case TypeCode.SByte:
				return new NumericSByte ();
			case TypeCode.Byte:
				return new NumericByte ();
			case TypeCode.Double:
				return new NumericDouble ();
			case TypeCode.Single:
				return new NumericSingle ();
			case TypeCode.UInt16:
				return new NumericUInt16 ();
			case TypeCode.UInt32:
				return new NumericUInt32 ();
			case TypeCode.UInt64:
				return new NumericUInt64 ();
			case TypeCode.Int16:
				return new NumericInt16 ();
			case TypeCode.Int32:
				return new NumericInt32 ();
			case TypeCode.Int64:
				return new NumericInt64 ();
			default:
				throw new NotImplementedException ("The numeric manager for "+ code.ToString() +" type code is not implemented");
			}
		}
	}
}
