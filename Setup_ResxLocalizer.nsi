; example2.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects,

;--------------------------------

!define APP "ResxLocalizer"
!define COM "HIRAOKA HYPERS TOOLS, Inc."

!define VER "0.1"
!define APV "0_1"

; The name of the installer
Name "${APP} ${VER}"

; The file to write
OutFile "Setup_${APP}_${APV}.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\${APP}"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\${COM}\${APP}" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------

; Pages

Page license
Page directory
Page components
Page instfiles

LicenseData "LICENSE"

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------

!ifdef SHCNE_ASSOCCHANGED
!undef SHCNE_ASSOCCHANGED
!endif
!define SHCNE_ASSOCCHANGED 0x08000000

!ifdef SHCNF_FLUSH
!undef SHCNF_FLUSH
!endif
!define SHCNF_FLUSH        0x1000

!ifdef SHCNF_IDLIST
!undef SHCNF_IDLIST
!endif
!define SHCNF_IDLIST       0x0000

!macro UPDATEFILEASSOC
  IntOp $1 ${SHCNE_ASSOCCHANGED} | 0
  IntOp $0 ${SHCNF_IDLIST} | ${SHCNF_FLUSH}
; Using the system.dll plugin to call the SHChangeNotify Win32 API function so we
; can update the shell.
  System::Call "shell32::SHChangeNotify(i,i,i,i) ($1, $0, 0, 0)"
!macroend

;--------------------------------

; The stuff to install
Section "ResxLocalizer"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File /x "*.vshost.*" "bin\DEBUG\*.*"
  
  WriteRegStr HKCR ".ResxLocalizer" "" "ResxLocalizer"
  
  WriteRegStr HKCR "ResxLocalizer" "" "ResxLocalizer"
  WriteRegStr HKCR "ResxLocalize\DefaultIcon" "" "$INSTDIR\${APP}.exe,-1"
  WriteRegStr HKCR "ResxLocalizer\shell\open\command" "" "$INSTDIR\${APP}.exe %1"

  DetailPrint "関連付け更新中です。お待ちください。"
  !insertmacro UPDATEFILEASSOC

  ; Write the installation path into the registry
  WriteRegStr HKLM "Software\${COM}\${APP}" "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP}" "DisplayName" "NSIS Example2"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP}" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

  StrCpy $1 "$SMPROGRAMS\${APP}"
  CreateDirectory "$1"
  CreateShortCut  "$1\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut  "$1\${APP}.lnk" "$INSTDIR\${APP}.exe" "" "$INSTDIR\${APP}.exe" 0
  
SectionEnd

Section /o "Open: Microsoft .NET Framework 4.5"
  Exec "http://www.microsoft.com/ja-jp/download/details.aspx?id=30653"
SectionEnd

;--------------------------------

; Uninstaller

Section "Uninstall"
  ; Remove files and uninstaller
  Delete "$INSTDIR\Microsoft.Windows.Shell.dll"
  Delete "$INSTDIR\ResxLocalizer.exe"
  Delete "$INSTDIR\ResxLocalizer.exe.config"
  Delete "$INSTDIR\ResxLocalizer.pdb"
  Delete "$INSTDIR\RibbonControlsLibrary.dll"

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\${APP}\*.lnk"

  ; Remove registry keys
  DeleteRegKey HKCR "ResxLocalizer\shell\open\command"
  DeleteRegKey HKCR "ResxLocalizer\shell\open"
  DeleteRegKey HKCR "ResxLocalizer\shell"
  DeleteRegKey HKCR "ResxLocalizer\DefaultIcon"
  DeleteRegKey HKCR "ResxLocalizer"
  DeleteRegKey HKCR ".ResxLocalizer"

  DetailPrint "関連付け更新中です。お待ちください。"
  !insertmacro UPDATEFILEASSOC

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP}"
  DeleteRegKey HKLM "Software\${COM}\${APP}"

  Delete "$INSTDIR\uninstall.exe"

  ; Remove directories used
  RMDir "$SMPROGRAMS\${APP}"
  RMDir "$INSTDIR"

SectionEnd
