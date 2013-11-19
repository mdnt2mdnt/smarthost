@echo off
set FIDDLER="C:\Program Files\Fiddler2\Fiddler.exe"
PATH=C:\Windows\Microsoft.NET\Framework\v2.0.50727;C:\Program Files\NSIS\
@del /f /q ..\obj\SmartHost.dll ..\obj\SmartHost.exe
title Making SmartHost Plugin
REM ..\tools\setVersion.exe 1.1.0.1 SmartHost.cs install.nsi
@echo on
@csc /o /w:1 /out:..\obj\Smarthost.dll /target:library SmartHost.cs /reference:%FIDDLER% /nologo /utf8output
@IF "%ERRORLEVEL%" NEQ "0" ( 
    @color f4
    @echo "Compile Dll Error"
) ELSE (
    makensis.exe /V1 install.nsi
    @IF "%ERRORLEVEL%" NEQ "0" ( 
        @color f4
        @echo "Packaging Exe Error"
    ) ELSE (
        @copy ..\obj\Smarthost.exe .\
        @color f2
        @echo "All Done"
    )
)
@pause
