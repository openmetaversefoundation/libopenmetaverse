@echo off
::
:: Prebuild generator for the OpenMetaverse Library
::
:: Command Line Options:
:: (none)            - create solution/project files and create compile.bat file to build solution
:: msbuild           - Create project files, compile solution
:: msbuild runtests  - create project files, compile solution, run unit tests
:: msbuild docs      - create project files, compile solution, build API documentation
:: msbuild docs dist - Create project files, compile solution, run unit tests, build api documentation, create binary zip
::                   - and exe installer
::
:: nant		     - Create project files, run nant to compile solution
:: nant runtests     - Create project files, run nant to compile solution, run unit tests
::

echo ##########################################
echo creating prebuild files for: nant, vs2008
echo Parameters: %1 %2
echo ##########################################

:: run prebuild to generate solution/project files from prebuild.xml configuration file
bin\Prebuild.exe /target nant
bin\Prebuild.exe /target vs2008

:: build compile.bat file based on command line parameters
echo @echo off > compile.bat
if(.%1)==(.) echo C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild OpenMetaverse.sln >> compile.bat

if(.%1)==(.msbuild) echo "echo ==== COMPILE BEGIN ====" >> compile.bat
if(.%1)==(.msbuild) echo C:\WINDOWS\Microsoft.NET\Framework\v3.5\msbuild /p:Configuration=Release OpenMetaverse.sln >> compile.bat
if(.%1)==(.msbuild) echo IF ERRORLEVEL 1 GOTO FAIL >> compile.bat

if(.%1)==(.nant) echo nant >> compile.bat
if(.%1)==(.nant) echo IF ERRORLEVEL 1 GOTO FAIL >> compile.bat

if(.%3)==(.docs) echo "echo ==== GENERATE DOCUMENTATION BEGIN ====" >> compile.bat
if(.%2)==(.docs) echo SandCastleBuilderConsole.exe docs\OpenMetaverse-docs.shfb >> compile.bat
if(.%2)==(.docs) echo IF ERRORLEVEL 1 GOTO FAIL >> compile.bat
if(.%2)==(.docs) echo 7za.exe a -tzip docs\documentation.zip docs\trunk >> compile.bat
if(.%2)==(.docs) echo IF ERRORLEVEL 1 GOTO FAIL >> compile.bat

if(.%3)==(.runtests) echo "echo ==== UNIT TESTS BEGIN ====" >> compile.bat
if(.%2)==(.runtests) echo "nunit-console.exe" /nologo /nodots /labels /exclude:NetworkTests bin\OpenMetaverse.Tests.dll >> compile.bat
if(.%2)==(.runtests) echo IF ERRORLEVEL 1 GOTO FAIL >> compile.bat

:: nsis compiler needs to be in path
if(.%3)==(.dist) echo "echo ==== GENERATE DISTRIBUTION BEGIN ====" >> compile.bat
if(.%3)==(.dist) echo makensis.exe /DPlatform=test docs\OpenMetaverse-installer.nsi >> compile.bat
if(.%3)==(.dist) echo IF ERRORLEVEL 1 GOTO FAIL >> compile.bat
if(.%3)==(.dist) echo 7za.exe a -tzip dist\openmetaverse-dist.zip @docs\distfiles.lst >> compile.bat
if(.%3)==(.dist) echo IF ERRORLEVEL 1 GOTO FAIL >> compile.bat

echo :SUCCESS >> compile.bat
echo echo Build Successful! >> compile.bat
echo exit /B 0 >> compile.bat
echo :FAIL >> compile.bat
echo echo Build Failed, check log for reason >> compile.bat
echo exit /B 1 >> compile.bat

:: perform the appropriate action
if(.%1)==(.msbuild) compile.bat
if(.%1)==(.nant) compile.bat
if(.%1)==(.dist) compile.bat





