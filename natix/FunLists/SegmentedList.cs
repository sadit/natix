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
//
// Eric Sadit -> Adapted and extended

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace natix
{
    public class SegmentedList<T> : ListGenerator<T>, ILoadSave where T : struct
    {
        int count = 0;
        int max_capacity = 0;
        int block_size;
        List<T[]> blocks;

        // header and data should be placed in different files, because of technical convenience.
        public void Load (BinaryReader Input)
        {         
            this.count = Input.ReadInt32 ();
            this.max_capacity = Input.ReadInt32 ();
            this.block_size = Input.ReadInt16 ();
            int len = Input.ReadInt32 ();
            this.blocks = new List<T[]>(len);
            for (int i = 0; i < len; ++i) {
                this.blocks[i] = (T[])PrimitiveIO<T>.LoadVector(Input, this.block_size, null);
            }
        }

        public void Save (BinaryWriter Output)
        {
            Output.Write (this.count);
            Output.Write (this.max_capacity);
            Output.Write (this.block_size);
            Output.Write (this.blocks.Count);
            for (int i = 0; i < this.blocks.Count; ++i) {
                PrimitiveIO<T>.SaveVector(Output, this.blocks[i]);
            }
        }


        public SegmentedList (int block_size)
        {
            this.block_size = block_size;
            this.blocks = new List<T[]>();
        }

        public override T GetItem (int index)
        {
            var blockID = index / this.block_size;
            var rem = index % this.block_size;
            return this.blocks[blockID][rem];
        }

        public override void SetItem (int index, T u)
        {
            var blockID = index / this.block_size;
            var rem = index % this.block_size;
//            Console.WriteLine ("== index: {0}, blockID: {1}, rem: {2}, block_size: {3}, blocks.Count: {4}",
//                               index, blockID, rem, this.block_size, this.blocks.Count);
//            Console.WriteLine ("== count: {0}, max_capacity: {1}", this.count, this.max_capacity);
            this.blocks[blockID][rem] = u;
        }

        public override void Add (T item)
        {
            if (this.count == this.max_capacity) {
                this.blocks.Add(new T[this.block_size]);
                this.max_capacity += this.block_size;
            }
            this [this.count] = item;
            ++this.count;
        }

        public override int Count
        {
            get {
                return this.count;
            }
        }
        
        public override bool IsReadOnly
        {
            get {
                return false;
            }
        }

   }
}