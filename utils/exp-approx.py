import os
import sys
import subprocess
import types
from multiprocessing import Pool


def apply(pool, args, kwargs):
    if kwargs.pop('multiprocessing', True):
        pool.apply_async(call, args, kwargs)
    else:
        call(*args, **kwargs)


def call(*args, **_kwargs):
    kwargs = dict(stype="VEC", qtype=-30, parameterless=None)
    kwargs.update(_kwargs)

    cmdargs = ["mono-sgen", "--debug", "ApproxIndexes.exe"]
    for arg in args:
        cmdargs.append(arg)

    for k, v in kwargs.items():
        k = k.replace('_', '-')
        cmdargs.append("--" + k)
        if v is None:
            pass
        elif isinstance(v, types.TupleType) or isinstance(v, types.ListType):
            cmdargs.append(",".join(map(str, v)))
        else:
            cmdargs.append(str(v))

    subprocess.call(cmdargs)
    #   --napp-ksearch=VALUE   Search KnrSeq indexes with the given list of 
    #                            near references (comma sep.)
    #   --lsh-instances=VALUE  A list of instances to for LSH_FloatVectors 
    #                            (comma sep.)
    #   --lsh-width=VALUE      A list of widths for LSH_FloatVectors (comma se-

BASE_ARGS = dict(
    # neighborhoodhash_instances=[1, 4, 8, 16],
    # neighborhoodhash_recall=[0.9, 0.8],
    optsearch_beamsize=[8, 16, 32],
    optsearch_restarts=[8, 16, 32],
    # optsearch_neighbors=[2, 4, 8, 16, 32],
    optsearch_neighbors=[2, 4, 8, 16, 32],  # 2, 4
    knr_numrefs=[2048],
    knr_kbuild=[7, 12],
    knr_maxcand=[0.03, 0.10],
    # knr_kbuild=[0, 7, 12],
    # knr_maxcand=[0.003, 0.01, 0.03],
)

BASE_ARGS_DIM = dict(
    # neighborhoodhash_instances=[1, 4, 8, 16],
    # neighborhoodhash_recall=[0.9, 0.8],
    optsearch_beamsize=[8, 16, 32],
    optsearch_restarts=[8, 16, 32],
    optsearch_neighbors=[2, 4, 8, 16, 32, 64],
    knr_numrefs=[2048],
    knr_kbuild=[7, 12],
    knr_maxcand=[0.03, 0.10],
    # knr_numrefs=[1024, 2048, 4096],
    # knr_kbuild=[7, 12],
    # knr_kbuild=[0, 7, 12],
)

BASE_ARGS_N = dict(
    # neighborhoodhash_instances=[1, 4, 8, 16],
    # neighborhoodhash_recall=[0.9, 0.8],
    optsearch_beamsize=[8, 16, 32],
    optsearch_restarts=[8, 16, 32],
    optsearch_neighbors=[8],
    knr_numrefs=[2048],
    knr_kbuild=[7, 12],
    knr_maxcand=[0.03, 0.10],
    # knr_numrefs=[2048],
    # knr_kbuild=[7],
    # knr_kbuild=[0, 7, 12],
    # knr_maxcand=[0.01],
)


def main_real(pool, **kwargs):
    ### real datasets
    args = dict(BASE_ARGS.items())
    args.update(kwargs)

    D = "dbs/vectors/nasa/nasa-20-40150"
    Q = "queries/nasa.queries"
    apply(pool, (), dict(database=D, queries=Q, **args))

    D = "dbs/vectors/colors/colors-112-112682"
    Q = "queries/colors-hard.queries"
    apply(pool, (), dict(database=D, queries=Q, **args))

    return
    D = "dbs/strings/wiktionary/english.tsv"
    Q = "queries/wiktionary-english.queries"
    apply(pool, (), dict(database=D, queries=Q, stype="WIKTIONARY", **args))

    #D = "dbs/strings/dictionaries/English.dic"
    #Q = "queries/dic-english.queries"
    #apply(pool, (), dict(database=D, queries=Q, stype="STR-ED", **args))

    # args['optsearch_neighbors'] = [8, 16, 32]
    # D = "dbs/cophir/cophir1M"
    # Q = "queries/cophir-208.queries"
    # apply(pool, (), dict(database=D, queries=Q, **args))


def main_n(pool, **kwargs):
    args = dict(BASE_ARGS_N.items())
    args.update(kwargs)

    for n in [3e5, 1e6, 3e6]:
        n = int(n)
        # for dim in [12, 16, 32, 64]:
        for dim in [16, 32, 64]:
            # if dim != 32 and n != 3e6: continue
            D = "dbs/vectors/random/db.random.%d.%d" % (dim, n)
            Q = "queries/random-%d.queries" % dim
            #if dim > 12 and n in [1e7, 3e7]:
            #    continue
            apply(pool, (), dict(database=D, queries=Q, **args))


def main_dimension(pool, **kwargs):
    args = dict(BASE_ARGS_DIM.items())
    args.update(kwargs)

    # for dim in [4, 8, 12, 16, 20, 24]:
    # for dim in [4, 8, 16, 32, 64, 128, 256]:
    for dim in [16, 32, 64, 128, 256]:
        # n = 1000000
        n = 100000
        D = "dbs/vectors/random/db.random.%d.%d" % (dim, n)
        Q = "queries/random-%d.queries" % dim
        apply(pool, (), dict(database=D, queries=Q, **args))


def main():
    p = Pool(processes=3)
    # construction
    args = dict(
        # skip_search=None,
        multiprocessing=False,
        cores=1
    )
    main_real(p, **args)
    # main_n(p, **args)
    # dimension should be the last one when we are searching because n writes Tab
    # main_dimension(p, **args)

    p.close()
    p.join()


if __name__ == "__main__":
    main()
