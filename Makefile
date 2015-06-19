SRCNATIX=$(shell find src/natix/ -name "*.cs")
SRCSIMSEARCH=$(shell find src/natix.SimilaritySearch/ -name "*.cs")
SRCAPPROX=$(shell find src/ApproxIndexes/ -name "*.cs")
SRCEXACT=$(shell find src/ExactIndexes/ -name "*.cs")
# debugging
# MCS=mcs -debug
# testing
# MCS=mcs -optimize+
## both debugging and testing (could produce debug info to be buggy due to optimizations)
MCS=mcs -optimize+ -debug

all: ExactIndexes.exe ApproxIndexes.exe Newtonsoft.Json.dll

clean:
	rm -f natix.dll natix.SimilaritySearch.dll ExactIndexes.exe AproxIndexes.exe

nuget.exe:
	wget https://nuget.org/nuget.exe

Newtonsoft.Json.dll: nuget.exe
	mono nuget.exe install Newtonsoft.Json -Version 6.0.8
	cp Newtonsoft.Json.6.0.8/lib/net40/Newtonsoft.Json.* .

natix.dll:
	$(MCS) $(SRCNATIX) -target:library -out:natix.dll

natix.SimilaritySearch.dll:  Newtonsoft.Json.dll
	$(MCS) $(SRCSIMSEARCH) -target:library -out:natix.SimilaritySearch.dll -r:natix.dll -r:Newtonsoft.Json.dll

ExactIndexes.exe: natix.dll natix.SimilaritySearch.dll
	$(MCS) $(SRCEXACT) -target:exe -out:ExactIndexes.exe -r:natix.dll -r:natix.SimilaritySearch.dll -r:Newtonsoft.Json.dll

ApproxIndexes.exe: natix.dll natix.SimilaritySearch.dll
	$(MCS) $(SRCAPPROX) -target:exe -out:ApproxIndexes.exe -r:natix.dll -r:natix.SimilaritySearch.dll -r:Newtonsoft.Json.dll
