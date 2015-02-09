//
//  Copyright 2013  Eric Sadit Tellez Avila
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
using System.IO;

namespace natix.SimilaritySearch
{
    public struct ItemPair : ILoadSave, IComparable<ItemPair>
    {
		public int ObjID;
		public double Dist;

		public ItemPair(int objID, double dist)
		{
			this.ObjID = objID;
			this.Dist = dist;
		}
        
        public void Load(BinaryReader Input)
        {
            this.ObjID = Input.ReadInt32 ();
            this.Dist = Input.ReadDouble();
        }
        
        public void Save (BinaryWriter Output)
        {
            Output.Write (this.ObjID);
            Output.Write (this.Dist);
        }

		public int CompareTo(ItemPair other)
		{
			var cmp = this.Dist - other.Dist;
			if (cmp == 0) {
				return this.ObjID - other.ObjID;
			}
			return Math.Sign(cmp);
		}
	}
}

