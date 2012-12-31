@echo off
@PATH=C:\Windows\Microsoft.NET\Framework\v3.5;C:\Program Files\NSIS
@title Making setVersion Tools
@echo on
@csc /o /w:1 /out:..\..\setVersion.exe setVersion.cs /nologo /utf8output
@IF "%ERRORLEVEL%" NEQ "0" ( 
    @color f4
    @echo "Compile Tools Error"
) ELSE (
    @color f2
    @echo "Tools Done"
)
@pause