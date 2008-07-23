bin\Prebuild.exe /target nant
bin\Prebuild.exe /target vs2005

echo C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\msbuild OpenMetaverse.sln > compile.bat
echo C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\msbuild /p:Configuration=Release OpenMetaverse.sln > builddocs.bat
echo SandCastleBuilderConsole.exe docs\OpenMetaverse-docs.shfb >> builddocs.bat
echo 7za.exe a -tzip docs\documentation.zip docs\trunk >> builddocs.bat
