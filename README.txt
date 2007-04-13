
libsecondlife
-------------

We're still working on getting some documentation prepared for our first 
"official" release, so bear with us for now. If you need any help we 
have a couple of resources, the primary one being the #libsl IRC channel 
on EFNet. There are also the web forums at 
http://www.libsecondlife.org/forums/ and the libsecondlife-dev mailing 
list at https://lists.berlios.de/mailman/listinfo/libsecondlife-dev and 
lastly you can use the e-mail contact@libsecondlife.org for any general 
inquiries (although we prefer developer-related questions to go to IRC, 
forums, or the mailing list). You can find us in-world at 
http://tinyurl.com/y472fj.

To checkout a copy of libsecondlife trunk see 
https://developer.berlios.de/svn/?group_id=7710 for details. 


Windows
------------------------------------------------------------------
------------------------------------------------------------------


Microsoft Visual Studio
-----------------------

Visual Studio 2005 project files are included to ease compiling on Windows 
platforms.  Some of the projects use a custom targets file that allows you 
to compile against .NET 1.0/1.1, mono, or the Compact Framework.  You will 
need to authorize this custom file when you first open the solution file.


Software Needed (all Freeware)
------------------------------

Basic Stuff (Windows):
   MS .NET Framework (1.1 and/or 2.0 depending on what you're working on; 
      preferably both).  Get directly from Windows Update.
   Visual C# Express - http://msdn.microsoft.com/vstudio/express/visualcsharp/
   nAnt (0.85) - http://nant.sourceforge.net/
   nUnit Framework (2.2.8 or greater) - http://www.nunit.org/


If you are using Visual Studio or Visual C# Express you can simply open 
the libsecondlife.sln solution file and begin compiling. It will complain 
about a missing dependency if you didn't install the nUnit framework, you 
can either install it now or remove libsecondlife.Tests from the solution 
file.


NAnt under Windows
-----------------------------------------------------------------

1) Get .Net Frameworks (1.1 and 2.0) plus any patches installed via 
Windows Update.  Multiple reboots may be needed.

2) Install Visual C# Express (defaults are fine)

3) Install nUnit Framework (defaults are fine)

4) Unpack the 0.85 binary of nAnt in a directory you can be happy with 
   (I chose C:\bin\nant-0.85\bin).  Add this to your XP PATH variable:

   * Start->My Computer->[Right Click]->Properties (new window)
   * Advanced Tab -> Environment Variables (new window)
   * System Variables is bottom subwindow, click on 'path' and then the 
     'Edit' button below that. (new window)
   * Add ';C:\bin\nant-0.85\bin' or whereever it is to the line and click 
     OK.
   * Click OK twice more to close out system config windows.

5) In a Command Prompt (yea you heard that right) cd to where you checked 
out the trunk (You're looking for the dir that has libsecondlife.build and 
Ovastus.CSharp.targets).  I'm going to call this %TRUNK% from here on.

   * Run 'nant' and sit back.  'cd bin' and enjoy!

6) [Optional]  Not all apps and examples will be in nant due to various 
compatibility and stability issues.  If you're still interested:

  * Under %TRUNK%\libsecondlife-cs, open the libsecondlife.sln file and 
    Build the Solution (you're likely going to get warnings and such here).
  * This may overwrite some of the files you created in step 5; If they 
    don't work re-run nant.


Linux
-----------------------------------------------------------------
-----------------------------------------------------------------


Software Needed
---------------

mono - http://www.mono-project.com/
nAnt (0.85) - http://nant.sourceforge.net/
nUnit Framework (2.2.8 or greater) - http://www.nunit.org/


Compiling
---------

Simply go to the directory where you extracted libsecondlife, where the 
libsecondlife.build file is located and type "nant". To build a zip file 
package use "nant package", and to generate documentation use "nant doc".


Happy fiddling,
--libsecondlife Team
