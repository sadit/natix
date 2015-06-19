NATIXSRCFILES=$(shell find natix/ -name "*.cs")
NATIXSIMSEARCHSRCFILES=$(shell find natix.SimilaritySearch/ -name "*.cs")

all: ExactIndexes.exe ApproxIndexes.exe Newtonsoft.Json.dll

clean:
	rm -f natix.dll natix.SimilaritySearch.dll ExactIndexes.exe AproxIndexes.exe

nuget.exe:
	wget https://nuget.org/nuget.exe

Newtonsoft.Json.dll: nuget.exe
	mono nuget.exe install Newtonsoft.Json -Version 6.0.8
	cp Newtonsoft.Json.6.0.8/lib/net40/Newtonsoft.Json.* .

natix.dll:
	mcs $(NATIXSRCFILES) -target:library -out:natix.dll

natix.SimilaritySearch.dll:  Newtonsoft.Json.dll
	mcs $(NATIXSIMSEARCHSRCFILES) -target:library -out:natix.SimilaritySearch.dll -r:natix.dll -r:Newtonsoft.Json.dll

ExactIndexes.exe: natix.dll natix.SimilaritySearch.dll
	mcs $(shell find ExactIndexes/ -name "*.cs") -target:exe -out:ExactIndexes.exe -r:natix.dll -r:natix.SimilaritySearch.dll -r:Newtonsoft.Json.dll

ApproxIndexes.exe: natix.dll natix.SimilaritySearch.dll
	mcs $(shell find ApproxIndexes/ -name "*.cs") -target:exe -out:ApproxIndexes.exe -r:natix.dll -r:natix.SimilaritySearch.dll -r:Newtonsoft.Json.dll
