MCS = mcs
LIBSECONDLIFE_BUILD_DIR = ../../libsecondlife-cs/bin/Debug/

default: library examples

library: SLProxy.dll
examples: ChatConsole.exe Analyst.exe

clean:
	rm -f SLProxy.dll ChatConsole.exe Analyst.exe libsecondlife.dll XmlRpcCS.dll

SLProxy.dll: libsecondlife.dll XmlRpcCS.dll SLProxy.cs
	${MCS} SLProxy.cs -target:library -r:libsecondlife.dll -r:XmlRpcCS.dll

ChatConsole.exe: SLProxy.dll libsecondlife.dll ChatConsole.cs
	${MCS} ChatConsole.cs -r:SLProxy.dll -r:libsecondlife.dll
Analyst.exe: SLProxy.dll libsecondlife.dll Analyst.cs
	${MCS} Analyst.cs -r:SLProxy.dll -r:libsecondlife.dll

libsecondlife.dll: ${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll
	cp ${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll .
XmlRpcCS.dll: ${LIBSECONDLIFE_BUILD_DIR}XmlRpcCS.dll
	cp ${LIBSECONDLIFE_BUILD_DIR}XmlRpcCS.dll .

${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll:
	@make libsecondlife
${LIBSECONDLIFE_BUILD_DIR}XmlRpcCS.dll:
	@make libsecondlife

libsecondlife:
	cd ../../libsecondlife-cs;prj2make libsecondlife.sln;make
