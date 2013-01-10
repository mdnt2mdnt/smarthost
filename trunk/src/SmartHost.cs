using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Resources;
using System.IO;
using Microsoft.Win32;
using Fiddler;

[assembly: AssemblyTitle("SmartHost")]
[assembly: AssemblyDescription("A simple multiple host mapping extension for Fiddler2")]
[assembly: AssemblyCompany("Tencent .Ltd")]
[assembly: AssemblyCopyright("Copyright Mooringniu@Tencent 2012")]
[assembly: AssemblyProduct("SmartHost")]
[assembly: AssemblyTrademark("SmartHost")]
[assembly: AssemblyVersion("1.0.2.3")]
[assembly: AssemblyFileVersion("1.0.2.3")]
[assembly: Fiddler.RequiredVersion("2.4.1.1")]


public class SmartHost : IAutoTamper {
    private bool   _tamperHost     = false;
    private string _scriptPath     = String.Empty;
    private int    _oldProxyEnabled;
    private Dictionary<string,string> usrConfig;
    private MenuItem mnuSmartHost;
    private MenuItem mnuSmartHostEnabled;
    private MenuItem mnuSmartHostConfig;
    private MenuItem mnuSmartHostReadme;
    private MenuItem mnuSplit;
    
    public SmartHost(){
        getPrefs();
        initializeMenu();
        setPluginPath();
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

        this.mnuSplit = new MenuItem();
        this.mnuSplit.Index = 2;
        this.mnuSplit.Text = "-";
        this.mnuSplit.Checked = true;
        
        this.mnuSmartHostReadme   = new MenuItem();
        this.mnuSmartHostReadme.Index = 3;
        this.mnuSmartHostReadme.Text = "&Readme";
        this.mnuSmartHostReadme.Click += new EventHandler(_smarthostReadme_click);
        
        this.mnuSmartHost = new MenuItem();
        this.mnuSmartHost.Text = "&SmartHost";
        this.mnuSmartHost.MenuItems.AddRange(new MenuItem[]{ 
                        this.mnuSmartHostEnabled, 
                        this.mnuSmartHostConfig,
                        this.mnuSplit,
                        this.mnuSmartHostReadme
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
    
    [CodeDescription("set plugin path from registry")]
    private void setPluginPath(){
        string path = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders";
        RegistryKey oReg = Registry.CurrentUser.OpenSubKey(path,RegistryKeyPermissionCheck.ReadSubTree);
        if( oReg != null ){
            string docPath = (string) oReg.GetValue("Personal");
            this._scriptPath = docPath+@"\Fiddler2\Scripts\Smarthost\";
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
        if( this.usrConfig.ContainsKey(cIP+"|"+hostname) ){
            if( this.usrConfig[cIP+"|"+hostname] == "" || this.usrConfig[cIP+"|"+hostname] == null ){
                oSession.bypassGateway = false;
                oSession["x-overrideHost"] = null;
            }else{
                oSession.bypassGateway = true;
                oSession["x-overrideHost"] = this.usrConfig[cIP+"|"+hostname];
            }
        }
    }
    
    [CodeDescription("Berfore Request Tamper.")]
    public void AutoTamperRequestBefore(Session oSession){
        if(!this._tamperHost){ return;}
        string cIP = (oSession.m_clientIP != null && oSession.m_clientIP.Length>0) ? oSession.m_clientIP : oSession.clientIP;
        string hostname = oSession.hostname;
        if( this.usrConfig.ContainsKey(cIP+"|"+hostname) ){
            //if request clients configed the host/ip list
            upRequestHost(cIP,oSession);
        }else if(oSession.HostnameIs("smart.host")||oSession.HostnameIs("config.qq.com")){
            if(oSession.HTTPMethodIs("GET")) {
                if(oSession.url.Contains(".ico")){
                    oSession["x-replywithfile"] = "favicon.ico";
                }else{
                    oSession["x-replywithfile"] = "form.html";
                }
            }else if(oSession.HTTPMethodIs("POST")) {
               saveConfig(cIP,oSession);
            }
        } 
    }
    
    public void AutoTamperRequestAfter(Session oSession){ }
    public void AutoTamperResponseBefore(Session oSession){ }
    public void AutoTamperResponseAfter(Session oSession){ }
    
    public void OnLoad(){
        FiddlerApplication.UI.mnuMain.MenuItems.Add(mnuSmartHost);
        FiddlerApplication.UI.lvSessions.AddBoundColumn("Client IP", 110, "x-clientIP");
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
