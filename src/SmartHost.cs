/*
 * Copyright by mooringniu@gmail.com
 * Any Suggestion Email ME
 */
using System;
using System.Management;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Resources;
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
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
[assembly: Fiddler.RequiredVersion("2.4.1.1")]

public class SmartHost : IAutoTamper
{
    private bool _tamperHost = false;
    private string _notifySrv = String.Empty;
    private string _scriptDir = String.Empty;
    private string _pluginDir = String.Empty;
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
        this.initConfig();
        this.initializeMenu();
        this.getPluginPath();
        this.usrConfig = new Dictionary<string, string>();
    }
    private void initConfig()
    {
        this._tamperHost = FiddlerApplication.Prefs.GetBoolPref("extensions.smarthost.enabled", false);
        this._notifySrv  = FiddlerApplication.Prefs.GetStringPref("extensions.smarthost.setip", "");
        if (this._notifySrv.Length > 0 && this._notifySrv.StartsWith("http:"))
        {
            this.reportAdapterAddress();
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(this.networdAddressChangeHandler);
        }
        else
        {
            this.getAdapterAddress();
        }

    }
    private void networdAddressChangeHandler(object sender, EventArgs e)
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
        if (File.Exists(argPath))
        {
            Fiddler.Utilities.RunExecutable("mshta.exe", "\"" + argPath + "\"");
        }
        else
        {
            FiddlerApplication.Log.LogString("hostEditor.hta not found at the Scripts folder,Please Reinstall SmartHost Plugin.");
        }
    }

    [CodeDescription("Readme menuItem clicked Event Handler")]
    private void _smarthostReadme_click(object sender, EventArgs e)
    {
        string argPath = this._scriptDir + @"\Readme.txt";
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
            + "\nA Remote IP/HOST Remaping Tool For Fiddler"
            + "\nMaking Mobile Development More Easier.\n"
            + "\nFileVersion: 1.1.0.0\n"
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
    public void getAdapterAddress()
    {
        int ipCnt = 0;
        string iip = "", sip = "", info = "";
        ManagementClass MC = new ManagementClass("Win32_NetworkAdapterConfiguration");
        ManagementObjectCollection MOC = MC.GetInstances();
        foreach (ManagementObject MO in MOC)
        {
            if (ipCnt == 3) { break; }
            if ((bool)MO["IPEnabled"] == true && (bool)MO["DHCPEnabled"] == true)
            {
                string[] ips = (string[])MO["IPAddress"];
                if (ips.Length > 0)
                {
                    if (MO["Description"].ToString().Contains("Wireless"))
                    {
                        ipCnt += 1;
                        iip = ips[0].ToString();
                    }
                    else
                    {
                        ipCnt += 2;
                        sip = ips[0].ToString();
                    }
                }
            }
        }
        if (ipCnt != 3)
        {
            if (ipCnt == 1)
            {
                sip = iip;
            }
            else if (ipCnt == 2)
            {
                iip = sip;
            }
        }
        this._wirelessIP = iip;
        this._lanIP = sip;
        if (this._notifySrv.StartsWith("http://"))
        {
            try { this.reportAdapterAddress(); }
            catch (Exception e) { }
        }
        this.printJSLog("\nNetwork Adapter Addresses : " + iip + (iip != sip ? "\t\t" + sip: ""));
    }

    [CodeDescription("send IP Config for other programs")]
    public void reportAdapterAddress()
    {
        this.getAdapterAddress();
        if (!this._notifySrv.StartsWith("http://")) { return; }
        string concat = this._notifySrv.Contains("?") ? "&" : "?";
        string url = this._notifySrv + concat + "wanip=" + this._wirelessIP + "&lanip=" + this._lanIP;
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        if (CONFIG.sGatewayUsername != null && CONFIG.sGatewayPassword != null)
        {
            httpWebRequest.Proxy.Credentials = (ICredentials)new NetworkCredential(CONFIG.sGatewayUsername, CONFIG.sGatewayPassword);
        }
        else
        {
            httpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        httpWebRequest.UserAgent = "SmartHost/1.1.0.0";
        httpWebRequest.Referer = "http://config.qq.com";
        httpWebRequest.KeepAlive = false;
        HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        if (httpWebResponse.StatusCode == HttpStatusCode.OK)
        {
            this.printJSLog(url);
        }
        httpWebResponse.Close();
    }

    [CodeDescription("Return Wireless and Lan IP address")]
    private void logAdapterAddress(Session oSession)
    {
        this.responseLogRequest(
            oSession,
            "Wireless Proxy:" + this._wirelessIP + "\nLanIP Address:" + this._lanIP + "\n",
            "text/plain",
            ""
        );
    }

    [CodeDescription("set plugin path from registry")]
    private void getPluginPath()
    {
        string path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders";
        RegistryKey oReg = Registry.CurrentUser.OpenSubKey(path, RegistryKeyPermissionCheck.ReadSubTree);
        if (oReg != null)
        {
            string docPath = (string)oReg.GetValue("Personal");
            this._pluginDir = docPath + @"\Fiddler2\";
            this._scriptDir = this._pluginDir + @"Scripts\Smarthost\";
            oReg.Close();
        }
        else
        {
            this._tamperHost = false;
            this.printJSLog("Can't find User Domuments Folder");
        }
    }
    /*****************************PLUGIN INIT END**************************************************/

    /*******************************IP/HOST REMAP CONFIG SETTING START*****************************/
    [CodeDescription("parse client post message")]
    private bool processClientConfig(string postStr, string cIP)
    {
        Dictionary<string, string> pQuery = this.splitString(postStr, new char[] { '&' }, new char[] { '=' });
        bool isRemote = false;
        foreach (string key in pQuery.Keys)
        {
            if (key == "remoteProxy")
            {
                if (pQuery[key].Length == 0)
                {
                    this.usrConfig.Remove(cIP + "|remoteProxy");
                }
                else
                {
                    isRemote = true;
                    pQuery[key] = pQuery[key].Replace("%3A", ":");
                    this.printJSLog(cIP + " remoteProxy Setting To: " + pQuery[key]);
                    this.usrConfig[cIP + "|remoteProxy"] = pQuery[key];
                }
                continue;
            }
            else if (key == "oid" && pQuery[key].Length > 0 && pQuery[key] != "Smarthost_")
            {
                this.saveConfig2File(postStr, pQuery[key]);
                continue;
            }
            else
            {
                if (pQuery[key].Length == 0)
                {
                    this.usrConfig.Remove(cIP + "|" + pQuery[key]);
                }
                else
                {
                    this.usrConfig[cIP + "|" + key] = pQuery[key];
                }
            }
        }
        return isRemote;
    }
    [CodeDescription("save Config File To String")]
    private void saveConfig2File(string postStr, string oid)
    {
        oid = Regex.Replace(oid, "[^a-z0-9_\\-\\.]+", "");
        string file = this._pluginDir + @"\Captures\Responses\Configs\" + oid + ".txt";
        System.IO.File.WriteAllText(file, postStr);
    }

    [CodeDescription("save client configuration to userConfig")]
    private void saveClientConfig(string cIP, Session oSession)
    {
        string postStr = Encoding.UTF8.GetString(oSession.RequestBody);
        bool ret = this.processClientConfig(postStr, cIP);
        if (ret)
        {
            oSession["x-replywithfile"] = "rdone.html";
        }
        else
        {
            oSession["x-replywithfile"] = "done.html";
            if (this.usrConfig.ContainsKey(cIP + "|remoteProxy"))
            {
                this.printJSLog(cIP + " remoteProxy " + this.usrConfig[cIP + "|remoteProxy"] + " removed ");
                this.usrConfig.Remove(cIP + "|remoteProxy");
            }
        }
    }

    /*******************************IP/HOST REMAP CONFIG SETTING END*******************************/

    /*******************************IP/HOST REMAP PROCESSING LOGIC START***************************/
    [CodeDescription("Deal With Request if client IP Configed")]
    private void tamperConfigedHost(string cIP, Session oSession)
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
    private void noBodyReponse(Session oSession, Int32 statusCode)
    {
        oSession.utilCreateResponseAndBypassServer();
        oSession.bypassGateway = true;
        oSession.responseCode = statusCode;
        oSession.oResponse.headers["Server"] = "SmartHost/1.1.0.0";
        oSession.oResponse.headers["Date"] = DateTime.Now.ToUniversalTime().ToString("r");

    }
    /*******************************IP/HOST REMAP PROCESSING LOGIC END*****************************/

    /*******************************REMOTE LOG PROCESSING LOGIC START******************************/
    [CodeDescription("process Remote Log list Processing")]
    private void resopnseLogRequest(Session oSession)
    {
        Dictionary<string, string> gQuery = this.splitString(oSession.PathAndQuery.Substring(1), new char[] { '&','?'}, new char[] {'='});
        Session[] sLists = FiddlerApplication.UI.GetAllSessions();
        string destIP = "", callback = "";
        Int32 id = 0, pageSize = 100;
        if (gQuery.ContainsKey("ip"))
        {
            destIP = Regex.Replace(gQuery["ip"], "[^\\d\\.]+", "");
        }
        if (gQuery.ContainsKey("callback"))
        {
            callback = Regex.Replace(gQuery["callback"], "[^\\w\\d_\\$\\.]+", "");
        }
        if (gQuery.ContainsKey("size"))
        {
            pageSize = Convert.ToInt32(gQuery["size"]);
        }
        if (gQuery.ContainsKey("id"))
        {
            id = Convert.ToInt32(gQuery["id"]);
        }
        pageSize = pageSize < 1 ? 100 : pageSize;
        id = id < 1 ? 1 : id;
        if (gQuery.ContainsKey("saz") && gQuery["saz"].Length > 0)
        {
            this.downloadLogRequest(oSession, sLists, destIP, pageSize);
            return;
        }
        if (destIP.Length > 0)
        {
            int listLength = sLists.Length; ;
            string body = "[";
            for (int i = 0, j = 0; j < pageSize && i < listLength; i++)
            {
                if (sLists[i].id < id
                    || sLists[i].state < SessionStates.Done
                    || sLists[i].isFlagSet(SessionFlags.ResponseGeneratedByFiddler)
                )
                {
                    continue;
                }
                if (sLists[i].m_clientIP == destIP || sLists[i].clientIP == destIP)
                {
                    body += (j > 0 ? "," : "") + strItem(sLists[i]);
                    j++;
                }
            }
            body += "]";
            this.responseLogRequest(oSession, body, "application/javascript", callback);
        }
        else
        {
            oSession.bypassGateway = true;
            oSession["x-replywithfile"] = "help.txt";
        }
    }
    [CodeDescription("set response header and send body")]
    private void responseLogRequest(Session oSession, string body, string type, string cb)
    {
        if (cb.Length > 0)
        {
            body = "try{" + cb + "(" + body + ");}catch(e){}";
        }
        this.noBodyReponse(oSession, 200);
        oSession.oResponse.headers.HTTPResponseStatus = "OK";
        oSession.oResponse.headers["Content-Type"] = type;
        oSession.oResponse.headers["Content-Length"] = "" + body.Length;
        oSession.utilSetResponseBody(body);
    }
    [CodeDescription("download sessions as saz for remote use")]
    private void downloadLogRequest(Session oSession, Session[] sLists, string destIP, Int32 pageSize)
    {
        if (sLists.Length < 1)
        {
            responseLogRequest(oSession, "No Log for Your Request", "text/plain", "");
            return;
        }
        int Count = 0;
        for (int i = 0, il = sLists.Length; i < il; i++)
        {
            if (sLists[i].oFlags["x-clientIP"] == destIP)
            {
                Count++;
            }
        }
        Session[] dLists = new Session[Count];
        for (int i = 0, j = 0, il = sLists.Length; i < il; i++)
        {
            if (j < Count && sLists[i].oFlags["x-clientIP"] == destIP)
            {
                dLists[j++] = sLists[i];
            }
        }
        string name = oSession.clientIP.Length > 0 ? oSession.clientIP : oSession.m_clientIP;
        string file = this._pluginDir + @"\Captures\Responses\Packages\" + name + ".saz";
        bool ret = false;
        try { ret = Utilities.WriteSessionArchive(file, dLists, "", true); }
        catch (Exception e) { }
        if (ret)
        {
            this.noBodyReponse(oSession,200);
            oSession["x-replywithfile"] = @"Packages\"+ name + ".saz";
            //oSession.oResponse.headers["Content-Type"] = "application/x-download";
            //oSession.oResponse.headers["Content-Disposition"] = "attachment; filename="+name+".saz";
        }
        else
        {
            this.responseLogRequest(oSession, "Not Ready", "text/plain", "");
        }
    }
    private Dictionary<string, string> splitString(string strIn, char[] split1, char[] split2)
    {
        Dictionary<string, string> obj = new Dictionary<string, string>();
        string[] pairs1 = strIn.Split(split1);
        for (int i = 0, il = pairs1.Length; i < il; i++)
        {
            string[] pairs2 = pairs1[i].Split(split2);
            if (pairs2.Length == 2 && pairs2[0].Length > 0)
            {
                if (obj.ContainsKey(pairs2[0]))
                {
                    obj[pairs2[0]] = pairs2[1];
                }
                else
                {
                    obj.Add(pairs2[0], pairs2[1]);
                }
            }
        }
        return obj;
    }
    private string strItem(Session oSession)
    {
        string info = "", reqHead = "", reqBody = "", resHead = "", timer = "";
        if (oSession.state == SessionStates.Done)
        {
            reqHead = Regex.Replace(oSession.oRequest.headers.ToString(), "[\r\n]+", @"\n");
            reqHead = Regex.Replace(reqHead, "\"", @"\u0022");
            resHead = Regex.Replace(oSession.oResponse.headers.ToString(), "[\r\n]+", @"\n");
            resHead = Regex.Replace(resHead, "\"", @"\u0022");

            if (oSession.requestBodyBytes.Length > 0)
            {
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
    /*******************************REMOTE LOG PROCESSING LOGIC END********************************/

    /******************************REQUEST PROCESSING ENTRY****************************************/
    [CodeDescription("Berfore Request Tamper.")]
    public void AutoTamperRequestBefore(Session oSession)
    {
        //如果没有激活，自动退出
        if (!this._tamperHost) { return; }
        string cIP = !String.IsNullOrEmpty(oSession.m_clientIP) ? oSession.m_clientIP : oSession.clientIP;
        string hostname = oSession.hostname;
        string host = oSession.host.Split(new char[] { ':' })[0];
        bool isConfig = oSession.HostnameIs("smart.host") || oSession.HostnameIs("config.qq.com");
        if (!isConfig && this.usrConfig.ContainsKey(cIP + "|remoteProxy") && this.usrConfig[cIP + "|remoteProxy"].Length > 10)
        {
            oSession.bypassGateway = true;
            oSession["x-overrideHost"] = this.usrConfig[cIP + "|remoteProxy"];
            oSession.oRequest.headers["clientIP"] = cIP;
            return;
        }
        if (this.usrConfig.ContainsKey(cIP + "|" + hostname))
        {
            this.tamperConfigedHost(cIP, oSession);
            return;
        }
        if (isConfig&&oSession.HTTPMethodIs("GET"))
        {
            string pathName = oSession.PathAndQuery.Substring(1).Split(new char[]{'?'})[0];
            string replyFile = pathName == "" ? "form.html" : pathName.Replace('/', '\\');
            //如果文件存在
            if (File.Exists(this._pluginDir + @"\Captures\Responses\" + replyFile))
            {
                oSession["x-replywithfile"] = replyFile;
            }
            else
            {
                if (oSession.url.Contains("/log/"))
                {
                    this.resopnseLogRequest(oSession);
                }
                else if (oSession.url.Contains("/ip/"))
                {
                    this.logAdapterAddress(oSession);
                }
                else
                {
                    this.noBodyReponse(oSession,404);
                }
            }
            return;
        }
        if (isConfig && oSession.HTTPMethodIs("POST"))
        {
            this.saveClientConfig(cIP, oSession);
            return;
        }
        if (oSession.oRequest.headers["User-Agent"].Contains("Mac OS X") && (oSession.HostnameIs("www.airport.us") || oSession.HostnameIs("www.thinkdifferent.us") || oSession.HostnameIs("www.itools.info")))
        {
            this.responseLogRequest(oSession, "<HTML><HEAD><TITLE>Success</TITLE></HEAD><BODY>Success</BODY></HTML>", "text/html", "");
        }
    }
    public void AutoTamperRequestAfter(Session oSession) { }
    public void AutoTamperResponseBefore(Session oSession) { }
    public void AutoTamperResponseAfter(Session oSession)
    {
        string host = oSession.host.Split(new char[] { ':' })[0];
        if (oSession.HostnameIs("smart.host") || oSession.HostnameIs("config.qq.com"))
        {
            oSession["ui-hide"] = "true";
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
