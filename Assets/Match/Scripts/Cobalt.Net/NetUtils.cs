using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Cobalt.Core;

namespace Cobalt.Net
{
    public static class NetUtils
    {
        public static UdpReceiveResult NULL = new UdpReceiveResult();

        public static List<IPInfo> GetSupportedIPs()
        {
            var result = new List<IPInfo>();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                var valid = false;
                valid |= adapter.OperationalStatus == OperationalStatus.Up;
                valid |= adapter.OperationalStatus == OperationalStatus.Unknown;
                valid &= adapter.Supports(NetworkInterfaceComponent.IPv4);
                if (!valid) continue;

                var unicasts = adapter.GetIPProperties().UnicastAddresses;
                foreach (var unicast in unicasts)
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                        result.Add(new IPInfo {
                            Address = unicast.Address,
                            Mask = unicast.IPv4Mask
                        });
            }

            if (result.Count > 1)
                result.RemoveAll(ip => IPAddress.IsLoopback(ip.Address));

            return result;
        }

        public static string GetInterfaceSummary()
        {
            var result = new StringBuilder();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in interfaces)
            {
                result.AppendLine($"Name: {adapter.Name}");
                result.AppendLine(adapter.Description);
                result.AppendLine(string.Empty.PadLeft(adapter.Description.Length,'-'));
                result.AppendLine($"  Interface type .......................... : {adapter.NetworkInterfaceType}");
                result.AppendLine($"  Operational status ...................... : {adapter.OperationalStatus}");
                result.AppendLine($"  Supports multicast ...................... : {adapter.SupportsMulticast}");
                string versions = "";

                // Create a display string for the supported IP versions.
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    versions = "IPv4";

                if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                {
                    if (versions.Length > 0)
                        versions += " ";

                    versions += "IPv6";
                }

                result.AppendLine($"  IP version .............................. : {versions}");
                result.AppendLine();
            }

            result.AppendLine();

            return result.ToString();
        }

        public static async Task<UdpReceiveResult> ReceiveAsyncOrNull(this UdpClient socket)
        {
            try { return await socket.ReceiveAsync(); }
            catch { return NULL; }
        }
    
        public class IPInfo
        {
            public IPAddress Address;
            public IPAddress Mask;

            public IPAddress GetBroadcast()
            {
                if (IPAddress.IsLoopback(Address))
                    return Address;

                byte[] addressBytes = Address.GetAddressBytes();
                byte[] maskBytes = Mask.GetAddressBytes();

                if (addressBytes.Length != maskBytes.Length)
                    throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

                byte[] broadcastAddress = new byte[addressBytes.Length];

                for (int i = 0; i < broadcastAddress.Length; i++)
                    broadcastAddress[i] = (byte)(addressBytes[i] | (maskBytes[i] ^ 255));

                return new IPAddress(broadcastAddress);
            } 
        }

        private static WifiMulticastLock wifiMulticastLock;

        public static bool ToggleWifiMulticast(bool value)
        {
            if (wifiMulticastLock == null)
                wifiMulticastLock = new WifiMulticastLock("COBALT");

            if (value) wifiMulticastLock.Acquire();
            else wifiMulticastLock.Release();

            return wifiMulticastLock.IsHeld();
        }

        private class WifiMulticastLock
        {
            #if UNITY_ANDROID && false
                private UnityEngine.AndroidJavaObject javaObject;

                public WifiMulticastLock(string key)
                {
                    try
                    {
                        using (UnityEngine.AndroidJavaObject activity = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<UnityEngine.AndroidJavaObject>("currentActivity"))
                            using (var wifiManager = activity.Call<UnityEngine.AndroidJavaObject>("getSystemService", "wifi"))
                                javaObject = wifiManager.Call<UnityEngine.AndroidJavaObject>("createMulticastLock", "lock");
                    }
                    catch (Exception e)
                    {
                        Log.Warning("[Spot] Can't perform Wifi Multicast Lock: {0}", e.Message);
                    }
                }
                
                public void Acquire() { if (javaObject != null) javaObject.Call("acquire"); }
                public void Release() { if (javaObject != null) javaObject.Call("release"); }
                public bool IsHeld() { return javaObject != null ? javaObject.Call<bool>("isHeld") : false; }
            #else
                public WifiMulticastLock(string key) { }
                public void Acquire() { }
                public void Release() { }
                public bool IsHeld() { return true; }
            #endif
        }
    }
}