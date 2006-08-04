#!/bin/bash
## A small file to help build SLIRC in mono.
resgen /compile frmSLIRC.resx Properties/Resources.resx
gmcs -r:bin/Debug/Meebey.SmartIrc4net.dll -r:bin/Debug/libsecondlife.dll -r:System.Windows.Forms -r:System.Drawing -r:System.Data *.cs Properties/*.cs
mv Program.exe bin/Debug/SLIRC.exe
