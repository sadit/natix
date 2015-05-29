# Eric S. Tellez
# donsadit@gmail.com
#
# Tools to read datasets from 
# http://corpus-texmex.irisa.fr/
# The one billion sift dataset

import os
import sys
import struct


from datetime import datetime

def main(filename):
    if filename.endswith(".fvecs"):
        vec_fmt = "f"
        typelen = 4
    elif filename.endswith(".ivecs"):
        vec_fmt = "i"
        typelen = 4
    elif filename.endswith(".bvecs"):
        vec_fmt = "B"
        typelen = 1
    else:
        raise Exception("Unknown type of file %s" % filename)

    size = os.path.getsize(filename)

    with file(filename) as f:
        dim = struct.unpack("i", f.read(4))[0]
        veclen   = 4 + dim * typelen
        size = size / veclen
        fmt = "i" + (vec_fmt * dim)
        f.seek(0, 0)
        print >>sys.stderr, "filename %s, vectors: %d, dimension %d" % (filename, size, dim)

        for i in range(size):           
            vec = struct.unpack(fmt, f.read(veclen))
            assert vec[0] == dim
            vecline = " ".join(map(str, vec[1:]))
            print (vecline)
            if i % 100000 == 0:
                print >>sys.stderr, "advance %d/%d -- %s" % (i, size, datetime.now())


if __name__ == "__main__":
    main(sys.argv[1])

