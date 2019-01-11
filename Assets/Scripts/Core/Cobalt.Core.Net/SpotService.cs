using System;
using System.Collections.Generic;
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
        private int frequency = 1;

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
            var ips = SpotUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces");

            foreach (var ip in ips)
                StartService(ip);
        }

        private async void StartService(IPAddress ip)
        {
            var responseStr = string.Format("{0}/{1}", Spot.REQUEST, version);
            var responseBytes = Encoding.ASCII.GetBytes(responseStr);

            var socketEndpoint = new IPEndPoint(ip, port);
            var socket = new UdpClient();
            socket.EnableBroadcast = true;
            socket.ExclusiveAddressUse = false;
            socket.Client.Bind(socketEndpoint);
            sockets.Add(socket);

            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);

            while (sockets.Count > 0)
            {
                socket.Send(responseBytes, responseBytes.Length, broadcastEndpoint);
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
        public List<Spot> Spots { get; private set; }
        public event Action Change;
        public bool IsRunning { get; private set; }

        private int port;
        private List<UdpClient> sockets;

        public SpotServiceFinder(int port)
        {
            this.port = port;

            Spots = new List<Spot>();
            sockets = new List<UdpClient>();
        }

        public void Refresh()
        {
            // var ips = SpotUtils.GetSupportedIPs();
            // if (ips.Count == 0)
            //     throw new Exception("There are no available network interfaces");

            // StartRefresh(IPAddress.Any);
            // IsRunning = true;
        }

        private async void StartRefresh(IPAddress ip)
        {
            // var request = Encoding.ASCII.GetBytes(Spot.REQUEST);

            var socketEndpoint = new IPEndPoint(ip, port);
            var socket = new UdpClient();
            socket.EnableBroadcast = true;
            socket.ExclusiveAddressUse = false;
            socket.Client.Bind(socketEndpoint);
            sockets.Add(socket);

            while (sockets.Count > 0)
            {
                var response = socket.ReceiveOrNull();
                if (response == SpotUtils.NULL) break;

                Utils.Log("Get Broadcast");
                await Task.Delay(1000);
            //     var responseString = Encoding.ASCII.GetString(response.Buffer);
            //     Utils.Log("[SpotServiceFinder] Process Response... " + responseString);
            //     var spot = Spot.Parse(responseString, response.RemoteEndPoint);
            //     Utils.Log("[SpotServiceFinder] Process Response... " + spot);
            //     if (spot != null) lock (Spots) Spots.Add(spot);
            }
        }

        public void Stop()
        {
            foreach (var socket in sockets)
                socket.Close();
            
            sockets.Clear();

            IsRunning = false;
        }
    }

    public class Spot : IEquatable<Spot>
    {
        internal static readonly string REQUEST = "COBALT";
        internal static readonly Regex RESPONSE = new Regex("^" + REQUEST + @"/(\d+)"); 

        public static Spot Parse(string response, IPEndPoint source)
        {
            var match = RESPONSE.Match(response);
            if (match.Success)
            {
                return new Spot
                {
                    EndPoint = source,
                    Version = int.Parse(match.Groups[1].Value),
                };
            }

            return null;
        }

        public override string ToString()
        {
            return "<Spot: " + EndPoint + " / " + Version + ">";
        }

        public IPEndPoint EndPoint;
        public int Version;

        bool IEquatable<Spot>.Equals(Spot other)
        {
            return EndPoint.Address == other.EndPoint.Address
                && EndPoint.Port == other.EndPoint.Port
                && Version == other.Version;
        }
    }

    internal static class SpotUtils
    {
        public static UdpReceiveResult NULL = new UdpReceiveResult();

        public static List<IPAddress> GetSupportedIPs()
        {
            var result = new List<IPAddress>();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                // TODO: More checkers for valid NetworkInterface/IPAdress

                var valid = false;
                valid |= adapter.OperationalStatus == OperationalStatus.Up;
                valid |= adapter.OperationalStatus == OperationalStatus.Unknown;
                valid &= adapter.Supports(NetworkInterfaceComponent.IPv4);

                if (!valid) continue;

                var props = adapter.GetIPProperties();
                var unicasts = props.UnicastAddresses;

                foreach (var unicast in unicasts)
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                        result.Add(unicast.Address);
            }

            if (result.Count > 1)
                result.RemoveAll(address => IPAddress.IsLoopback(address));

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

        public static UdpReceiveResult ReceiveOrNull(this UdpClient socket)
        {
            try
            {
                var reciveEP = NULL.RemoteEndPoint;
                var reciveBytes = socket.Receive(ref reciveEP);
                return new UdpReceiveResult(reciveBytes, reciveEP);
            }
            catch (Exception e)
            {
                // Utils.LogError(e);
                return NULL;
            }
        }
    }
}