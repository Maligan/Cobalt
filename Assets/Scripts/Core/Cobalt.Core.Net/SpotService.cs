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

namespace Cobalt.Core.Net
{
    public class SpotService
    {
        private const int frequency = 2;

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
            Utils.Log("[SpotService] Starting...");

            var ips = SpotUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces");

            foreach (var ip in ips)
                StartService(ip);
        }

        private async void StartService(SpotUtils.IP ip)
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
        private const int timeout = 1500;

        public List<Spot> Spots => spots;
        public event Action Change;

        private int port;
        private UdpClient socket;
        private List<Spot> spots;
        private CancellationTokenSource tokenSource;

        public SpotServiceFinder(int port)
        {
            this.port = port;
            this.spots = new List<Spot>();
        }

        public void Refresh()
        {
            Stop();

            SpotUtils.ToggleWifiMulticast(true);
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
                if (response == SpotUtils.NULL) break;

                var responseString = Encoding.ASCII.GetString(response.Buffer);
                var spot = Spot.Parse(responseString, response.RemoteEndPoint);
                if (spot != null) Insert(spot);
            }
        }

        private void Insert(Spot spot)
        {
            var indexOf = -1;

            lock (spots)
            {
                indexOf = spots.IndexOf(spot);
                if (indexOf == -1) spots.Add(spot);
                else spots[indexOf] = spot;
            }

            Purge(indexOf != -1);
        }

        private void Purge(bool changed)
        {
            lock (spots)
            {
                var now = DateTime.UtcNow;
                var i = spots.Count;
                while (i --> 0)
                {
                    var spot = spots[i];
                    var span = (now - spot.Time).TotalMilliseconds;

                    if (span > timeout)
                    {
                        spots.RemoveAt(i);
                        changed = true;
                    }
                }

                if (changed && Change != null)
                    Change();
            }
        }

        public void Stop()
        {
            // SpotUtils.ToggleWifiMulticast(false);

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

        public IPEndPoint EndPoint;
        public int Version;
        public DateTime Time;
    }

    internal static class SpotUtils
    {
        public static UdpReceiveResult NULL = new UdpReceiveResult();

        public static List<IP> GetSupportedIPs()
        {
            var result = new List<IP>();

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
                        result.Add(new IP {
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

            return string.Empty; // result.ToString();
        }

        public static async Task<UdpReceiveResult> ReceiveAsyncOrNull(this UdpClient socket)
        {
            try { return await socket.ReceiveAsync(); }
            catch { return NULL; }
        }
    
        public class IP
        {
            public IPAddress Address;
            public IPAddress Mask;

            public IPAddress GetBroadcast()
            {
                // return IPAddress.Parse("224.0.0.1")

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

        #region Android WiFi-Multicast

        #if UNITY_ANDROID //&& !UNITY_EDITOR

        private static UnityEngine.AndroidJavaObject multicastLock;

        public static bool ToggleWifiMulticast(bool value)
        {
            try
            {
                if (multicastLock == null)
                {
                    using (UnityEngine.AndroidJavaObject activity = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<UnityEngine.AndroidJavaObject>("currentActivity"))
                    {
                        using (var wifiManager = activity.Call<UnityEngine.AndroidJavaObject>("getSystemService", "wifi"))
                        {
                            multicastLock = wifiManager.Call<UnityEngine.AndroidJavaObject>("createMulticastLock", "lock");
                            multicastLock.Call("acquire");
                        }
                    }
                }

                return multicastLock.Call<bool>("isHeld");
            }
            catch (Exception e)
            {
                Utils.LogError(e);
            }

            return false;
        }

        #else

        public static bool ToggleWifiMulticast(bool value) { return value; }

        #endif



        #endregion
    }
}