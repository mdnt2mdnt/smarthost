/*
 * Copyright by mooringniu@gmail.com ,Any Suggestion Contact me by email
 * Author : mooring
 * Date   : 12/04/2013
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using Microsoft.Win32;
using Fiddler;

[assembly: AssemblyTitle("SmartHost")]
[assembly: AssemblyDescription("A Romote IP/Host Remap Plugin for Fiddler")]
[assembly: AssemblyCompany("Tencent .Ltd")]
[assembly: AssemblyCopyright("Copyright Mooringniu@Tencent 2013")]
[assembly: AssemblyProduct("SmartHost")]
[assembly: AssemblyTrademark("SmartHost")]
[assembly: AssemblyVersion("1.1.0.3")]
[assembly: AssemblyFileVersion("1.1.0.3")]
[assembly: Fiddler.RequiredVersion("2.4.1.1")]

public class SmartHost : IAutoTamper
{
    private bool _tamperHost = false;
    private string _notifySrv = String.Empty;
    private string _scriptDir = String.Empty;
    private string _pluginDir = String.Empty;
    private string _wifiIP = String.Empty;
    private string _lanIP = String.Empty;
    private Dictionary<string, string> usrConfig;
    private MenuItem mnuSmartHost;
    private MenuItem mnuSmartHostEnabled;
    private MenuItem mnuSmartHostConfig;
    private MenuItem mnuSmartHostReadme;
    private MenuItem mnuSmartHostAbout;
    private MenuItem mnuSplit;
    private MenuItem mnuSplit1;

    public SmartHost()
    {
        this.initConfig();
        this.initializeMenu();
        this.getPluginPath();
        this.reportAdapterAddress();
    }
    private void initConfig()
    {
        this.usrConfig   = new Dictionary<string, string>();
        this._tamperHost = FiddlerApplication.Prefs.GetBoolPref("extensions.smarthost.enabled", false);
        this._notifySrv  = FiddlerApplication.Prefs.GetStringPref("extensions.smarthost.setip", "");
        if (this._notifySrv.Length>0 && this._notifySrv.StartsWith("http:",StringComparison.OrdinalIgnoreCase)) {
            //NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(this.networkAvailabilityChangeHandler);
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(this.networdAddressChangeHandler);
        }
    }
    private void networdAddressChangeHandler(object sender, EventArgs e)
    {
        this.reportAdapterAddress();
    }
    private void networkAvailabilityChangeHandler(object sender, NetworkAvailabilityEventArgs e)
    {
        this.reportAdapterAddress();
    }
    private void initializeMenu()
    {
        this.mnuSmartHostEnabled = new MenuItem();
        this.mnuSmartHostEnabled.Index = 0;
        this.mnuSmartHostEnabled.Text = "&Enabled";
        this.mnuSmartHostEnabled.Checked = this._tamperHost;
        this.mnuSmartHostEnabled.Click += new EventHandler(this._smarthostEnabled_click);
        this.mnuSmartHostConfig = new MenuItem();
        this.mnuSmartHostConfig.Index = 1;
        this.mnuSmartHostConfig.Text = "&Config Hosts";
        this.mnuSmartHostConfig.Click += new EventHandler(this._smarthostConfig_click);
        this.mnuSmartHostReadme = new MenuItem();
        this.mnuSmartHostReadme.Index = 2;
        this.mnuSmartHostReadme.Text = "&Readme";
        this.mnuSmartHostReadme.Click += new EventHandler(this._smarthostReadme_click);
        this.mnuSplit = new MenuItem();
        this.mnuSplit.Index = 3;
        this.mnuSplit.Text = "-";
        this.mnuSplit.Checked = true;
        this.mnuSmartHostAbout = new MenuItem();
        this.mnuSmartHostAbout.Index = 4;
        this.mnuSmartHostAbout.Text = "&About";
        this.mnuSmartHostAbout.Click += new EventHandler(this._smarthostAbout_click);
        this.mnuSmartHost = new MenuItem();
        this.mnuSmartHost.Text = "&SmartHost";
        this.mnuSmartHost.MenuItems.AddRange(new MenuItem[]{
                this.mnuSmartHostEnabled,
                this.mnuSmartHostConfig,
                this.mnuSmartHostReadme,
                this.mnuSplit,
                this.mnuSmartHostAbout
        });
    }
    [CodeDescription("If Enabled, each request will be dealed")]
    private void _smarthostEnabled_click(object sender, EventArgs e)
    {
        MenuItem oSender = (sender as MenuItem);
        oSender.Checked = !oSender.Checked;
        this._tamperHost = oSender.Checked;
    }
    [CodeDescription("Config MenuItem clicked Event Handler")]
    private void _smarthostConfig_click(object sender, EventArgs e)
    {
        string argPath = this._scriptDir + @"\hostEditor.hta";
        if (File.Exists(argPath)){
            Fiddler.Utilities.RunExecutable("mshta.exe", "\"" + argPath + "\"");
        }else{
            this.printJSLog("hostEditor.hta not found at the Scripts folder,Please Reinstall SmartHost Plugin");
        }
    }
    [CodeDescription("Readme menuItem clicked Event Handler")]
    private void _smarthostReadme_click(object sender, EventArgs e)
    {
        string argPath = this._scriptDir + @"\Readme.txt";
        if (File.Exists(argPath)){
            Fiddler.Utilities.RunExecutable("notepad.exe", "\"" + argPath + "\"");
        }else{
            this.printJSLog("Readme.txt not found at the Scripts folder,Please Reinstall SmartHost Plugin");
        }
    }
    [CodeDescription("About menuItem clicked Event Handler")]
    private void _smarthostAbout_click(object sender, EventArgs e)
    {
        MessageBox.Show(
            "Smarthost For Fiddler\n--------------------------------------------------"
            + "\nA Remote IP/Host REMAP Add-on for Fiddler"
            + "\nMaking mobile developming More Easier.\n"
            + "\nFileVersion: 1.1.0.3\n"
            + "\nAny suggestion contact mooringniu@gmail.com",
            "About SmartHost",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    [CodeDescription("print jslog to fiddler for mobile debuging")]
    private void printJSLog(string log)
    {
        FiddlerApplication.Log.LogFormat("SmartHost: {0}\n",new object[1]{(object) log});
    }
    [CodeDescription("set WireLess & LanIP for future Use")]
    private void getAdapterAddress()
    {
        foreach(NetworkInterface NI in NetworkInterface.GetAllNetworkInterfaces()) {
            if(NI.NetworkInterfaceType==NetworkInterfaceType.Wireless80211) {
                foreach(UnicastIPAddressInformation IP in NI.GetIPProperties().UnicastAddresses) {
                    if (IP.Address.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork && IP.IsDnsEligible && !NI.Description.Contains("Virtual")) {
                        this._wifiIP = IP.Address.ToString();
                        break;
                    }
                }
            } else if(NI.NetworkInterfaceType==NetworkInterfaceType.Ethernet) {
                foreach(UnicastIPAddressInformation IP in NI.GetIPProperties().UnicastAddresses) {
                    if (IP.Address.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork && IP.IsDnsEligible && !NI.Description.Contains("Virtual")) {
                        //USB Wireless Network Adapter Will be Here
                        if(NI.Description.Contains("Wireless") && NI.Description.Contains("802.11")){
                            this._wifiIP = IP.Address.ToString();
                        }else{
                            this._lanIP = IP.Address.ToString();
                            break;
                        }
                    }
                }
            }
        }
        if(this._wifiIP.Length>0 || this._lanIP.Length>0){
            this.printJSLog("IP Address \t"+(this._wifiIP.Length>0?" WIFI:"+this._wifiIP:"")+(this._lanIP.Length>0?"\tEthernet:"+this._lanIP:""));
        }
    }
    [CodeDescription("send IP Config for other programs")]
    private void reportAdapterAddress()
    {
        this.getAdapterAddress();
        if (!this._notifySrv.StartsWith("http://",StringComparison.OrdinalIgnoreCase)) { return; }
        string url = this._notifySrv;
        url += (this._wifiIP.Length>0?(url.Contains("?")?"&":"?")+"wanip="+this._wifiIP:"");
        url += (this._lanIP.Length >0?(url.Contains("?")?"&":"?")+"lanip="+this._lanIP:"");
        if(url.Length==this._notifySrv.Length){ return; }
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        if(CONFIG.sGatewayUsername != null && CONFIG.sGatewayPassword != null){
            httpWebRequest.Proxy.Credentials = (ICredentials)new NetworkCredential(CONFIG.sGatewayUsername, CONFIG.sGatewayPassword);
        }else{
            httpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        httpWebRequest.UserAgent = "SmartHost/1.1.0.3";
        httpWebRequest.Referer = "http://smart.host/";
        try{
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            httpWebResponse.Close();
        }catch(Exception e){}
        this.printJSLog(url);
    }
    [CodeDescription("Print Wireless and Lan IP address")]
    private void logAdapterAddress(Session oSession)
    {
        this.responseLogRequest( oSession, "Wireless Proxy:" + this._wifiIP + "\nLanIP Address:" + this._lanIP + "\n", "text/plain", "");
    }
    [CodeDescription("get smarthost install folder from registry")]
    private void getPluginPath()
    {
        string path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders";
        RegistryKey oReg = Registry.CurrentUser.OpenSubKey(path, RegistryKeyPermissionCheck.ReadSubTree);
        if (oReg != null) {
            string docPath = (string)oReg.GetValue("Personal");
            this._pluginDir = docPath + @"\Fiddler2\";
            this._scriptDir = this._pluginDir + @"Scripts\Smarthost\";
            oReg.Close();
        } else {
            this._tamperHost = false;
            this.printJSLog("Can't find User Domuments Folder");
        }
    }
    [CodeDescription("parse client post message")]
    private bool processClientConfig(string postStr, string cIP)
    {
        Dictionary<string, string> pQuery = this.splitString(postStr, new char[] { '&' }, new char[] { '=' });
        bool isRemote = false;
        foreach (string key in pQuery.Keys) {
            if (key == "remoteProxy") {
                List<string> keys = new List<string>(this.usrConfig.Keys);
                foreach(string ipHost in keys){
                    if(ipHost.StartsWith(cIP+"|",StringComparison.OrdinalIgnoreCase)){
                        try{this.usrConfig.Remove(ipHost);}catch(Exception e){}
                    }
                }
                isRemote = true;
                if (pQuery[key].Length > 0) {
                    this.usrConfig[cIP+"|remoteProxy"] = pQuery[key];
                    this.printJSLog("All HTTP Requests from "+cIP+" will be Sent to To: "+pQuery[key]+".\n");
                }else{
                    this.printJSLog("All IP/Host map Configuration of "+cIP+" has been cleaned.");
                }
            } else if (key == "oid" && pQuery[key].Length > 0 && !pQuery[key].Contains("Smarthost_")) {
                this.saveConfig2File(postStr, pQuery[key]);
            } else {
                if (pQuery[key].Length == 0) {
                    this.usrConfig.Remove(cIP + "|" + pQuery[key]);
                } else {
                    this.usrConfig[cIP + "|" + key] = pQuery[key];
                }
            }
        }
        return isRemote;
    }
    [CodeDescription("save client Config To File")]
    private void saveConfig2File(string postStr, string oid)
    {
        oid = Regex.Replace(oid, "[^a-z0-9_\\-\\.]+", "");
        string file = this._pluginDir + @"\Captures\Responses\Configs\" + oid + ".txt";
        try{System.IO.File.WriteAllText(file, postStr);}catch(Exception e){}
    }
    [CodeDescription("save client configuration to userConfig")]
    private void updateClientConfig(string cIP, Session oSession)
    {
        string postStr = Encoding.UTF8.GetString(oSession.RequestBody);
        bool ret = this.processClientConfig(postStr, cIP);
        if (ret) {
            oSession["x-replywithfile"] = "rdone.html";
        } else {
            oSession["x-replywithfile"] = "done.html";
            if (this.usrConfig.ContainsKey(cIP + "|remoteProxy")) {
                this.printJSLog(cIP + " remoteProxy " + this.usrConfig[cIP + "|remoteProxy"] + " removed");
                this.usrConfig.Remove(cIP + "|remoteProxy");
            }
        }
    }
    [CodeDescription("Deal With Request if client IP Configed")]
    private void tamperConfigedHost(string cIP, Session oSession)
    {
        string hostname = oSession.hostname;
        if (this.usrConfig.ContainsKey(cIP + "|" + hostname)) {
            if (this.usrConfig[cIP + "|" + hostname] == "" || this.usrConfig[cIP + "|" + hostname] == null || hostname == this._wifiIP || hostname == this._lanIP) {
                oSession.bypassGateway = false;
                oSession["x-overrideHost"] = null;
            } else {
                oSession.bypassGateway = true;
                oSession["x-overrideHost"] = this.usrConfig[cIP + "|" + hostname];
            }
        }
    }
    private void noBodyReponse(Session oSession, Int32 statusCode)
    {
        oSession.utilCreateResponseAndBypassServer();
        oSession.bypassGateway = true;
        oSession.responseCode = statusCode;
        oSession.oResponse.headers["Server"] = "SmartHost/1.1.0.3";
        oSession.oResponse.headers["Date"] = DateTime.Now.ToUniversalTime().ToString("r");
    }
    
    [CodeDescription("set response header and send body")]
    private void responseLogRequest(Session oSession, string body, string type, string cb)
    {
        if (cb.Length > 0) {
            body = "try{" + cb + "(" + body + ");}catch(e){}";
        }
        this.noBodyReponse(oSession, 200);
        oSession.oResponse.headers.HTTPResponseStatus = "OK";
        oSession.oResponse.headers["Content-Type"] = type;
        oSession.oResponse.headers["Content-Length"] = "" + body.Length;
        oSession.utilSetResponseBody(body);
    }
    private Dictionary<string, string> splitString(string strIn, char[] split1, char[] split2)
    {
        Dictionary<string, string> obj = new Dictionary<string, string>();
        string[] pairs1 = strIn.Split(split1);
        for (int i = 0, il = pairs1.Length; i < il; i++) {
            string[] pairs2 = pairs1[i].Split(split2);
            if (pairs2.Length == 2 && pairs2[0].Length > 0) {
                try{ pairs2[1] = Utilities.UrlDecode(pairs2[1]); }catch(Exception e){};
                if (obj.ContainsKey(pairs2[0])){
                    obj[pairs2[0]] = pairs2[1];
                }else{
                    obj.Add(pairs2[0], pairs2[1]);
                }
            }
        }
        return obj;
    }
    private string strItem(Session oSession)
    {
        string info = "", reqHead = "", reqBody = "", resHead = "", timer = "";
        if (oSession.state == SessionStates.Done) {
            reqHead = Regex.Replace(oSession.oRequest.headers.ToString(), "[\r\n]+", @"\n");
            reqHead = Regex.Replace(reqHead, "\"", @"\u0022");
            resHead = Regex.Replace(oSession.oResponse.headers.ToString(), "[\r\n]+", @"\n");
            resHead = Regex.Replace(resHead, "\"", @"\u0022");
            if (oSession.requestBodyBytes.Length > 0) {
                reqBody = oSession.GetRequestBodyAsString();
                reqBody = reqBody != "" ? Regex.Replace(reqBody, "[\r\n]+", @"\n") : "";
                reqBody = !String.IsNullOrEmpty(reqBody) ? Regex.Replace(reqBody, "\"", @"\u0022") : "";
            }
            HTTPHeaders oHeaders = oSession.oResponse.headers;
            string type = oHeaders["Content-Type"];
        }
        info += "{\"id\":" + oSession.id;
        info += ",\"status\":" + oSession.responseCode;
        info += ",\"clientIP\":\"" + (oSession.clientIP.Length > 0 ? oSession.clientIP : oSession.m_clientIP) + "\"";
        info += ",\"serverIP\":\"" + oSession.m_hostIP + "\"";
        info += ",\"host\":\"" + oSession.hostname + (oSession.port == 80 ? "" : ":" + oSession.port) + "\"";
        info += ",\"url\":\"" + System.Uri.EscapeUriString(oSession.PathAndQuery) + "\"";
        info += ",\"requestHeaders\":\"" + reqHead + "\"";
        info += ",\"responseHeaders\":\"" + resHead + "\"";
        info += ",\"requestBody\":\"" + reqBody + "\"";
        timer = Regex.Replace(oSession.Timers.ToString(), "[\r\n]+", @"\n");
        info += ",\"times\":\"" + timer + "\"";
        info += "}";
        return info;
    }
    [CodeDescription("Berfore Request Tamper.")]
    public void AutoTamperRequestBefore(Session oSession)
    {
        if (!this._tamperHost || oSession.isTunnel || oSession.isHTTPS || oSession.isFTP ) { return; }
        string cIP = !String.IsNullOrEmpty(oSession.m_clientIP) ? oSession.m_clientIP : oSession.clientIP;
        string hostname = oSession.hostname;
        string host = oSession.host.Split(new char[] { ':' })[0];
        bool isConfig = oSession.HostnameIs("smart.host") || oSession.HostnameIs("config.qq.com");
        if (!isConfig  && this.usrConfig.ContainsKey(cIP + "|remoteProxy") && this.usrConfig[cIP + "|remoteProxy"].Length > 10) {
            oSession.bypassGateway = true;
            oSession["x-overrideHost"] = this.usrConfig[cIP + "|remoteProxy"];
            oSession.oRequest.headers["clientIP"] = cIP;
            return;
        }
        if (this.usrConfig.ContainsKey(cIP + "|" + hostname)) {
            this.tamperConfigedHost(cIP, oSession);
            return;
        }
        if (isConfig&&oSession.HTTPMethodIs("GET")) {
            string pathName = oSession.PathAndQuery.Substring(1).Split(new char[]{'?'})[0];
            string replyFile = pathName == "" ? "form.html" : pathName.Replace('/', '\\');
            if (File.Exists(this._pluginDir + @"\Captures\Responses\" + replyFile)) {
                oSession["x-replywithfile"] = replyFile;
            } else {
                if (oSession.url.Contains("/ip/")) {
                    this.logAdapterAddress(oSession);
                } else {
                    this.noBodyReponse(oSession,404);
                }
            }
            return;
        }
        if (isConfig && oSession.HTTPMethodIs("POST")) {
            this.updateClientConfig(cIP, oSession);
            return;
        }
    }
    public void AutoTamperRequestAfter(Session oSession) { }
    public void AutoTamperResponseBefore(Session oSession) { }
    public void AutoTamperResponseAfter(Session oSession) {
        string host = oSession.host.Split(new char[] { ':' })[0];
        if (oSession.HostnameIs("smart.host") || oSession.HostnameIs("config.qq.com")) {
            oSession["ui-hide"] = "true";
        }
    }
    public void OnLoad() {
        FiddlerApplication.UI.mnuMain.MenuItems.Add(mnuSmartHost);
        FiddlerApplication.UI.lvSessions.AddBoundColumn("Client IP", 100, "x-clientIP");
        FiddlerApplication.UI.lvSessions.AddBoundColumn("Server IP", 110, "x-HostIP");
        FiddlerApplication.UI.lvSessions.SetColumnOrderAndWidth("Client IP", 2, -1);
        FiddlerApplication.UI.lvSessions.SetColumnOrderAndWidth("Host", 3, -1);
        FiddlerApplication.UI.lvSessions.SetColumnOrderAndWidth("Server IP", 4, -1);
    }
    public void OnBeforeUnload()
    {
        FiddlerApplication.Prefs.SetBoolPref("extensions.smarthost.enabled", this._tamperHost);
    }
    public void OnBeforeReturningError(Session oSession) {  }
}
