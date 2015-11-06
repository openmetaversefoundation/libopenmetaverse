libOpenMetaverse Library Quick Start


Finding Help
------------

If you need any help we have a couple of resources, the primary one being 
the #libomv-dev IRC channel on Freenode. There is also the libomv-dev mailing list 
at http://groups.google.com/group/libomv-dev. You can find us 
in-world via the open invitation libsecondlife group.

Source Code:
   To checkout a copy of libopenmv trunk
   git clone https://github.com/openmetaversefoundation/libopenmetaverse.git libopenmetaverse

For more details see: 
   http://lib.openmetaverse.co/wiki/Download#Development
   http://lib.openmetaverse.co/wiki/Getting_Started

Getting started on Windows
====================================================================================


Prerequisites (all Freely Available)
--------------------------------------

Microsoft .NET Framework 3.5 - Get directly from Windows Update.
Visual C# Express - http://msdn.microsoft.com/vstudio/express/visualcsharp/

Optional-
nAnt (0.86) - http://nant.sourceforge.net/
nUnit Framework (2.2.8 or greater) - http://www.nunit.org/


Compiling
---------
For Visual Studio 2008/Visual C# Express 2008
1. Open Explorer and browse to the directory you extracted the source distribution to
2. Double click the runprebuild2008.bat file, this will create the necessary solution and project files
3. open the solution OpenMetaverse.sln from within Visual Studio
4. From the Build Menu choose Build Solution (or press the F6 Key)

The library, example applications and tools will be in the bin directory

For Visual Studio 2010:
1. Open Explorer and browse to the directory you extracted the source distribution to
2. Double click the runprebuild2010.bat file, this will create the necessary solution and project files
3. open the solution OpenMetaverse.sln from within Visual Studio
4. From the Build Menu choose Build Solution (or press the F6 Key)

The library, example applications and tools will be in the bin directory

For more details http://lib.openmetaverse.co/wiki/Getting_Started


Getting started on Linux
====================================================================================

Prerequisites Needed
--------------------

mono 2.4 - http://www.mono-project.com/

Optional-
nUnit Framework (2.2.8 or greater) - http://www.nunit.org/
nAnt (0.86) - http://nant.sourceforge.net/

Compiling
---------

Using nant:
1. Change to the directory you extracted the source distribution to
2. run the prebuild file: % sh runprebuild.sh nant - This will generate the required nant build files and run
   nant with the correct buildfile parameter to build the library, examples and tools

Using mono xbuild:
1. Change to the directory you extracted the source distribution to
2. run the prebuild file: % sh runprebuild.sh - This will generate the solution files for xbuild
3. Compile the solution with the command: % xbuild OpenMetaverse.sln

The library, example applications and tools will be in the bin directory

For more details http://lib.openmetaverse.co/wiki/Getting_Started


Happy fiddling,
-- OpenMetaverse Ninjas 

