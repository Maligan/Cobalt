using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cobalt.Core;

namespace Cobalt.Net
{
    public class SpotService
    {
        private int frequency = Constants.SPOT_FREQUENCY;

        private int version;
        private int port;
        private List<UdpClient> sockets;

        public SpotService(int version, int port)
        {
            this.version = version;
            this.port = port;
            sockets = new List<UdpClient>();
        }

        public void Start()
        {
            Log.Info("[Spot] Starting...");

            var ips = NetUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces");

            foreach (var ip in ips)
                StartService(ip);
        }

        private async void StartService(NetUtils.IPInfo ip)
        {
            var broadcastStr = string.Format(Spot.MESSAGE_FORMAT, version);
            var broadcastBytes = Encoding.ASCII.GetBytes(broadcastStr);
            var broadcastEndpoint = new IPEndPoint(ip.GetBroadcast(), port);

            var socketEndpoint = new IPEndPoint(ip.Address, port);
            var socket = new UdpClient();
            socket.EnableBroadcast = true;
            socket.ExclusiveAddressUse = false;
            socket.Client.Bind(socketEndpoint);
            lock(sockets) sockets.Add(socket);

            while (sockets.IndexOf(socket) != -1)
            {
                socket.Send(broadcastBytes, broadcastBytes.Length, broadcastEndpoint);
                await Task.Delay(1000 / frequency);
            }
        }

        public void Stop()
        {
            foreach (var socket in sockets)
                socket.Close();

            sockets.Clear();
        }
    }

    public class SpotServiceFinder
    {
        private int timeout = Constants.SPOT_TIMEOUT;

        public List<Spot> Spots { get; private set; }
        public event Action Change;

        private int port;
        private UdpClient socket;
        private CancellationTokenSource tokenSource;

        public SpotServiceFinder(int port)
        {
            this.port = port;
            this.Spots = new List<Spot>();
        }

        public void Refresh()
        {
            Stop();

            tokenSource = new CancellationTokenSource();
            StartListener(tokenSource.Token);
            StartPurge(tokenSource.Token);
        }

        private async void StartPurge(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Purge(false);
                await Task.Delay(100);
            }
        }

        private async void StartListener(CancellationToken token)
        {
            var socketEndpoint = new IPEndPoint(IPAddress.Any, port);
            
            socket = new UdpClient();
            socket.EnableBroadcast = true;
            socket.ExclusiveAddressUse = false;
            socket.Client.Bind(socketEndpoint);

            while (!token.IsCancellationRequested)
            {
                var response = await socket.ReceiveAsyncOrNull();
                if (response == NetUtils.NULL) break;

                var responseString = Encoding.ASCII.GetString(response.Buffer);
                var spot = Spot.Parse(responseString, response.RemoteEndPoint);
                if (spot != null) Insert(spot);
            }
        }

        private void Insert(Spot spot)
        {
            var indexOf = -1;

            lock (Spots)
            {
                indexOf = Spots.IndexOf(spot);
                if (indexOf == -1) Spots.Add(spot);
                else Spots[indexOf] = spot;
            }

            Purge(indexOf != -1);
        }

        private void Purge(bool changed)
        {
            lock (Spots)
            {
                var now = DateTime.UtcNow;
                var i = Spots.Count;
                while (i --> 0)
                {
                    var spot = Spots[i];
                    var span = (now - spot.Time).TotalMilliseconds;

                    if (span > timeout)
                    {
                        Spots.RemoveAt(i);
                        changed = true;
                    }
                }

                if (changed && Change != null)
                    Change();
            }
        }

        public void Stop()
        {
            NetUtils.ToggleWifiMulticast(false);

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            if (tokenSource != null)
                tokenSource.Cancel();
        }
    }

    public class Spot
    {
        private static readonly string MESSAGE = "COBALT";
        private static readonly Regex MESSAGE_REGEX = new Regex("^" + MESSAGE + @"/(\d+)");
        internal static readonly string MESSAGE_FORMAT = MESSAGE + "/{0}";

        public static Spot Parse(string response, IPEndPoint source)
        {
            var match = MESSAGE_REGEX.Match(response);
            if (match.Success)
            {
                return new Spot
                {
                    EndPoint = source,
                    Version = int.Parse(match.Groups[1].Value),
                    Time = DateTime.UtcNow
                };
            }

            return null;
        }

        public override string ToString()
        {
            return "<Spot: " + EndPoint + " / " + Version + ">";
        }

        public override bool Equals(object obj)
        {
            var other = obj as Spot;

            if (other == null) return false;
            if (other.Version != Version) return false;
            if (other.EndPoint.Port != other.EndPoint.Port) return false;

            var b1 = EndPoint.Address.GetAddressBytes();
            var b2 = other.EndPoint.Address.GetAddressBytes();
            return b1.SequenceEqual(b2);
        }

        public override int GetHashCode()
        {
            return EndPoint.GetHashCode() ^ Version.GetHashCode();
        }

        public IPEndPoint EndPoint;
        public int Version;
        public DateTime Time;
    }

    internal static class NetUtils
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
                result.AppendLine(String.Empty.PadLeft(adapter.Description.Length,'-'));
                result.AppendLine($"  Interface type .......................... : {adapter.NetworkInterfaceType}");
                result.AppendLine($"  Operational status ...................... : {adapter.OperationalStatus}");
                result.AppendLine($"  Supports multicast ...................... : {adapter.SupportsMulticast}");
                string versions ="";

                // Create a display string for the supported IP versions.
                if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    versions = "IPv4";
                }
                if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                {
                    if (versions.Length > 0)
                    {
                        versions += " ";
                    }
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
            #if UNITY_ANDROID
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
                        Log.Warning("[Spot] Can't createMulticastLock: {0}", e.Message);
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