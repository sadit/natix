#!/usr/bin/python
import sys
import os
import io
import re
import math
import json

def load_records(digname):
    with io.open(digname) as f:
        s = json.loads(f.read())
        for m in s:
            for k, v in m["Parametrized"].items():
                m["@%s" % k] = v
            del m["Parametrized"]
            del m["QueryList"]
        return s

def select_keys(records, cols):
    L = []
    for rec in records:
        L.append([ rec[c] for c in cols ])
    return L


def square_speedup(rec, seq):
    """
    Emulates the performance on a costly distance function
    (square like cost, i.e., real_cost \times dimension)
    """
    dim = 64
    dist_time = seq['SearchTime'] / seq['SearchCostTotal']
    square_time = rec['SearchTime'] - dist_time * rec['SearchCostTotal'] + dist_time * dim * rec['SearchCostTotal']
    return seq['SearchTime'] * dim / square_time


def postprocessing(records, tabname):
    seq = None
    for rec in records:
        if 'Seq' in rec['ResultName']:
            seq = rec
            break
    assert seq is not None, "ERROR Sequential search was not detected in %s" % tabname

    for rec in records:
        rec['@SpeedupSquare'] = square_speedup(rec, seq)

    return records


def _str(d):
    if type(d) is float:
        return "%0.6f" % d
    else:
        return str(d)


if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--cols",
                        type=str,
                        help=("A comma separated list of keys to be shown as columns "
                              "(the given order is preserved). It defaults to all "
                              "available columns in lexicographical order"))
    parser.add_argument("--header",
                        action='store_true',
                        help="Display a brief information of the output")
    parser.add_argument("input_list",
                        type=str,
                        # action="append",
                        nargs="+",
                        help="The input filename list")
    parser.add_argument("--sort",
                        dest="sort_by",
                        type=str,
                        help=("A list of columns to induce the data order"
                              "(comma separated, natural ordering for the column's type). "
                              "Per input."))
    parser.add_argument("--reverse",
                        dest="reverse",
                        action='store_true',
                        help=("Reverse the order of the records, per input"))
    parser.add_argument("--first",
                        action='store_true',
                        help="Prints the first record of each input")
    parser.add_argument("--filter",
                        type=str,
                        dest="filter",
                        help=("Accepts records evaluated as True the given python expression. "
                              "Notes: "
                              "i) all key-values are exposed as local variables, "
                              "ii) the current record is stored in the rec variable, "
                              "ii) the re module is available"))
    parser.add_argument("--reduce",
                         type=str,
                         dest="reduce",
                         help=("A python expression to reduce records into a single one. "
                               "Notes: "
                               "i) accumulated record is stored in *acc* variable. "
                               "ii) current record is stored in *curr* variable. "
                               "iii) the expression must evaluates to a dictionary, it updates acc. "
                               "iv) all records can be accessed using the *records* variable."))
    args = parser.parse_args()

    for input in args.input_list:
        records = postprocessing(load_records(input), input)
        if args.filter:
            def _filter(rec):
                _locals = dict(rec=rec)
                _locals.update(rec)
                return eval(args.filter, globals(), _locals)
            records = filter(_filter, records)

        if len(records) == 0:
            continue

        if args.sort_by:
            def _keyfun(rec):
                return [ rec[c] for c in args.sort_by.split(",") ]
            records.sort(key=_keyfun)

        if args.reverse:
            records.reverse()

        if args.reduce:
            def _reduce(acc, curr):
                _locals = dict(acc=acc, curr=curr, records=records)
                res = eval(args.reduce, globals(), _locals)
                acc.update(res)
                return acc

            records = [reduce(_reduce, records)]

        if args.first:
            records = [records[0]]

        available_cols = sorted(records[0].keys())

        if args.cols:
            cols = args.cols.split(",")
        else:
            cols = available_cols

        if args.header:
            print "# " + "\t ".join(cols)
            print "# available cols: "  + ",".join(available_cols)
            if args.filter:
                print "# filters: " + args.filter

        for rec in select_keys(records, cols):
            print "\t ".join(map(_str, rec))

