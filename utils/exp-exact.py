import os
import sys
import subprocess
import types
from multiprocessing import Pool


def apply(pool, args, kwargs):
    #pool.apply_async(call, args, kwargs)
    call(*args, **kwargs)


def call(*args, **_kwargs):
    kwargs=dict(stype="VEC", qtype=-1, parameterless=None)
    kwargs.update(_kwargs)

    cmdargs = ["mono-sgen", "--debug", "ExactIndexes.exe"]
    for arg in args:
        cmdargs.append(arg)

    for k, v in kwargs.items():
        cmdargs.append("--" + k)
        if v is None:
            pass
        elif isinstance(v, types.TupleType) or isinstance(v, types.ListType):
            cmdargs.append(",".join(map(str, v)))
        else:
            cmdargs.append(str(v))

    subprocess.call(cmdargs)


BASE_ARGS = dict(
    parameterless=None,
    laesa=[4,8,16,32,64],
    spa=[4,8,16,32,64],
    sss=[0.4],
    bnc=[4,8,16,32,64],
    kvp=[4,8,16,32],
    milc=[4,8,16,32,64],
    ept=[4,8,16,32,64],
    lc=[1024,512,256,128,64]
    )


def main_real(pool):
    ### real datasets
    args = dict(BASE_ARGS.items())
    args["lc"] = [1024,512,256,128,64]

    D="dbs/strings/dictionaries/English.dic"
    Q="queries/dic-english.queries"
    apply(pool, (), dict(database=D, queries=Q, stype="STR-ED", **args))
    
    D="dbs/strings/wiktionary/english.tsv"
    Q="queries/wiktionary-english.queries"
    apply(pool, (), dict(database=D, queries=Q, stype="WIKTIONARY", **args))

    D="dbs/cophir/cophir1M"
    Q="queries/cophir-208.queries"
    apply(pool, (), dict(database=D, queries=Q, **args))

    D="dbs/sift/ascii/sift_base"
    Q="dbs/sift/ascii/sift_query-256.vecs"
    apply(pool, (), dict(database=D, queries=Q, **args))
    sys.exit(0)

    args["lc"].append(32)
    D="dbs/vectors/nasa/nasa-20-40150"
    Q="queries/nasa.queries"
    apply(pool, (), dict(database=D, queries=Q, **args))
    args["lc"].append(16)

    D="dbs/vectors/colors/colors-112-112682"
    Q="queries/colors.queries"
    apply(pool, (), dict(database=D, queries=Q, **args))


def main_n(pool):
    args = dict(BASE_ARGS.items())

    for n in [100000, 300000, 1000000, 3000000]:
        for dim in [4, 12]:
            if dim == 4:
                args["lc"] = [1024,512,256]
            else:
                args["lc"] = [1024,512,256,128,64]
            D="dbs/vectors/random/db.random.%d.%d" % (dim, n)
            Q="queries/random-%d.queries" % dim
            apply(pool, (), dict(database=D, queries=Q, **args))


def main_dimension(pool):
    args = dict(BASE_ARGS.items())

    for dim in [4, 8, 12, 16, 20, 24]:
        n=1000000
        D="dbs/vectors/random/db.random.%d.%d" % (dim, n)
        Q="queries/random-%d.queries" % dim
        if dim >= 16:
            args["lc"] = [128, 64, 32, 16]
        apply(pool, (), dict(database=D, queries=Q, **args))


def main():
    p = Pool(processes=16)
    main_real(p)
    main_n(p)
    main_dimension(p)
    p.close()
    p.join()


if __name__ == "__main__":
    main()
