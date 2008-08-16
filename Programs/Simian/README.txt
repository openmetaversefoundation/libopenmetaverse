Introduction
------------

Simian is a lightweight simulator built on the libOpenMetaverse framework. Its
primary uses are rapid prototyping of new designs, a lightweight benchmarking
suite, and unit testing of client applications.

Extensions
------------

Extensions can be written in one of three ways.

1) Add a class that inherits from ISimianExtension directly in the Simian
   project. Typically this is done by adding a new .cs file in the
   extensions folder.

2) Create a new assembly containing one or more extensions. The assembly must
   follow the naming convention of Simian.*.dll.

3) Put a source code file alongside the running Simian.exe binary that will be
   compiled at runtime. The code must follow the naming convention Simian.*.cs.
   Look at Simian.ViewerEffectPrinter.cs.example for an example. Remove the
   .example extension and drop the file alongside the Simian.exe binary to see
   it in action.

All extensions must inherit from ISimianExtension and have a constructor that
takes a Simian object as the only parameter.
