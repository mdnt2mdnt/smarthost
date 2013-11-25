@echo off
PATH=C:\Windows\Microsoft.NET\Framework\v2.0.50727
@del getIP.exe
@csc /o /w:1 /out:getIP.exe  getIP.cs /nologo /utf8output