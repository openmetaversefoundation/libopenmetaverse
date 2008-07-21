libsecondlife 

Finding Help
------------

If you need any help we have a couple of resources, the primary one being 
the #libsl IRC channel on EFNet. There is also the libsl-dev mailing list 
at http://openmetaverse.org/cgi-bin/mailman/listinfo/libsl-dev and lastly 
you can use the e-mail contact@libsecondlife.org for any general inquiries 
(although we prefer developer-related questions to go to IRC or the mailing 
list). You can find us in-world via the open invitation libsecondlife group 
or at our HQ and testing area in Hooper(SLURL: http://xrl.us/bi233).

Source Code:
   To checkout a copy of libsecondlife trunk
   svn co svn://openmetaverse.org/libsl/trunk libsl

For more details see: 
   http://www.libsecondlife.org/wiki/SVN
   http://www.libsecondlife.org/wiki/Getting_Started

Getting started on Windows
------------------------------------------------------------------
------------------------------------------------------------------


Software Needed (all Freeware)
------------------------------

MS .NET Framework 2.0 - Get directly from Windows Update.
Visual C# Express - http://msdn.microsoft.com/vstudio/express/visualcsharp/

Optional-
nAnt (0.85) - http://nant.sourceforge.net/
nUnit Framework (2.2.8 or greater) - http://www.nunit.org/


Compiling
---------

If you are using Visual Studio or Visual C# Express you can simply open 
the libsecondlife.sln solution file and begin compiling. It will complain 
about a missing dependency if you didn't install the nUnit framework, you 
can either install it now or remove libsecondlife.Tests from the solution 
file.

For more details http://www.libsecondlife.org/wiki/Getting_Started

Getting started on Linux
-----------------------------------------------------------------
-----------------------------------------------------------------


Software Needed
---------------

mono 1.9 - http://www.mono-project.com/
nAnt (0.85) - http://nant.sourceforge.net/

Optional-
nUnit Framework (2.2.8 or greater) - http://www.nunit.org/


Compiling
---------

Simply go to the directory where you extracted libsecondlife, where the 
libsecondlife.build file is located and type "nant". To build a zip file 
package use "nant package", and to generate documentation use "nant doc".

For more details http://www.libsecondlife.org/wiki/Getting_Started


Happy fiddling,
--libsecondlife Team 
