using System;
using System.Management;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Resources;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Microsoft.Win32;
using Fiddler;

[assembly: AssemblyTitle("SmartHost")]
[assembly: AssemblyDescription("A simple multiple host mapping extension for Fiddler2")]
[assembly: AssemblyCompany("Tencent .Ltd")]
[assembly: AssemblyCopyright("Copyright Mooringniu@Tencent 2013")]
[assembly: AssemblyProduct("SmartHost")]
[assembly: AssemblyTrademark("SmartHost")]
[assembly: AssemblyVersion("1.0.2.8")]
[assembly: AssemblyFileVersion("1.0.2.8")]


public class SmartHost : IAutoTamper
{
    private bool _tamperHost = false;
    private string _ipConfigServer = String.Empty;
    private string _scriptPath = String.Empty;
    private string _pluginBase = String.Empty;
    private string _wirelessIP = String.Empty;
    private string _lanIP = String.Empty;
    private int _oldProxyEnabled;
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
        getPrefs();
        initializeMenu();
        setPluginPath();
        setIPAddress();
        this.usrConfig = new Dictionary<string, string>();
    }
    private void getPrefs()
    {
        this._tamperHost = FiddlerApplication.Prefs.GetBoolPref("extensions.smarthost.enabled", false);
        this._ipConfigServer = FiddlerApplication.Prefs.GetStringPref("extensions.smarthost.ipServerUrl", "");
    }
    private void initializeMenu()
    {

        this.mnuSmartHostEnabled = new MenuItem();
        this.mnuSmartHostEnabled.Index = 0;
        this.mnuSmartHostEnabled.Text = "&Enabled";
        this.mnuSmartHostEnabled.Checked = this._tamperHost;
        this.mnuSmartHostEnabled.Click += new EventHandler(_smarthostEnabled_click);

        this.mnuSmartHostConfig = new MenuItem();
        this.mnuSmartHostConfig.Index = 1;
        this.mnuSmartHostConfig.Text = "&Config Hosts";
        this.mnuSmartHostConfig.Click += new EventHandler(_smarthostConfig_click);

        this.mnuSmartHostReadme = new MenuItem();
        this.mnuSmartHostReadme.Index = 2;
        this.mnuSmartHostReadme.Text = "&Readme";
        this.mnuSmartHostReadme.Click += new EventHandler(_smarthostReadme_click);

        this.mnuSplit = new MenuItem();
        this.mnuSplit.Index = 3;
        this.mnuSplit.Text = "-";
        this.mnuSplit.Checked = true;

        this.mnuSmartHostAbout = new MenuItem();
        this.mnuSmartHostAbout.Index = 4;
        this.mnuSmartHostAbout.Text = "&About";
        this.mnuSmartHostAbout.Click += new EventHandler(_smarthostAbout_click);

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
        string argPath = this._scriptPath + @"\hostEditor.hta";
        if (File.Exists(argPath))
        {
            Fiddler.Utilities.RunExecutable("mshta.exe", "\"" + argPath + "\"");
        }
        else
        {
            FiddlerApplication.Log.LogString("hostEditor.hta not found at the Scripts folder,"
                 + "Please Reinstall SmartHost Plugin.");
        }
    }

    [CodeDescription("Readme menuItem clicked Event Handler")]
    private void _smarthostReadme_click(object sender, EventArgs e)
    {
        string argPath = this._scriptPath + @"\Readme.txt";
        if (File.Exists(argPath))
        {
            Fiddler.Utilities.RunExecutable("notepad.exe", "\"" + argPath + "\"");
        }
        else
        {
            FiddlerApplication.Log.LogString("Readme.txt not found at the Scripts folder,"
                + "Please Reinstall SmartHost Plugin.");
        }
    }

    [CodeDescription("About menuItem clicked Event Handler")]
    private void _smarthostAbout_click(object sender, EventArgs e)
    {
        MessageBox.Show(
            "Smarthost For Fiddler\n--------------------------------------------------"
            + "\nA Remote IP/HOST Remaping Tool For Fiddler\nMaking Mobile Development More Easier.\n"
            + "\nCurrent Version: 1.0.2.8\n"
            + "\nAny Suggestion Concat mooringniu@gmail.com",
            "About SmartHost",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    [CodeDescription("print jslog to fiddler for mobile debuging")]
    private void printJSLog(string log)
    {
        FiddlerApplication.Log.LogString(log);
    }

    /*****************************PLUGIN INIT START************************************************/
    [CodeDescription("set WireLess & LanIP for future Use")]
    public void setIPAddress()
    {
        int cmdCount = 0;
        string iip = "", sip = "", info = "";
        ManagementClass MC = new ManagementClass("Win32_NetworkAdapterConfiguration");
        ManagementObjectCollection MOC = MC.GetInstances();
        foreach (ManagementObject MO in MOC)
        {
            if (cmdCount == 3) { break; }
            if ((bool)MO["IPEnabled"] == true && (bool)MO["DHCPEnabled"] == true)
            {
                string[] ips = (string[])MO["IPAddress"];
                if (ips.Length > 0)
                {
                    if (MO["Description"].ToString().Contains("Wireless"))
                    {
                        cmdCount += 1;
                        iip = ips[0].ToString();
                    }
                    else
                    {
                        cmdCount += 2;
                        sip = ips[0].ToString();
                    }
                }
            }
        }
        if (cmdCount != 3)
        {
            if (cmdCount == 1)
            {
                sip = iip;
            }
            else if (cmdCount == 2)
            {
                iip = sip;
            }
        }
        this._wirelessIP = iip;
        this._lanIP = sip;
        if (this._ipConfigServer.Length > 0)
        {
            sendIPConfig();
        }
        printJSLog("\nwirelessIP : " + iip + "\nlanIP:" + sip);
    }
    [CodeDescription("send IP Config for other programs")]
    public void sendIPConfig()
    {
        if (!this._ipConfigServer.StartsWith("http://")) { return; }
        WebClient client = new WebClient();
        client.Headers.Add("User-Agent", "Smarthost Fiddler Plugin Version 1.0.2.8");
        string concat = this._ipConfigServer.Contains("?") ? "&" : "?";
        try
        {
            client.DownloadString(this._ipConfigServer + concat + "wanip=" + this._wirelessIP + "&lanip=" + this._lanIP);
        }
        catch (Exception ex) { }
    }
    [CodeDescription("Return Wireless and Lan IP address")]
    private void getIPAddress(Session oSession)
    {
        ResponseLogRequest(
            oSession,
            "Wireless Proxy:" + this._wirelessIP + "\nLanIP Address:" + this._lanIP + "\n",
            "text/plan",
            ""
        );
    }
    [CodeDescription("set plugin path from registry")]
    private void setPluginPath()
    {
        string path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders";
        RegistryKey oReg = Registry.CurrentUser.OpenSubKey(path, RegistryKeyPermissionCheck.ReadSubTree);
        if (oReg != null)
        {
            string docPath = (string)oReg.GetValue("Personal");
            this._pluginBase = docPath + @"\Fiddler2\";
            this._scriptPath = this._pluginBase + @"Scripts\Smarthost\";
            oReg.Close();
        }
        else
        {
            this._tamperHost = false;
            FiddlerApplication.Log.LogString("Can't find User Domuments Folder");
        }
    }
    /*****************************PLUGIN INIT END**************************************************/

    /*******************************IP/HOST REMAP CONFIG SETTING START*****************************/
    [CodeDescription("parse client post message")]
    private void parseAndSavePOST(string postStr, string cIP)
    {
        postStr = Regex.Replace(postStr, "\\s+", "");
        string[] pairs = Regex.Split(postStr, "&+");
        string[] hostIP = new string[2];
        char[] spliter = new char[] { '=' };
        for (int i = 0, il = pairs.Length; i < il; i++)
        {
            hostIP = pairs[i].Split(spliter);
            if (hostIP[0].Length == 0)
            {
                continue;
            }
            if (hostIP[1].Length == 0 && this.usrConfig.ContainsKey(cIP + "|" + hostIP[0]))
            {
                this.usrConfig.Remove(cIP + "|" + hostIP[0]);
            }
            else
            {
                this.usrConfig[cIP + "|" + hostIP[0]] = hostIP[1];
            }
        }
    }
    [CodeDescription("save client configuration to userConfig")]
    private void saveConfig(string cIP, Session oSession)
    {
        string postStr = Encoding.UTF8.GetString(oSession.RequestBody);
        parseAndSavePOST(postStr, cIP);
        oSession["x-replywithfile"] = "done.html";
    }
    /*******************************IP/HOST REMAP CONFIG SETTING END*******************************/

    /*******************************IP/HOST REMAP PROCESSING LOGIC START***************************/
    [CodeDescription("Deal With Request if client IP Configed")]
    private void upRequestHost(string cIP, Session oSession)
    {
        string hostname = oSession.hostname;
        if (this.usrConfig.ContainsKey(cIP + "|" + hostname))
        {
            if (this.usrConfig[cIP + "|" + hostname] == ""
                || this.usrConfig[cIP + "|" + hostname] == null
                || hostname == this._wirelessIP
                || hostname == this._lanIP)
            {
                oSession.bypassGateway = false;
                oSession["x-overrideHost"] = null;
            }
            else
            {
                oSession.bypassGateway = true;
                oSession["x-overrideHost"] = this.usrConfig[cIP + "|" + hostname];
            }
        }
    }
    /*******************************IP/HOST REMAP PROCESSING LOGIC END*****************************/

    /*******************************REMOTE LOG PROCESSING LOGIC START******************************/
    [CodeDescription("set response header and send body")]
    private void ResponseLogRequest(Session oSession, string body, string type, string cb)
    {
        if (cb.Length > 0)
        {
            body = "try{" + cb + "(" + body + ");}catch(e){}";
        }
        oSession.utilCreateResponseAndBypassServer();
        oSession.bypassGateway = true;
        oSession.oResponse.headers.HTTPResponseCode = 200;
        oSession.oResponse.headers.HTTPResponseStatus = "OK";
        oSession.oResponse.headers["Server"] = "SmartHost/1.0.2.8";
        oSession.oResponse.headers["Date"] = DateTime.Now.ToUniversalTime().ToString("r");
        oSession.oResponse.headers["Content-Type"] = type;
        oSession.oResponse.headers["Content-Length"] = "" + body.Length;
        oSession.utilSetResponseBody(body);
    }
    [CodeDescription("download sessions as saz for remote use")]
    private void downloadLogRequest(Session oSession)
    {
    }
    [CodeDescription("process Remote Log list Processing")]
    private void processLogRequest(Session oSession)
    {
        string[] query = oSession.PathAndQuery.Split(new char[] { '?', '&' });
        string[] pairs = new string[2];
        Session[] lists;
        bool returnBody = false, download = false;
        string destIP = "", callback = "";
        Int32 id = 0, idx = 1, pageSize = 100;
        for (int i = 0, il = query.Length; i < il; i++)
        {
            pairs = query[i].Split(new char[] { '=' });
            if (pairs.Length == 2)
            {
                if (pairs[0] == "ip")
                {
                    destIP = pairs[1];
                }
                else if (pairs[0] == "callback")
                {
                    callback = pairs[1];
                }
                else if (pairs[0] == "id")
                {
                    id = Convert.ToInt32(pairs[1]);
                }
                else if (pairs[0] == "size")
                {
                    pageSize = Convert.ToInt32(pairs[1]);
                }
                else if (pairs[0] == "body")
                {
                    returnBody = pairs[1].Length > 0 && pairs[1] != "0";
                }
                else if (pairs[0] == "download")
                {
                    download = pairs[1].Length > 0 && pairs[1] != "0";
                }
            }
        }
        destIP = Regex.Replace(destIP, "[^\\d\\.]+", "");
        callback = Regex.Replace(callback, "[^\\w\\d_\\$\\.]+", "");
        if (destIP.Length > 0)
        {
            Session[] sLists = FiddlerApplication.UI.GetAllSessions();
            string body = "[";
            for (int i = 0, il = sLists.Length; i < il; i++)
            {
                if (idx > pageSize) { break; }
                if (sLists[i].id < id
                    || sLists[i].isFlagSet(SessionFlags.ResponseGeneratedByFiddler)) { continue; }
                if (sLists[i].m_clientIP == destIP || sLists[i].clientIP == destIP)
                {
                    if (sLists[i].responseCode >= 100)
                    {
                        body += (idx != 1 ? "," : "") + strItem(sLists[i], returnBody);
                        idx++;
                    }
                }
            }
            body += "]";
            ResponseLogRequest(oSession, body, "application/javascript", callback);
        }
        else
        {
            ResponseLogRequest(
                oSession
                , "Log Helper:\n\nexmaple url: \n"
                 + "http://" + this._lanIP + ":" + CONFIG.ListenPort + "/log/?ip=127.0.0.1&size=10&callback=console.log\n"
                 + "support params: ip, id, size, body, callback, download\n"
                 + "\nMore Help Concat mooringniu"
                , "text/plan; charset=utf-8", "");
        }
    }
    private string strItem(Session oSession, bool returnBody)
    {
        string info = "", reqHead = "", reqBody = "", resHead = "", resBody = "", timer = "";
        if (oSession.state == SessionStates.Done)
        {
            reqHead = Regex.Replace(oSession.oRequest.headers.ToString(), "[\r\n]+", @"\n");
            reqHead = Regex.Replace(reqHead, "\"", @"\u0022");
            resHead = Regex.Replace(oSession.oResponse.headers.ToString(), "[\r\n]+", @"\n");
            resHead = Regex.Replace(resHead, "\"", @"\u0022");

            if (!Utilities.IsNullOrEmpty(oSession.requestBodyBytes))
            {
                reqBody = oSession.GetRequestBodyAsString();
                reqBody = reqBody != "" ? Regex.Replace(reqBody, "[\r\n]+", @"\n") : "";
                reqBody = reqBody.Length > 0 ? Regex.Replace(reqBody, "\"", @"\u0022") : "";
            }
            HTTPHeaders oHeaders = oSession.oResponse.headers;
            string type = oHeaders["Content-Type"];
            if (returnBody && !Utilities.IsNullOrEmpty(oSession.responseBodyBytes)
                && (type.Contains("text") || type.Contains("javascript")
                     || type.Contains("json") || type.Contains("charset"))
            )
            {
                if (oHeaders.Exists("Transfer-Encoding") || oHeaders.Exists("Content-Encoding"))
                {
                    byte[] value = oSession.responseBodyBytes;
                    byte[] arrCopy = (byte[])value.Clone();
                    Utilities.utilDecodeHTTPBody(oHeaders, ref arrCopy);
                    value = arrCopy;
                    Encoding oEncoding = Utilities.getEntityBodyEncoding(oHeaders, value);
                    resBody = Utilities.GetStringFromArrayRemovingBOM(value, oEncoding);
                }
                else
                {
                    resBody = oSession.GetResponseBodyAsString();
                }
                resBody = resBody.Length > 0 ? Regex.Replace(resBody, "\"", @"\u0022") : "";
                resBody = resBody != "" ? Regex.Replace(resBody, "[\r\n]+", @"\n") : "";
            }
        }
        info += "{id:" + oSession.id + ",method:\"" + oSession.RequestMethod + "\"";
        info += ",status:" + oSession.responseCode;
        info += ",clientIP:\"" + (oSession.clientIP.Length > 0 ? oSession.clientIP : oSession.m_clientIP) + "\"";
        info += ",serverIP:\"" + oSession.m_hostIP + "\"";
        info += ",Host:\"" + oSession.hostname + (oSession.port == 80 ? "" : ":" + oSession.port) + "\"";
        info += ",url:\"" + System.Uri.EscapeUriString(oSession.PathAndQuery) + "\"";
        info += ",requestHeaders:\"" + reqHead + "\"";
        info += ",responseHeaders:\"" + resHead + "\"";
        info += ",requestBody:\"" + reqBody + "\"";
        if (returnBody)
        {
            info += ",responseBody:\"" + resBody + "\"";
        }
        timer = Regex.Replace(oSession.Timers.ToString(), "[\r\n]+", @"\n");
        info += ",times:\"" + timer + "\"";
        info += "}";
        return info;
    }
    /*******************************REMOTE LOG PROCESSING LOGIC END********************************/

    /******************************REQUEST PROCESSING ENTRY****************************************/
    [CodeDescription("Berfore Request Tamper.")]
    public void AutoTamperRequestBefore(Session oSession)
    {
        //如果没有激活，自动退出
        if (!this._tamperHost) { return; }
        string cIP = (oSession.m_clientIP != null && oSession.m_clientIP.Length > 0) ? oSession.m_clientIP : oSession.clientIP;
        string hostname = oSession.hostname;
        string host = oSession.host.Split(new char[] { ':' })[0];
        //printJSLog(host+"=="+this._lanIP+"==="+this._wirelessIP);
        //设置IP/HOST映射关系
        if (this.usrConfig.ContainsKey(cIP + "|" + hostname))
        {
            upRequestHost(cIP, oSession);
        }
        else if (oSession.HostnameIs("smart.host") || oSession.HostnameIs("config.qq.com")
         || (CONFIG.ListenPort == oSession.port
                && (host == this._lanIP || host == this._wirelessIP || host.Contains("127.0.0.")))
        )
        {
            if (oSession.HTTPMethodIs("GET"))
            {
                string replyFile = oSession.PathAndQuery.Substring(1).Split(new char[] { '?', '#' })[0].Replace('/', '\\');
                replyFile = replyFile.Length == 0 ? "form.html" : replyFile;
                //如果文件存在
                if (File.Exists(this._pluginBase + @"\Captures\Responses\" + replyFile))
                {
                    oSession["x-replywithfile"] = replyFile;
                }
                else
                {
                    if (oSession.url.Contains("/log/"))
                    {
                        processLogRequest(oSession);
                    }
                    else if (oSession.url.Contains("/ip/"))
                    {
                        getIPAddress(oSession);
                    }
                    else
                    {
                        oSession["x-replywithfile"] = "form.html";
                    }
                }
            }
            //处理配置保存信息
            else if (oSession.HTTPMethodIs("POST"))
            {
                saveConfig(cIP, oSession);
            }
        }
    }
    public void AutoTamperRequestAfter(Session oSession) { }
    public void AutoTamperResponseBefore(Session oSession) { }
    public void AutoTamperResponseAfter(Session oSession)
    {
        string host = oSession.host.Split(new char[] { ':' })[0];
        if (
            oSession.HostnameIs("smart.host") || oSession.HostnameIs("config.qq.com")
            || (
                oSession.port == CONFIG.ListenPort
                && (host == this._lanIP || host == this._wirelessIP || host.Contains("127.0.0."))
            )
        )
        {
            //oSession["ui-hide"] = "true";
        }
    }
    public void OnLoad()
    {
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
    public void OnBeforeReturningError(Session oSession) { }
}