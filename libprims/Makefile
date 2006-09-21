SOURCES = prims01.cs PrimsLoader.cs

RESOURCES = prims01.xsd

CSC = mcs
XSD = xsd

all: libprims.dll

libprims.dll: $(SOURCES) $(RESOURCES)
	$(CSC) -resource:prims01.xsd \
		$(SOURCES) \
		-target:library \
		-out:libprims.dll

prims01.cs: prims01.xsd
	$(XSD) prims01.xsd /c

clean:
	rm *.dll prims01.cs
