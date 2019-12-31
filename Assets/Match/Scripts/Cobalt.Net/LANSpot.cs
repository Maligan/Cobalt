using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cobalt.Net
{
    public class LANSpot
    {
        private int frequency = 2;

        private int version;
        private int broadcastPort;
        private int authPort;
        private List<UdpClient> sockets;

        public LANSpot(int version, int broadcastPort, int authPort)
        {
            this.version = version;
            this.broadcastPort = broadcastPort;
            this.authPort = authPort;
            sockets = new List<UdpClient>();
        }

        public void Start()
        {
            Log.Info(this, "Start");

            var ips = NetUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces\n" + NetUtils.GetInterfaceSummary());

            foreach (var ip in ips)
                StartService(ip);
        }

        private async void StartService(NetUtils.IPInfo ip)
        {
            var broadcastStr = string.Format(LANSpotInfo.MESSAGE_FORMAT, version, authPort);
            var broadcastBytes = Encoding.ASCII.GetBytes(broadcastStr);
            var broadcastEndpoint = new IPEndPoint(ip.GetBroadcast(), broadcastPort);

            Log.Info(this, "Broadcast on " + broadcastEndpoint);
            var socketEndpoint = new IPEndPoint(ip.Address, 0);
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
            Log.Info(this, "Stop");

            foreach (var socket in sockets)
                socket.Close();

            sockets.Clear();
        }
    }

    public class LANSpotFinder : IDisposable
    {
        private int timeout = 2000;

        public List<LANSpotInfo> Spots { get; private set; }
        public event Action Change;

        private int port;
        private UdpClient socket;

        public LANSpotFinder(int port)
        {
            this.port = port;
            this.Spots = new List<LANSpotInfo>();
        }

        public void Start(int timeout = 8000)
        {
            // Already running
            if (socket != null) return;

            Log.Info(this, "Start");

            var locked = NetUtils.SetWifiMulticast(true);
            if (locked != true)
                Log.Warning(this, "WiFi multicast doesn't locked success");

            StartListener();
            StartPurge();
            StartTimeout(timeout);
        }

        private async void StartTimeout(int timeout)
        {
            await Task.Delay(timeout);
            Stop();
        }

        private async void StartPurge()
        {
            while (socket != null)
            {
                Purge(false);
                await Task.Delay(100);
            }
        }

        private async void StartListener()
        {
            var socketEndpoint = new IPEndPoint(IPAddress.Any, port);
            socket = new UdpClient();
            socket.EnableBroadcast = true;
            socket.ExclusiveAddressUse = false;
            socket.Client.Bind(socketEndpoint);

            while (socket != null)
            {
                var response = await socket.ReceiveAsyncOrNull();
                if (response == NetUtils.NULL) break;

                var responseString = Encoding.ASCII.GetString(response.Buffer);
                var spot = LANSpotInfo.Parse(responseString, response.RemoteEndPoint);
                if (spot != null) Insert(spot);
            }
        }

        private void Insert(LANSpotInfo spot)
        {
            var indexOf = -1;

            lock (Spots)
            {
                indexOf = Spots.IndexOf(spot);
                if (indexOf == -1) Spots.Add(spot);
                else Spots[indexOf] = spot;
            }

            Purge(indexOf == -1);
        }

        private void Purge(bool forceChanged)
        {
            lock (Spots)
            {
                var i = Spots.Count;

                while (i-- > 0)
                {
                    var spot = Spots[i];
                    var span = (DateTime.UtcNow - spot.Time).TotalMilliseconds;

                    if (span > timeout)
                    {
                        Spots.RemoveAt(i);
                        forceChanged = true;
                    }
                }

                if (forceChanged && Change != null)
                    Change();
            }
        }

        public void Stop()
        {
            if (socket != null)
            {
                Log.Info(this, "Stop");

                NetUtils.SetWifiMulticast(false);
                socket.Close();
                socket = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class LANSpotInfo
    {
        private static readonly string MESSAGE = "SPOT";
        private static readonly Regex MESSAGE_REGEX = new Regex("^" + MESSAGE + @"/(\d+) (\d+)");
        internal static readonly string MESSAGE_FORMAT = MESSAGE + "/{0} {1}";

        public static LANSpotInfo Parse(string response, IPEndPoint source)
        {
            var match = MESSAGE_REGEX.Match(response);
            if (match.Success)
            {
                var valid = int.TryParse(match.Groups[2].Value, out int port);
                if (valid) return new LANSpotInfo
                {
                    EndPoint = new IPEndPoint(source.Address, port),
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
            var other = obj as LANSpotInfo;

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
}