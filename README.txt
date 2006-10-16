NAnt
----

The */*.build files in this tree are nAnt build files; to use them,
download nAnt from nant.sourceforge.net and then run

nant -projecthelp

in any directory with a .build file to see available targets.

For example, to build a nightly build: 

nant package

Microsoft Visual Studio
-----------------------

Visual Studio 2005 project files are included to ease compiling on Windows 
platforms. Some of the projects use a custom targets file that allows you
to compile against .NET 1.0/1.1, mono, or the Compact Framework. You will 
need to authorize this custom file when you first open the solution file.
