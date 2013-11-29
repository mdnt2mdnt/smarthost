Page                  license
Page                  instfiles
RequestExecutionLevel user

OutFile               "..\obj\SmartHost.exe"
Caption               "SmartHost Fiddler Extension"
Icon                  "..\package\favicon.ico"
LicenseData           "..\package\readme.txt"
Name                  "SmartHost"
CRCCheck              on
XPStyle               on
AutoCloseWindow       false
ShowUninstDetails     show
LicenseBkColor        ffffff
LicenseText           "Ready To Intall Smarthost Extension for Fiddler?" "Next"
DetailsButtonText     "DETAILS"
CompletedText         "Completed"
MiscButtonText        "Previous" "Next" "Cancel" "OK"
InstallButtonText     "Install"

UninstallCaption      "Remove SmartHost Extension"
UninstallButtonText   "Remove"
UninstallIcon         "..\package\favicon.ico"
UninstallText         "Are you Sure" "Smarthost path:"
UninstPage            uninstConfirm
UninstPage            instfiles

VIProductVersion                      1.1.0.1
VIAddVersionKey ProductName           "SmartHost"
VIAddVersionKey Comments              "All Right Reserved By Mooring"
VIAddVersionKey CompanyName           "Tencent .Ltd"
VIAddVersionKey FileDescription       "A Simple Host Mapping Tool for Fiddler"
VIAddVersionKey FileVersion           1.1.0.1
VIAddVersionKey ProductVersion        1.1.0.1
VIAddVersionKey LegalCopyright        "Copyright By mooringniu 2013"
VIAddVersionKey InternalName          "SmartHost.exe"
VIAddVersionKey OriginalFilename      "SmartHost.exe"

Function .onGUIEnd
    IfFileExists "$DOCUMENTS\Fiddler2\Scripts\Smarthost\hostEditor.hta" ext next
    ext:
        Exec  '"mshta.exe" "$DOCUMENTS\Fiddler2\Scripts\Smarthost\hostEditor.hta"'
    next:
FunctionEnd

Section SmartHost
    ReadRegStr $0   HKLM SOFTWARE\Microsoft\Fiddler2 "InstallPath"
    StrCmp     "$0" ""   eq0  nt0
    eq0:
        MessageBox   MB_OK|MB_ICONEXCLAMATION "Can't find Fiddler, Installation Will Quit"
        Quit
    nt0:
    StrCpy           $R1 "$0"
    StrCpy           $R2 "$OUTDIR"
    
    SetOutPath       "$DOCUMENTS\Fiddler2\"
    CreateDirectory  $OUTDIR\Captures\Responses
    CreateDirectory  $OUTDIR\Scripts\Smarthost
    File             "/oname=$OUTDIR\Scripts\Smarthost.dll"             ..\obj\Smarthost.dll
    File             "/oname=$OUTDIR\Scripts\Smarthost\README.txt"      ..\package\readme.txt
    File             "/oname=$OUTDIR\Captures\Responses\form.html"      ..\package\form.html
    File             "/oname=$OUTDIR\Captures\Responses\help.txt"       ..\package\help.txt
    File             "/oname=$OUTDIR\Captures\Responses\remote.html"    ..\package\remote.html
    File             "/oname=$OUTDIR\Captures\Responses\done.html"      ..\package\done.html
    File             "/oname=$OUTDIR\Captures\Responses\rdone.html"     ..\package\rdone.html
    File             "/oname=$OUTDIR\Captures\Responses\blank.gif"      ..\package\blank.gif
    File             "/oname=$OUTDIR\Captures\Responses\favicon.ico"    ..\package\favicon.ico
    File             "/oname=$OUTDIR\Scripts\Smarthost\smarthost.ico"   ..\package\favicon.ico
    File             "/oname=$OUTDIR\Scripts\Smarthost\hostEditor.hta"  ..\package\hostEdit.hta
    File             "/oname=$OUTDIR\Scripts\Smarthost\extend.js"       ..\package\extend.js
    File             "/oname=$OUTDIR\Scripts\Smarthost\jquery.js"       ..\package\jquery.js
    File             "/oname=$OUTDIR\Scripts\Smarthost\style.css"       ..\package\style.css
    CreateDirectory  "$OUTDIR\Captures\Responses\Configs"
    CreateDirectory  "$OUTDIR\Captures\Responses\Packages"
    WriteUninstaller "$OUTDIR\Scripts\Smarthost\Uninstall.exe"
    WriteRegStr HKCU "Software\SmartHost" "HostPath" "$OUTDIR"
    WriteRegStr HKCU "Software\SmartHost" "InstallPath" "$OUTDIR"
    Delete           "$OUTDIR\Scripts\CustomRules.js"
    IfFileExists     "$OUTDIR\Scripts\Smarthost\hosts" fex fnex
    fnex:
        File         "/oname=$OUTDIR\Scripts\Smarthost\hosts"           ..\package\hosts
    fex:
SectionEnd


;Section DesktopShortCut
;    CreateShortCut  "$DESKTOP\Config.lnk" "$OUTDIR\Scripts\Smarthost\hostEditor.hta" "" \
;                    "$OUTDIR\Scripts\Smarthost\smarthost.ico"
;SectionEnd

Section Uninstall
    SetOutPath    "$DOCUMENTS\Fiddler2\"
    Delete        "$Desktop\Config.lnk"
    Delete        "$OUTDIR\Scripts\Smarthost\README.txt"
    Delete        "$OUTDIR\Captures\Responses\blank.gif"
    Delete        "$OUTDIR\Captures\Responses\form.html"
    Delete        "$OUTDIR\Captures\Responses\help.txt"
    Delete        "$OUTDIR\Captures\Responses\done.html"
    Delete        "$OUTDIR\Captures\Responses\rdone.html"
    Delete        "$OUTDIR\Captures\Responses\remote.html"
    Delete        "$OUTDIR\Captures\Responses\favicon.ico"
    Delete        "$OUTDIR\Scripts\Smarthost\smarthost.ico"
    Delete        "$OUTDIR\Scripts\Smarthost\hosts"
    Delete        "$OUTDIR\Scripts\Smarthost\hostEditor.hta"
    Delete        "$OUTDIR\Scripts\Smarthost\extend.js"
    Delete        "$OUTDIR\Scripts\Smarthost\jquery.js"
    Delete        "$OUTDIR\Scripts\Smarthost\style.css"
    Delete        "$OUTDIR\Scripts\Smarthost\Uninstall.exe"
    Delete        "$OUTDIR\Scripts\Smarthost.dll"
    DeleteRegKey   HKCU "Software\SmartHost"
    RMDir         "$OUTDIR\Scripts\Smarthost"
    RMDir         "$OUTDIR\Captures\Responses\Configs"
    RMDir         "$OUTDIR\Captures\Responses\Packages"
SectionEnd
