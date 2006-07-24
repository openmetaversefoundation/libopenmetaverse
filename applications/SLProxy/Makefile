MCS = mcs
LIBSECONDLIFE_BUILD_DIR = ../../libsecondlife-cs/bin/Debug/

default: library examples

library: SLProxy.dll

examples: ChatConsole.exe Analyst.exe

clean::
	rm -f SLProxy.dll ChatConsole.exe Analyst.exe

SLProxy.dll: ${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll ${LIBSECONDLIFE_BUILD_DIR}XmlRpcCS.dll SLProxy.cs
	mcs SLProxy.cs -target:library -r:${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll -r:${LIBSECONDLIFE_BUILD_DIR}XmlRpcCS.dll

ChatConsole.exe: SLProxy.dll ${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll ChatConsole.cs
	mcs ChatConsole.cs -r:SLProxy.dll -r:${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll
Analyst.exe: SLProxy.dll ${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll Analyst.cs
	mcs Analyst.cs -r:SLProxy.dll -r:${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll

${LIBSECONDLIFE_BUILD_DIR}libsecondlife.dll:
	@make libsecondlife
${LIBSECONDLIFE_BUILD_DIR}XmlRpcCS.dll:
	@make libsecondlife

libsecondlife:
	@cd ../../libsecondlife-cs;prj2make libsecondlife.sln;make
