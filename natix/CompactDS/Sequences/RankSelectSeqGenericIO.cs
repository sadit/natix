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
//   Original filename: natix/CompactDS/Sequences/RankSelectSeqGenericIO.cs
// 
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace natix.CompactDS
{
	/// <summary>
	/// Save/Load an index of sequences
	/// </summary>
	public class RankSelectSeqGenericIO
	{
		static List<Type> Catalog = new List<Type>() {
			typeof(WaveletTree),
			typeof(InvIndexSketches),
			typeof(InvIndexSeq),
			typeof(GolynskiMunroRaoSeq),
			typeof(SeqSinglePerm),
			typeof(SeqSinglePerm),//duplicated because we remove	typeof(GolynskiListRL2Seq),
			typeof(SeqXLB),
			typeof(InvIndexXLBSeq),
			typeof(SeqPlain),
			typeof(WTM),
            typeof(SeqPlainCopyOnUnravel)
		};
		
		/// <summary>
		/// Saves "seq" to the "Output"
		/// </summary>

		public static void Save (BinaryWriter Output, IRankSelectSeq seq)
		{
			var type = seq.GetType ();
			byte idType = 255;
			for (byte i = 0; i < Catalog.Count; i++) {
				if (type == Catalog [i]) {
					idType = i;
					break;
				}
			}
			if (idType == 255) {
				var s = String.Format ("Type {0} is not a recognized indexed sequence, please add it to " +
					"RankSelectSeqGenericIO.Catalog", type);
				throw new ArgumentException (s);
			}
			Output.Write (idType);
			seq.Save (Output);
		}
		
		/// <summary>
		/// Load a seq index from "Input"
		/// </summary>

		public static IRankSelectSeq Load (BinaryReader Input)
		{
			byte idType = Input.ReadByte ();
			var type = Catalog[idType];
			if (type == null) {
				var s = String.Format ("RankSelectSeqGenericIO.Catalog returned null " +
					"using idType: {0}, is it deprecated?", idType);
				throw new ArgumentNullException (s);
			}
			var seq = (IRankSelectSeq)Activator.CreateInstance (type);
			seq.Load (Input);
			return seq;
		}

        public static int[] ToIntArray (IRankSelectSeq seq, bool use_access_based_copy)
        {
            var S = new int[seq.Count];
            if (use_access_based_copy) {
                for (int i = 0; i < seq.Count; ++i) {
                    S[i] = seq.Access(i);
                }
            } else {
                for (int sym = 0; sym < seq.Sigma; ++sym) {
                    var rs = seq.Unravel (sym);
                    var count1 = rs.Count1;
                    for (int i = 1; i <= count1; ++i) {
                        var p = rs.Select1 (i);
                        S [p] = sym;
                    }
                }
            }
            return S;
        }
	}
}
