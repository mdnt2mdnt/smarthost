@echo off
set FIDDLER=D:\Programs\Fiddler2\Fiddler.exe
PATH=C:\Windows\Microsoft.NET\Framework\v3.5;D:\Programs\NSIS
@del /f /q ..\obj\SmartHost.dll ..\obj\SmartHost.exe
title Making SmartHost Plugin
..\tools\setVersion.exe 1.0.2.5 SmartHost.cs install.nsi
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
