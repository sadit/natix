__doc__ = """
Creates a database in a unitary hypercube using a random distribution.
"""
import random
import os
import sys
import re

def create_db(outname, dim, n, p):
    vecname = outname + ".vecs"
    f = file(vecname, "w")
    for i in range(n):
        v = [ random.random() for j in range(dim) ]
        f.write(" ".join(map(str, v)))
        if i + 1 < n:
            f.write("\n")
    f.close()
    f = file(outname, "w")
    f.write("%(dim)s %(n)s %(p)s"%locals())
    f.close()


def main(args):
    try:
        outname = args[0]
        dim = int(args[1])
        n = int(args[2])
        p = int(args[3])
    except IndexError, e:
        print
        print "usage: python %s outname dim n p"%sys.argv[0]
        print
        raise e
    create_db(outname, dim, n, p)

if __name__ == "__main__":
    main(sys.argv[1:])

