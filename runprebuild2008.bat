@echo off
bin\Prebuild.exe /target nant
bin\Prebuild.exe /target vs2008

echo C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild OpenMetaverse.sln > compile.bat

echo C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild /p:Configuration=Release OpenMetaverse.sln > builddocs.bat
echo SandCastleBuilderConsole.exe docs\OpenMetaverse-docs.shfb >> builddocs.bat
echo 7za.exe a -tzip docs\documentation.zip docs\trunk >> builddocs.bat

if(%1)==(msbuild) compile.bat
if(%1)==(docs) builddocs.bat
if(%1)==(nant) nant