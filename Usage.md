# Smarthost #

**An IP/Domain Remap Extension for Fiddler**

![https://smarthost.googlecode.com/files/follow-chat_en.png](https://smarthost.googlecode.com/files/follow-chat_en.png)
https://smarthost.googlecode.com/files/network0.png?1
https://smarthost.googlecode.com/files/network1.png?1


**Introduction**
  1. It's an extension for fiddler
  1. A single Domain can be mapped to multiple host without changing the system host but select the ip/domain pair at client.
  1. Suitable for Mobile webpage development
  1. WIFI environment are better



**Usage**
  1. Fiddler 2.4.1.1 is required
  1. Close Fiddler first
  1. Click Smarthost.exe to install
  1. Smarthost Menuitem will be placed At the main menubar of Fiddler
  1. Check Enabled to make this extension work
  1. Config the hosts you want to remap from Smarthost->Config Host
  1. Set default proxy to fiddler then visit http://smart.host or http://config.qq.com
  1. Click submit button to make it work



**Compile Note**
  1. .Net framework 2.0 SDK is required http://www.microsoft.com/en-us/download/details.aspx?id=19988
  1. NSIS Unicode is required http://code.google.com/p/unsis/downloads/list
  1. Change Path Value in make.cmd file
  1. Let csc.exe and makensis.exe in your PATH ENV(.ie include Path\_to\_Microsoft.NET\Framework\Version\_Number  and Path\_to\_NSIS in your path)
  1. Run Make.cmd in src folder
  1. Smarthost.dll and Smarthost.exe will be placed at obj folder if no error occurred