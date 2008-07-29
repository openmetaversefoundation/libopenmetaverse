@echo off
bin\Prebuild.exe /target nant
bin\Prebuild.exe /target vs2008

echo C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild OpenMetaverse.sln > compile.bat

echo C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild /p:Configuration=Release OpenMetaverse.sln > builddocs.bat
echo SandCastleBuilderConsole.exe docs\OpenMetaverse-docs.shfb >> builddocs.bat
echo 7za.exe a -tzip docs\documentation.zip docs\trunk >> builddocs.bat

if(%2)==(runtests) echo "c:\Program Files\NUnit 2.4.7\bin\nunit-console.exe" bin\OpenMetaverse.Tests.dll > runtests.bat

if(%1)==(msbuild) compile.bat
if(%1)==(docs) builddocs.bat
if(%1)==(nant) nant
if(%2)==(runtests) runtests.bat