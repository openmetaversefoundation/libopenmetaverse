MacProxy is a tool that packages an SLProxy application as a
standalone Mac OS X application.  When double-clicked, the application
will start the proxy in the background and open Second Life,
configured to connect through the proxy.  The proxy runs entirely in
the background, so its output will not be visible to the user.  You do
not need to have a Mac to run MacProxy; any typical Unix-like
environment with Perl installed should suffice.

To use MacProxy, navigate to the MacProxy directory and type

	./build.pl "Application Name" path/to/executable.exe \
	path/to/libraries.dll ...

For example, to create a standalone ChatConsole for the Mac:

	./build.pl "Chat Console" ../../bin/ChatConsole.exe \
	../../bin/SLProxy.dll ../../bin/libsecondlife.dll

(Of course, this is entirely pointless, since ChatConsole provides no
functionality unless run from a terminal.)

CAVEATS

The generated application will not work unless the user has the Mono
framework installed.  This should be distributed separately, since
it's huge.  The installer is available from
http://www.mono-project.com/Downloads.

MacProxy assumes that your proxy application accepts the standard
suite of --proxy arguments via the command line (see SLProxy's
documentation) and that it doesn't output anything of its own to
stdout before the proxy is active.
