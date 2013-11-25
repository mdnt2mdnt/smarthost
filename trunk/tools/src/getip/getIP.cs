using System;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;


class IPReport{
    public static void Main()
    {
        string _wifiIP = String.Empty;
        string _lanIP  = String.Empty;
        foreach(NetworkInterface NI in NetworkInterface.GetAllNetworkInterfaces()) {
            if(NI.NetworkInterfaceType==NetworkInterfaceType.Wireless80211) {
                foreach(UnicastIPAddressInformation IP in NI.GetIPProperties().UnicastAddresses) {
                    if (IP.Address.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork && IP.IsDnsEligible && !NI.Description.Contains("Virtual")) {
                        _wifiIP = IP.Address.ToString();
                        break;
                    }
                }
            } else if(NI.NetworkInterfaceType==NetworkInterfaceType.Ethernet) {
                foreach(UnicastIPAddressInformation IP in NI.GetIPProperties().UnicastAddresses) {
                    if (IP.Address.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork && IP.IsDnsEligible && !NI.Description.Contains("Virtual")) {
                        //USB NetWork Adapter will Here
                        if(NI.Description.Contains("Wireless") && NI.Description.Contains("802.")){
                            _wifiIP = IP.Address.ToString();
                        }else{
                            _lanIP = IP.Address.ToString();
                            break;
                        }
                    }
                }
            }
        }
        System.Console.WriteLine("\nNetwork Adapter Addresses : " + _wifiIP + " " + _lanIP + "\n");
    }
}