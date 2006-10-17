
LibsecondLife Trunk
-------------------

Welcome.  [Add something for real here.  --TSK]


Microsoft Visual Studio
-----------------------

Visual Studio 2005 project files are included to ease compiling on Windows 
platforms. Some of the projects use a custom targets file that allows you
to compile against .NET 1.0/1.1, mono, or the Compact Framework. You will 
need to authorize this custom file when you first open the solution file.


Software Needed (all Freeware)
------------------------------

Basic Stuff (Windows):
   MS .NET Framework (1.1 and/or 2.0 depending on what you're working on; 
      preferrably both).  Get directly from Windows Update.
   Visual C# Express - http://msdn.microsoft.com/vstudio/express/visualcsharp/
   nUnit Framework (2.0 apparently) - http://www.nunit.org/index.php?p=download
   nAnt - http://nant.sourceforge.net/

Visual Stuff (i.e. sceneviewer) for Windows:
   XNA - http://msdn.microsoft.com/directx/XNA/default.aspx
   XGE - http://msdn.microsoft.com/directx/xna/gse/


Install and Config Order (for 'quickest' bootstrap under Windows)
-----------------------------------------------------------------

0.5) Checkout a copy of libsecondlife trunk; See the GNA project page for details.  This differs by preference and if you're pulling as a compiler or developer; Configuration details for the various methods could be coaxed out if someone would like to poke me (tkimball) with an interest in this info.  ;-)

1) Get .Net Frameworks (1.1 and 2.0) plus any patches installed via Windows Update.  Multiple reboots may be needed.

2) Install Visual C# Express (defaults are fine)

3) Install nUnit Framework (defaults are fine)

4) Unpack the 0.85 binary of nAnt in a directory you can be happy with (I chose C:\bin\nant-0.85\bin).  Add this to your XP PATH variable:
   Start->My Computer->[Right Click]->Properties (new window)
   Advanced Tab -> Environment Variables (new window)
   System Variables is bottom subwindow, click on 'path' and then the 'Edit' button
      below that. (new window)
   Add ';C:\bin\nant-0.85\bin' or whereever it is to the line and click OK.
   Click OK twice more to close out system config windows.

5) In a Command Prompt (yea you heard that right) cd to where you checked out the trunk (You're looking for the dir that has libsecondlife.build and Ovastus.CSharp.targets).  I'm going to call this %TRUNK% from here on.
   Run 'nant' and sit back.  'cd bin' and enjoy!

6) [Optional]  Not all apps and examples will be in nant due to various compatibility and stability issues.  If you're still interested:
  Under %TRUNK%\libsecondlife-cs, open the libsecondlife.sln file and Build the Solution (you're likely going to get warnings and such here).
  This may overwrite some of the files you created in step 5; If they don't work re-run nant.


Where to contact the Dev Team members
-------------------------------------
[Fill this in pending approval.  --TSK]


Happy Fiddling,
--LibSecondLife Team
