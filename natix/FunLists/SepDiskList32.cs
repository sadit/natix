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
// Chris Parnin -> Initial code
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
    public class SepDiskList32<T> : ListGenerator<T>, IDisposable, ILoadSave where T : struct
    {
        int count = 0; 
        int max_capacity = 0;
        int block_size; 
        short sizeOfT;
        string filename; // None, pointer to this filename
        MemoryMappedFile data = null;
        MemoryMappedViewAccessor view = null;

        public string FileName {
            get {
                return this.filename;
            }
        }

        // header and data should be placed in different files, because of technical convenience.
        public void Load (BinaryReader Input)
        {
            this.count = Input.ReadInt32 ();
            this.max_capacity = Input.ReadInt32 ();
            this.block_size = Input.ReadInt16 ();
            this.filename = Input.ReadString();
            this.sizeOfT = (short)Marshal.SizeOf(typeof(T));
            this.OpenAndGrow(false);
        }

        public void Save (BinaryWriter Output)
        {
            Output.Write (this.count);
            Output.Write (this.max_capacity);
            Output.Write (this.block_size);
            Output.Write (this.filename);
        }

        public void Dispose()
        {
            if (this.view != null) {
				this.view.Dispose ();
				this.view = null;
			}
            if (this.data != null) {
				this.data.Dispose ();
				this.data = null;
			}
        }

        public void DeleteFile ()
        {
            this.Dispose();
            File.Delete(this.FileName);
        }

        protected void OpenAndGrow (bool grow)
        { 
            this.Dispose ();
            var V = new byte[ this.block_size * this.sizeOfT];
            long num_bytes = 0;
            if (this.count == this.max_capacity) {
                if (File.Exists (this.filename)) {
                    if (grow) {
                        using (var f = new BinaryWriter(File.OpenWrite(this.filename))) {
                            f.Seek (0, SeekOrigin.End);
                            f.Write (V);
                            num_bytes = f.BaseStream.Length;
                        }
                    }
                } else {
                    using (var f = new BinaryWriter(File.Create(this.filename))) {
                        f.Write (V);
                        num_bytes = f.BaseStream.Length;
                    }
                }
                this.max_capacity += this.block_size;
            } else {
                num_bytes = this.max_capacity * this.sizeOfT;
            }
            this.data = MemoryMappedFile.CreateOrOpen (this.filename, num_bytes);
            this.view = this.data.CreateViewAccessor ();
        }

        public SepDiskList32 (string name, int block_size)
        {
            this.filename = name;
            this.block_size = block_size;
            this.sizeOfT = (short)Marshal.SizeOf(typeof(T));
            this.OpenAndGrow(false);
        }

        ~SepDiskList32()
        {
            this.Dispose();
        }

        public override T GetItem (int index)
        {
            T result = default(T);
            this.view.Read<T>(index*this.sizeOfT, out result);
            return result;
        }

        public override void SetItem (int index, T u)
        {
            this.view.Write<T>(index*this.sizeOfT, ref u);
        }

        public override void Add (T item)
        {
            if (this.count == this.max_capacity) {
                this.OpenAndGrow(true);
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