MCS = mcs
LIBSECONDLIFE_BUILD_DIR = ../../libsecondlife-cs/bin/Debug/

default: library examples

library: SLProxy.dll
examples: Analyst.exe ChatConsole.exe GodMode.exe

clean:
	rm -f SLProxy.dll Analyst.exe ChatConsole.exe GodMode.exe libsecondlife.dll XmlRpcCS.dll

SLProxy.dll: libsecondlife.dll XmlRpcCS.dll SLProxy.cs
	${MCS} SLProxy.cs -target:library -r:libsecondlife.dll -r:XmlRpcCS.dll

ChatConsole.exe: SLProxy.dll libsecondlife.dll XmlRpcCS.dll ChatConsole.cs
	${MCS} ChatConsole.cs -r:SLProxy.dll -r:libsecondlife.dll -r:XmlRpcCS.dll
Analyst.exe: SLProxy.dll libsecondlife.dll XmlRpcCS.dll Analyst.cs
	${MCS} Analyst.cs -r:SLProxy.dll -r:libsecondlife.dll -r:XmlRpcCS.dll
GodMode.exe: SLProxy.dll libsecondlife.dll XmlRpcCS.dll GodMode.cs
	${MCS} GodMode.cs -r:SLProxy.dll -r:libsecondlife.dll -r:XmlRpcCS.dll

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
