
Function .onInit

SetOutPath $TEMP
  File /oname=spltmp.bmp "InstallSplash.bmp"

; optional
 File /oname=spltmp.wav "libomv-chiptune-mono.wav"

  splash::show 2500 $TEMP\spltmp

  Pop $0 ; $0 has '1' if the user closed the splash screen early,
	 ; '0' if everything closed normally, and '-1' if some error occurred.

  Delete $TEMP\spltmp.bmp

; prevent installer from being started twice
System::Call 'kernel32::CreateMutexA(i 0, i 0, t "OpenMetaverseInstaller") i .r1 ?e'
Pop $R0
StrCmp $R0 0 +3
  MessageBox MB_OK|MB_ICONEXCLAMATION "The OpenMetaverse installer is already running."
  Abort

SectionSetFlags SEC01 17 ; locks first section, ie forced to install

; get release version information
File /oname=omvdll.dll "..\bin\OpenMetaverse.dll"
GetDLLVersion "$TEMP\omvdll.dll" $R0 $R1
IntOp $R2 $R0 >> 16
IntOp $R2 $R2 & 0x0000FFFF ; $R2 now contains major version
IntOp $R3 $R0 & 0x0000FFFF ; $R3 now contains minor version
IntOp $R4 $R1 >> 16
IntOp $R4 $R4 & 0x0000FFFF ; $R4 now contains release
IntOp $R5 $R1 & 0x0000FFFF ; $R5 now contains build
Var /GLOBAL PRODUCT_MAJOR
Var /GLOBAL PRODUCT_MINOR
Var /GLOBAL PRODUCT_RELEASE
Var /GLOBAL PRODUCT_BUILD

StrCpy $PRODUCT_MAJOR $R2
StrCpy $PRODUCT_MINOR $R3
StrCpy $PRODUCT_RELEASE $R4
StrCpy $PRODUCT_BUILD $R5
Delete $TEMP\omvdll.dll

FunctionEnd

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "OpenMetaverse"
!define PRODUCT_VERSION "$PRODUCT_MAJOR.$PRODUCT_MINOR.$PRODUCT_RELEASE (build $PRODUCT_BUILD)"
!define PRODUCT_PUBLISHER "OpenMetaverse Ninjas"
!define PRODUCT_WEB_SITE "http://www.libsecondlife.org/"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\OpenMetaverse.dll"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

VIProductVersion "1.0.0.0"
VIAddVersionKey "ProductName" "OpenMetaverse Library"
VIAddVersionKey "Comments" ""
VIAddVersionKey "CompanyName" "OpenMetaverse Ninjas"
VIAddVersionKey "LegalTrademarks" "See License.txt for licensing terms"
VIAddVersionKey "LegalCopyright" "© OpenMetaverse"
VIAddVersionKey "FileDescription" "OpenMetaverse Installer"
VIAddVersionKey "FileVersion" "1.0.0"

; MUI 1.67 compatible ------
!include "MUI2.nsh"
BrandingText "OpenMetaverse Installer v2.36"
; MUI Settings
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "InstallerHeader.bmp" ; optional
;!define MUI_BGCOLOR 001122
;!define MUI_HEADER_TRANSPARENT_TEXT
!define MUI_WELCOMEFINISHPAGE_BITMAP "InstallWelcome.bmp"
!define MUI_ABORTWARNING

!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!define MUI_LICENSEPAGE_BGCOLOR /grey
;BGGradient 001122 0058B0 FFFFFF
; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!insertmacro MUI_PAGE_LICENSE "..\LICENSE.txt"
; Components page
!insertmacro MUI_PAGE_COMPONENTS
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
;!define MUI_FINISHPAGE_RUN "$INSTDIR\GUITestClient.exe"
!define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\docs\README.txt"
!define MUI_FINISHPAGE_LINK "OpenMetaverse"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.openmetaverse.org/"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

; MUI end ------
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "OpenMetaverseInstaller.exe"
XPStyle on
InstallDir "$PROGRAMFILES\OpenMetaverse\libomv"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

; required base system!
Section "!Base Libraries" SEC01
  SetOutPath "$INSTDIR\bin"
  CreateDirectory "$SMPROGRAMS\OpenMetaverse\libomv"
  SetOverwrite ifnewer
  File "..\bin\log4net.dll"
  File "..\bin\openjpeg-dotnet.dll"
  File "..\bin\OpenMetaverse.dll"
  File "..\bin\OpenMetaverse.GUI.dll"
  File "..\bin\OpenMetaverse.Http.dll"
  File "..\bin\OpenMetaverse.StructuredData.dll"
  File "..\bin\OpenMetaverse.Utilities.dll"
  File "..\bin\OpenMetaverseTypes.dll"
  File "..\bin\OpenMetaverse.dll.config"
  File "..\README.txt"
  File "..\License.txt"
  File "..\bin\XMLRPC.dll"
SectionEnd

Section "API Documentation" SEC02
  SetOutPath "$INSTDIR\docs"
;  CreateDirectory "$SMPROGRAMS\OpenMetaverse\libomv\docs"
  File "trunk\OpenMetaverse.chm"
  File "..\README.txt"
  CreateShortCut "$SMPROGRAMS\OpenMetaverse\libomv\README.lnk" "$INSTDIR\docs\README.txt"
  SetOutPath "$INSTDIR\bin"
  File "..\bin\*.XML"
  CreateShortCut "$SMPROGRAMS\OpenMetaverse\libomv\API Documentation.lnk" "$INSTDIR\docs\OpenMetaverse.chm"
  CreateShortCut "$SMPROGRAMS\OpenMetaverse\libomv\Library and Examples.lnk" $INSTDIR\bin"
SectionEnd

Section "Example Applications" SEC03
  SetOutPath "$INSTDIR\bin"
;  File "..\bin\*.exe"
   File "..\bin\Dashboard.exe"
   File "..\bin\GridAccountant.exe"
   File "..\bin\GridImageUpload.exe"
   File "..\bin\GridProxyApp.exe"
   File "..\bin\GridProxy.dll"
   File "..\bin\groupmanager.exe"
   File "..\bin\TestClient.exe"
   ; PrimWorkShop/AvatarPreview
   File "..\bin\PrimWorkshop.exe"
   File "..\bin\AvatarPreview.exe"
   File "..\bin\GlacialList.dll"
   File "..\bin\Tao.OpenGL.dll"
   File "..\bin\Tao.Platform.Windows.dll"
   File "..\bin\ICSharpCode.SharpZipLib.dll"
   File "..\bin\WinGridProxy.exe"
   File "..\bin\WinGridProxy.exe.config"
   File "..\bin\Be.Windows.Forms.HexBox.dll"
SectionEnd

Section -AdditionalIcons
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\OpenMetaverse\libomv Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\OpenMetaverse\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\OpenMetaverse.dll"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\OpenMetaverse.dll"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

; Section descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC01} "Core library components required for application usage"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC02} "API Documentation files in CHM format and Intellisense Databases for Visual Studio"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC03} "Example applications including TestClient"
!insertmacro MUI_FUNCTION_DESCRIPTION_END


Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  Delete "$INSTDIR\${PRODUCT_NAME}.url"
  Delete "$INSTDIR\uninst.exe"

  Delete "$INSTDIR\docs\*"
  RMDir "$INSTDIR\docs"

  Delete "$INSTDIR\bin\*"
  RMDir "$INSTDIR\bin"

  Delete "$SMPROGRAMS\OpenMetaverse\Uninstall.lnk"
  Delete "$SMPROGRAMS\OpenMetaverse\libomv Website.lnk"
  Delete "$SMPROGRAMS\OpenMetaverse\libomv\README.lnk"
  Delete "$SMPROGRAMS\OpenMetaverse\libomv\API Documentation.lnk"
  Delete "$SMPROGRAMS\OpenMetaverse\libomv\Library and Examples.lnk"
  RMDir "$SMPROGRAMS\OpenMetaverse\libomv"
  RMDir "$SMPROGRAMS\OpenMetaverse"
  RMDir "$INSTDIR"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd


