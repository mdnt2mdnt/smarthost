# Smarthost #

**A Remote IP/Host Remap Plugin for Fiddler**

## Future Release will be Published at Github **http://github.com/mooring/smarthost** ##

[Usage with More Images](https://code.google.com/p/smarthost/wiki/Usage)
**[click here to Download latest Version](https://smarthost.googlecode.com/files/SmartHost.1.1.0.3.exe)**

https://smarthost.googlecode.com/files/Smarthost-en.png?1.2

**Introduction**
  1. It's an extension for fiddler
  1. A Remote IP/Host Remap Plugin for fiddler.
  1. A single Domain can be mapped to multiple host without changing the system host but select the ip/domain pair at client.
  1. Suitable for Mobile webpage development
  1. WIFI environment is better



**Usage**
  1. Fiddler 2.4.1.1 is required
  1. Smarthost Menuitem will be placed At the main menubar of Fiddler
  1. check Enabled to make this extension work
  1. Config the hosts you want to remap from Smarthost->Config Host
  1. Config your Mobile http proxy to Fiddler Computer IP and Port
  1. On Mobile:
    * visit http://config.qq.com/form.html for local model
    * ![https://smarthost.googlecode.com/files/remote.png](https://smarthost.googlecode.com/files/remote.png)
    * visit http://config.qq.com/remote.html for remote model
    * ![https://smarthost.googlecode.com/files/local.png](https://smarthost.googlecode.com/files/local.png)
  1. Click submit button to make it work
  1. Check Your Fiddler For Requests





**Compiling Note**
  1. .Net framework 2.0 SDK is required http://www.microsoft.com/en-us/download/details.aspx?id=19988
  1. NSIS Unicode is required http://code.google.com/p/unsis/downloads/list
  1. Change Path Value in make.cmd file
  1. Let csc.exe and makensis.exe in your PATH ENV(.ie include Path\_to\_Microsoft.NET\Framework\Version\_Number  and Path\_to\_NSIS in your path)
  1. Run Make.cmd in src folder
  1. Smarthost.dll and Smarthost.exe will be placed at obj folder if no error occurred


**Change log**
  1. 1.1.0.3 fixed https blank response errors on remote model
  1. 1.1.0.2 add config remote save support
  1. 1.1.0.0 add UpStreaming proxy support
  1. 1.0.2.8 add remote log and ip/host config adding on mobile
  1. 1.0.2.6 fix a menu not show bug under fiddler v4.x beta version
  1. 1.0.2.5 add remote log for mobile debuging

---



**Feature List**
```
//1.1.0.2 Feature
/*
 * visit http://smart.host/form.html?oid=your_name will save you ip/host map at fiddler 
 *    and can be loaded next time
 * no remote proxy model optimize
 */

//1.1.0.0 Feature
/*
 *config Host at your client first
 *visit http://smart.host/remote.html on your client
 *setting up Up-streaming Proxy and Port like 192.168.1.10:8888 , submit
 *visit some page then checking your Up-streaming proxy
 */

//1.0.2.8 Feature :
//remote log view as script with url
function remoteLog(res){
    for(var i=0,il=res.length;i<il;i++){
        console.log(res[i].clientIP,res[i].serverIP,res[i].Host, res[i].status);
    }
}
var script = document.createElement("script");
script.type = "text/javascript";
//machine_ip can be 192.168.1.1 , your_mobile_ip can be 192.168.1.10 
script.src = "http://machine_ip:8888/log/?ip=your_mobile_ip&callback=remoteLog";
document.getElementsByTagName("head")[0].appendChild(script);
/*
 *********************************************************************
 * Params support:
 * ip        string required client ip which will be showed
 * saz       int    optional 1/0 download sessions or not
 * id        int    optional the session id start with
 * size      int    optional page Size max is 100(except download as saz)
 * callback  string optional jsonp callback name
 *********************************************************************
 * Return Data Example:
 *  callback([{id:1,
 *    clienIP:"192.168.1.1",
 *    serverIP:"111.112.113.114",
 *    host:"demo.host.com",
 *    method:"GET",
 *    status:304,
 *    requestHeaders:"GET / HTTP/1.1\nHost: demo.host.com\n.....",
 *    responseHeaders:"HTTP/1.1 304 Not Modified\nServer: JSP2/1.0.16....",
 *    requestBody: "",
 *    times:""ClientConnected: 00:06:28.324,...Overall Elapsed: 0:00:00.406"
 *   }
 *   other records
 * ]);
 ***********************************************************************
 *
 * http://machine_ip_with_fiddler:8888/log/?ip=client_ip&saz=1
 * return sessions of of request client_ip as saz file 
 */

//1.0.2.6 Feature:
//remote debug usage for javascript
new Image().src = "http://smart.host/log/?your_echo_string";
// then open the log tab on fiddler window, you_echo_string will be there
```


### **Contact** ###
  * WeChat: smart-host
  * ![https://smarthost.googlecode.com/files/wechatqrcode.jpg](https://smarthost.googlecode.com/files/wechatqrcode.jpg)