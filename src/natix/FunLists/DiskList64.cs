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

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace natix
{
    public class DiskList64<T> : ListGenerator64<T>, IDisposable where T : struct
    {
        const int header_size = 2 * sizeof(long); //  two integers
		byte[] VACCUM;
		long __MaxCapacity = 0;
		long __Count = 0;

        protected long _Count {
            get {
				return this.__Count;
            }
            set {
                this.view.Write(0, value);
				this.__Count = value;
            }
        }
        
        protected long MaxCapacity {
            get {
				return this.__MaxCapacity;
            }
            set {
                this.view.Write(sizeof(long), value);
				this.__MaxCapacity = value;
            }
        }

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

		protected void OpenMappedFile()
		{
			FileInfo f = new FileInfo (this.filename);
			this.data = MemoryMappedFile.CreateOrOpen (this.filename, f.Length);
			this.view = this.data.CreateViewAccessor ();
		}

		protected void Grow(BinaryWriter Output)
		{
			Output.Seek (0, SeekOrigin.End);
			Output.Write (this.VACCUM);
		}

        public DiskList64 (string name, int block_size)
        {
            this.filename = name;
            this.block_size = block_size;
            this.sizeOfT = (short)Marshal.SizeOf(typeof(T));
			this.VACCUM = new byte[this.block_size * this.sizeOfT];
			if (File.Exists (this.filename)) {
				using (var Input = new BinaryReader(File.OpenRead(name))) {
					this.__Count = Input.ReadInt64();
					this.__MaxCapacity = Input.ReadInt64();
				}
			} else {
				using (var Output = new BinaryWriter(File.Create(name))) {
					this.__MaxCapacity = this.block_size;
					Output.Write ((long) 0L);
					Output.Write ((long) this.__MaxCapacity);
					this.Grow (Output);
				}
			}
			this.OpenMappedFile();
		}

        ~DiskList64()
        {
            this.Dispose();
        }

        public override T GetItem (long index)
        {
            T result = default(T);
            this.view.Read<T>(header_size + index*this.sizeOfT, out result);
            return result;
        }

        public override void SetItem (long index, T u)
        {
            this.view.Write<T>(header_size + index*this.sizeOfT, ref u);
        }

		public T[] ReadArray (long index, int len)
		{
			var output = new T[len];
			this.view.ReadArray<T>(header_size + index*this.sizeOfT, output, 0, len);
			return output;
		}

		public T[] ReadArray (long index, T[] output)
		{
			this.view.ReadArray<T>(header_size + index*this.sizeOfT, output, 0, output.Length);
			return output;
		}

		public void WriteArray (long index, T[] output)
		{
			this.view.WriteArray<T>(header_size + index*this.sizeOfT, output, 0, output.Length);
		}

        public override void Add (T item)
        {
            if (this.Count == this.MaxCapacity) {
				this.MaxCapacity += this.block_size;
				this.Dispose ();
				using (var Output = new BinaryWriter(File.OpenWrite(this.filename))) {
					this.Grow (Output);
				}
				this.OpenMappedFile();
            }
            this [this.Count] = item;
            ++this._Count;
        }

        public override long Count {
            get {
                return this.__Count;
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