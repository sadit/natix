The natix library
================


Natix is an opensource (Apache 2.0) library with state of the art
methods in the following areas:


* Similarity and metric search
 - Exact indexes
 - Approximate indexes
* Compressed (and compact) data-structures
 - Indexed bitmaps
 - Indexed sequences
 - Indexed permutations
 - Sorted and unsorted lists of integers
 - Integer encoders
* Fulltext indexes
 - Inverted lists
 - Intersection algorithms
 - T-Threshold algorithms
 - Union-Intersection algorithms
* Searching and sorting algorithms and structures
 - Numeric sorting
 - Comparison based algorithms - sorting and searching
 - Adaptive searching/sorting with SkipLists


Natix is designed for experimentation and testing of
algorithms. However, it should be robust enough to be of use in application
development.



Requirements:
-------------

Natix has few dependencies

* A running Mono/.NET environment. We use the [mono framework](http://www.mono-project.com) in Linux.
* [JSon.NET](http://www.newtonsoft.com) from Newtonsoft
* (Optional) An IDE to modify the C# project Monodevelop / XamarinStudio / #Develop / VisualStudio. We use [monodevelop](http://www.monodevelop.com) in Linux.

New: We added a Makefile that compile libraries and programs. Moreover, it fetches dependencies using nuget.exe
     Using this Makefile is also of use for environments without an IDE

Building:
-----------
At the main directory execute

`make all`

It will fetch `nuget.exe` from NuGet and then get Json from Newtonsoft (using nuget.exe). After that, it will build the natix libraries and programs:

- `natix.dll` and `natix.SimilaritySearch.dll`
- `ExactIndexes.exe` and `ApproxIndexes.exe`

You can copy all `*.dll` and `*.exe` to your working directory or work directly in the current path


How to use it:
--------------

You can write you own programs and scripts
(e.g., using [IronPython](http://www.ironpython.net) or any other
programming language for the CLR).

For similarity search, `natix.SimilaritySearch`, we provide two
programs `ApproxIndexes.exe` and `ExactIndexes`, for aproximate and
exact indexes, with a bunch of indexes to be tested with a number of
parameters.
Both can be used for benchmarking purposes or examples of how to create these
indexes. Of course, you need to check the available options.
A complete example is provided
`exp-approx.py` and `exp-exact.py`, two Python scripts,
those systematically perform a benchmarking.

Also, for summarize the output of `ApproxIndexes.exe` and `ExactIndexes.exe`
we provide the `summary.py` script.

All these tools are not intended to be exhaustive for the available
indexes, and should be adjusted as need.



Queries and Databases:
---------------------
We strongly recommend to use standard databases for testing
purposes. We also provide some [fixed queries](https://github.com/sadit/natix/tree/master/queries).

- [SISAP project](http://www.sisap.org)
  + Histogram of colors
     * Hard queries - nearest neighbors are close to mean
     * Easy queries - following dataset distribution
     * Dictionaries
       - English
       - Spanish
  + Nasa images
     * Queries
- [Wiktionary](http://dumps.wikimedia.org/enwiktionary/)
- CoPhIR project
- [Synthetic datasets](https://github.com/sadit/natix/blob/master/utils/create-random-vectors.py).
  Randomly generated datasets in the unitary cube, from a wide range
  of dimensions and sizes. We provide a python script for this
  purpose. New synthetic queries can be created with this tool.
- Texmex datasets. [The one billion dataset](http://corpus-texmex.irisa.fr/). We provide a
  [python script to decode binary files](https://github.com/sadit/natix/blob/master/utils/dump-matlab-binary.py).