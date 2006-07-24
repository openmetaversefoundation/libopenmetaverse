default: library examples

library: SLProxy.dll

examples: ChatConsole.exe Analyst.exe

clean::
	rm -f SLProxy.dll ChatConsole.exe Analyst.exe

SLProxy.dll: libsecondlife.dll XmlRpcCS.dll SLProxy.cs
	mcs SLProxy.cs -target:library -r:libsecondlife.dll -r:XmlRpcCS.dll

ChatConsole.exe: SLProxy.dll libsecondlife.dll ChatConsole.cs
	mcs ChatConsole.cs -r:SLProxy.dll -r:libsecondlife.dll
Analyst.exe: SLProxy.dll libsecondlife.dll Analyst.cs
	mcs Analyst.cs -r:SLProxy.dll -r:libsecondlife.dll

libsecondlife.dll:
	@echo Please place a copy of libsecondlife.dll in this folder.; false
XmlRpcCS.dll:
	@echo Please place a copy of XmlRpcCS.dll in this folder.; false
