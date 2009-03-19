Introduction
------------

Simian is a lightweight simulator built on the libOpenMetaverse framework. Its
primary uses are protocol translation and new protocol testing, rapid 
prototyping of new designs, a lightweight benchmarking suite, and unit testing 
of client applications.

The design of Simian is centered around the one client-facing protocol it 
supports. Where OpenSim defines a generic framework to support many backend 
simulation systems and many frontend clients, Simian works with the concepts 
that the Second Life viewer understands. Avatars, prims, and terrain become the 
basic entities of the metaverse. Concepts of space are converted into 256x256 
meter squares of terrain. Chat and instant messaging are translated into the 
mechanisms used in Second Life. This allows new protocols and behaviors to be 
added without having to modify the Second Life viewer directly, or work with the
complex (many-to-many) problem of translating concepts between all virtual 
worlds.

Extensions
------------

Extensions can be written in one of three ways.

1) Add a class that inherits from IExtension directly in the Simian project.
   Typically this is done by adding a new .cs file in the extensions folder.

2) Create a new assembly containing one or more extensions. The assembly must
   follow the naming convention of Simian.*.dll.

3) Put a source code file alongside the running Simian.exe binary that will be
   compiled at runtime. The code must follow the naming convention Simian.*.cs.
   Look at Simian.ViewerEffectPrinter.cs.example for an example. Remove the
   .example extension and put the file alongside the Simian.exe binary to see it
   working.

*NOTE*: Extensions will only be loaded if they are listed in the Simian.ini file
in the [Extensions] section. You can comment out extensions, but if an extension
implements an interface and there is no other loaded extension that implements
that interface the Simian server will stop with an error message.

All extensions must inherit from IExtension and have a Start method that
takes a Simian object as the only parameter, along with an empty Stop method. 
See the http://code.google.com/p/extensionloader/ project for more details on 
writing extensions.
