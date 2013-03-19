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
    public struct ItemPair : ILoadSave
    {
        public int objID;
        public double dist;
        
        public ItemPair (int objID, double dist)
        {
            this.objID = objID;
            this.dist = dist;
        }
        
        public void Load(BinaryReader Input)
        {
            this.objID = Input.ReadInt32 ();
            this.dist = Input.ReadDouble();
        }
        
        public void Save (BinaryWriter Output)
        {
            Output.Write (this.objID);
            Output.Write (this.dist);
        }
        
        public override string ToString ()
        {
            return string.Format ("[Item (objID={0},dist={1})]", this.objID, this.dist);
        }
    }

}

