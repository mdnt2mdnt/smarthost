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
[assembly: Fiddler.RequiredVersion("2.4.1.1")]


public class SmartHost : IAutoTamper {
    private bool   _tamperHost     = false;
    private string _scriptPath     = String.Empty;
    private string _pluginBase     = String.Empty;
    private string _wirelessIP     = String.Empty;
    private string _lanIP          = String.Empty;
    private int    _oldProxyEnabled;
    private Dictionary<string,string> usrConfig;
    private MenuItem mnuSmartHost;
    private MenuItem mnuSmartHostEnabled;
    private MenuItem mnuSmartHostConfig;
    private MenuItem mnuSmartHostReadme;
    private MenuItem mnuSmartHostAbout;
    private MenuItem mnuSplit;
    private MenuItem mnuSplit1;
    
    public SmartHost(){
        getPrefs();
        initializeMenu();
        setPluginPath();
        setIPAddress();
        this.usrConfig= new Dictionary<string,string>();
    }
    private void getPrefs(){
        this._tamperHost = FiddlerApplication.Prefs.GetBoolPref("extensions.smarthost.enabled",false);
    }
    
    private void initializeMenu(){
        
        this.mnuSmartHostEnabled = new MenuItem();
        this.mnuSmartHostEnabled.Index = 0;
        this.mnuSmartHostEnabled.Text = "&Enabled";
        this.mnuSmartHostEnabled.Checked = this._tamperHost;
        this.mnuSmartHostEnabled.Click += new EventHandler(_smarthostEnabled_click);
        
        this.mnuSmartHostConfig = new MenuItem();
        this.mnuSmartHostConfig.Index = 1;
        this.mnuSmartHostConfig.Text = "&Config Hosts";
        this.mnuSmartHostConfig.Click += new EventHandler(_smarthostConfig_click);
        
        this.mnuSmartHostReadme   = new MenuItem();
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
    private void _smarthostEnabled_click(object sender, EventArgs e){
        MenuItem oSender = (sender as MenuItem);
        oSender.Checked = !oSender.Checked;
        this._tamperHost = oSender.Checked;
    }
    
    [CodeDescription("Config MenuItem clicked Event Handler")]
    private void _smarthostConfig_click(object sender, EventArgs e){
        string argPath = this._scriptPath+@"\hostEditor.hta";
        if(File.Exists(argPath)){
            Fiddler.Utilities.RunExecutable("mshta.exe", "\""+argPath+"\"");
        }else{
           FiddlerApplication.Log.LogString("hostEditor.hta not found at the Scripts folder, Please Reinstall SmartHost Plugin.");
        }
    }
    
    [CodeDescription("Readme menuItem clicked Event Handler")]
    private void _smarthostReadme_click(object sender, EventArgs e){
        string argPath = this._scriptPath+@"\Readme.txt";
        if(File.Exists(argPath)){
            Fiddler.Utilities.RunExecutable("notepad.exe", "\""+argPath+"\"");
        }else{
            FiddlerApplication.Log.LogString("Readme.txt not found at the Scripts folder, Please Reinstall SmartHost Plugin.");
        }
    }

    [CodeDescription("About menuItem clicked Event Handler")]
    private void _smarthostAbout_click(object sender, EventArgs e){
        MessageBox.Show(
            "Smarthost For Fiddler\n-----------------------------------------------------------------"+
            "\nA Remote IP/HOST Remaping Tool For Fiddler Making Mobile Development More Easier.\n"+
            "\nCurrent Version: 1.0.2.8",
            "About SmartHost",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    [CodeDescription("print jslog to fiddler for mobile debuging")]
    private void printJSLog(string log){
        FiddlerApplication.Log.LogString(log);
    }
    
    [CodeDescription("set WireLess & LanIP for future Use")]
    public void setIPAddress(){
        int cmdCount = 0;
        string iip = "", sip = "" , info = "";
        ManagementClass MC = new ManagementClass("Win32_NetworkAdapterConfiguration");
        ManagementObjectCollection MOC = MC.GetInstances();
        foreach (ManagementObject MO in MOC)
        {
            if(cmdCount==3){ break; }
            if ((bool)MO["IPEnabled"] == true && (bool) MO["DHCPEnabled"] == true )
            {
                string[] ips = (string[])MO["IPAddress"];
                if(ips.Length>0){
                    if(MO["Description"].ToString().Contains("Wireless")){
                        cmdCount += 1;
                        iip = ips[0].ToString();
                        //sendIPCommand("iip",iip);
                    }else{
                        cmdCount += 2;
                        sip = ips[0].ToString();
                        //sendIPCommand("sip",sip);
                    }
                }
            }
        }
        if(cmdCount!=3){
            if(cmdCount == 1){
                sip = iip;
                //sendIPCommand("sip",iip);
            }else if(cmdCount==2){
                iip = sip;
                //sendIPCommand("iip",sip);
            }
        }
        this._wirelessIP = iip;
        this._lanIP = sip;
        printJSLog("wirelessIP:"+iip+" lanIP:"+sip);
    }
    
    [CodeDescription("set plugin path from registry")]
    private void setPluginPath(){
        string path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders";
        RegistryKey oReg = Registry.CurrentUser.OpenSubKey(path,RegistryKeyPermissionCheck.ReadSubTree);
        if( oReg != null ){
            string docPath = (string) oReg.GetValue("Personal");
            this._pluginBase = docPath + @"\Fiddler2\";
            this._scriptPath = this._pluginBase + @"Scripts\Smarthost\";
            oReg.Close();
        }else{
            this._tamperHost = false;
            FiddlerApplication.Log.LogString("Can't find User Domuments Folder");
        }
    }
    [CodeDescription("parse client post message")]
    private void parseAndSaveConfig(string postStr,string cIP){
        postStr = Regex.Replace(postStr,"\\s+","");
        string[] pairs = Regex.Split(postStr,"&+");
        string[] hostIP = new string[2];
        Char[] spliter = new Char[]{'='};
        for(int i=0,il=pairs.Length;i<il;i++){
            hostIP = pairs[i].Split(spliter);
            if(hostIP[0].Length==0){
                continue;
            }
            if(hostIP[1].Length==0 && this.usrConfig.ContainsKey(cIP+"|"+hostIP[0])){
                this.usrConfig.Remove(cIP+"|"+hostIP[0]);
            }else{
                this.usrConfig[cIP+"|"+hostIP[0]] = hostIP[1];
            }
        }
    }
    [CodeDescription("save client configuration to userConfig")]
    private void saveConfig(string cIP, Session oSession){
        string postStr = Encoding.UTF8.GetString(oSession.RequestBody);
        parseAndSaveConfig(postStr,cIP);
        oSession["x-replywithfile"] = "done.html";
    }
    [CodeDescription("Deal With Request if client IP Configed")]
    private void upRequestHost(string cIP,Session oSession){
        string hostname = oSession.hostname;
        printJSLog(hostname+"==>"+this._wirelessIP+"===>"+this._lanIP);
        if( this.usrConfig.ContainsKey(cIP+"|"+hostname) ){
            if( this.usrConfig[cIP+"|"+hostname] == "" 
                || this.usrConfig[cIP+"|"+hostname] == null 
                || hostname == this._wirelessIP 
                || hostname == this._lanIP )
            {
                oSession.bypassGateway = false;
                oSession["x-overrideHost"] = null;
            }else{
                oSession.bypassGateway = true;
                oSession["x-overrideHost"] = this.usrConfig[cIP+"|"+hostname];
            }
        }
    }
    
    private void ResponseLogRequest(Session oSession, string body){
        oSession.utilCreateResponseAndBypassServer();
        oSession.bypassGateway = true;
        oSession.oResponse.headers.HTTPResponseCode    = 200;
        oSession.oResponse.headers.HTTPResponseStatus  = "OK";
        oSession.oResponse.headers["Server"]           = "SmartHost";
        oSession.oResponse.headers["Content-Type"]     = "application/x-javascript";
        oSession.oResponse.headers["Content-Length"]   = ""+body.Length;
        oSession.utilSetResponseBody(body);
    }
    
    [CodeDescription("process Remote Log list Processing")]
    private void processLogRequest(Session oSession){
        String[] query = oSession.PathAndQuery.Split(new char[]{'?','&'});
        string destIP = "" , callback = "";
        Int32 minId = 0;
        for(int i=0,il=query.Length;i<il;i++){
            if(query[i].Contains("rip=") ){
                destIP = query[i].Split('=')[1];
            }else if(query[i].Contains("callback=")){
                callback = query[i].Split('=')[1];
            }else if(query[i].Contains("mid=")){
                string mid = query[i].Split('=')[1];
                if(mid.Length>0){
                    minId = Convert.ToInt32(mid);
                }
            }
        }
        destIP = Regex.Replace(destIP,"[^\\d\\.]+","");
        callback = Regex.Replace(callback, "[^\\w\\d_\\$\\.]+","");
        callback = callback.Length>0 ? callback : "callback";
        if(destIP.Length>0){
            Session[] sLists = FiddlerApplication.UI.GetAllSessions();
            string body = "";
            for(int i=0,il=sLists.Length;i<il;i++){
                if(sLists[i].id < minId){ continue; }
                if(sLists[i].m_clientIP == destIP || sLists[i].clientIP == destIP ){
                    body += sLists[i].id + "." + sLists[i].fullUrl + "\n";
                }
            }
            ResponseLogRequest(oSession, body);
        }else{
            oSession["x-replywithfile"] = "blank.gif";
            printJSLog(oSession.PathAndQuery);
        }
    }
    [CodeDescription("Berfore Request Tamper.")]
    public void AutoTamperRequestBefore(Session oSession){
        //如果没有激活，自动退出
        if(!this._tamperHost){return;}
        string cIP = (oSession.m_clientIP != null && oSession.m_clientIP.Length>0) ? oSession.m_clientIP : oSession.clientIP;
        string hostname = oSession.hostname;
        //如果是远程日志请求，则立即处理
        if(CONFIG.ListenPort == oSession.port && oSession.url.Contains("/log/")){
            processLogRequest(oSession);
            return;
        }
        //设置IP/HOST映射关系
        if(this.usrConfig.ContainsKey(cIP+"|"+hostname))
        {
            upRequestHost(cIP,oSession);
        }
        else if(oSession.HostnameIs("smart.host")||oSession.HostnameIs("config.qq.com"))
        {
            if(oSession.HTTPMethodIs("GET"))
            {
                string replyFile = oSession.PathAndQuery.Substring(1).Split(new char[]{'?','#'})[0].Replace('/', '\\');
                       replyFile = replyFile.Length==0 ? "form.html" : replyFile;
                //如果文件存在
                if(File.Exists(this._pluginBase+@"\Captures\Responses\"+replyFile))
                {
                    oSession["x-replywithfile"] = replyFile;
                }
                else
                {
                    if(oSession.url.Contains("/log/")){
                        processLogRequest(oSession);
                    }else{
                        oSession["x-replywithfile"] = "form.html";
                    }
                }
            }
            //处理配置保存信息
            else if(oSession.HTTPMethodIs("POST"))
            {
                saveConfig(cIP,oSession);
            }
        }
    }
    public void AutoTamperRequestAfter(Session oSession){ }
    public void AutoTamperResponseBefore(Session oSession){ }
    public void AutoTamperResponseAfter(Session oSession){ }
    
    public void OnLoad(){
        FiddlerApplication.UI.mnuMain.MenuItems.Add(mnuSmartHost);
        FiddlerApplication.UI.lvSessions.AddBoundColumn("Client IP", 100, "x-clientIP");
        FiddlerApplication.UI.lvSessions.AddBoundColumn("Server IP", 110, "x-HostIP");
        FiddlerApplication.UI.lvSessions.SetColumnOrderAndWidth("Client IP", 2, -1); 
        FiddlerApplication.UI.lvSessions.SetColumnOrderAndWidth("Host", 3, -1); 
        FiddlerApplication.UI.lvSessions.SetColumnOrderAndWidth("Server IP", 4, -1);
    }
    
    public void OnBeforeUnload(){
        FiddlerApplication.Prefs.SetBoolPref("extensions.smarthost.enabled",this._tamperHost);
    }
    public void OnBeforeReturningError(Session oSession){ }
}
