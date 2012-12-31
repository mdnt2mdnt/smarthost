@echo off
PATH=C:\Windows\Microsoft.NET\Framework\v3.5;C:\Program Files\NSIS
@del /f /q ..\obj\SmartHost.dll ..\obj\SmartHost.exe
title Making SmartHost Plugin
REM ..\tools\setVersion.exe 1.0.2.3 SmartHost.cs install.nsi
@echo on
@csc /o /w:1 /out:..\obj\Smarthost.dll /target:library SmartHost.cs /reference:"C:\Program Files\Fiddler\Fiddler.exe" /nologo /utf8output
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
