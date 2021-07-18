using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cobalt.Net
{
    public class LanSpot
    {
        private int frequency = 2;

        private int version;
        private int broadcastPort;
        private int authPort;
        private List<UdpClient> sockets;

        public LanSpot(int broadcastPort, int authVersion, int authPort)
        {
            this.version = authVersion;
            this.broadcastPort = broadcastPort;
            this.authPort = authPort;
            sockets = new List<UdpClient>();
        }

        public void Start()
        {
            var ips = NetUtils.GetUnicasts();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces\n" + NetUtils.GetInterfaceSummary());

            var endpoints = ips.Select(x => new IPEndPoint(x.GetBroadcast(), broadcastPort));
            var endpointsString = string.Join(", ", endpoints);
            Log.Info(this, $"Start broadcast (on {endpointsString})");

            foreach (var ip in ips)
                StartService(ip);
        }

        private async void StartService(NetUtils.IPUnicast ip)
        {
            var broadcastBytes = LanSpotInfo.Compose(version, authPort);
            var broadcastEndpoint = new IPEndPoint(ip.GetBroadcast(), broadcastPort);

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

    public class LanSpotFinder : IDisposable
    {
        private int timeout = 2000;

        public List<LanSpotInfo> Spots { get; private set; }
        public event Action Change;
        public bool IsRunnig => socket != null;

        private int port;
        private UdpClient socket;

        public LanSpotFinder(int port)
        {
            this.port = port;
            this.Spots = new List<LanSpotInfo>();
        }

        public void Start(int timeout = 8000)
        {
            // Already running
            if (socket != null) return;

            Log.Info(this, "Start");

            var locked = NetUtils.SetWifiMulticast(true);
            if (locked != true)
                Log.Warning(this, "WiFi multicast wasn't locked");

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
                if (response == null) break;

                var spot = LanSpotInfo.Parse(response.Value.Buffer, response.Value.RemoteEndPoint);
                if (spot != null) Insert(spot);
            }
        }

        private void Insert(LanSpotInfo spot)
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

        private void Purge(bool forceChangeEvent)
        {
            lock (Spots)
            {
                var i = Spots.Count;

                while (i-- > 0)
                {
                    var spot = Spots[i];
                    var span = (DateTime.Now - spot.Time).TotalMilliseconds;

                    if (span > timeout)
                    {
                        Spots.RemoveAt(i);
                        forceChangeEvent = true;
                    }
                }

                if (forceChangeEvent && Change != null)
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

    public class LanSpotInfo
    {
        private static readonly string sMessage = "SPOT";
        private static readonly Regex sMessageRegex = new Regex("^" + sMessage + @"/(\d+) (\d+)");

        public static byte[] Compose(int version, int port)
        {
            var str = string.Format(sMessage + "/{0} {1}", version, port);
            var bytes = Encoding.ASCII.GetBytes(str);
            return bytes;
        }

        public static LanSpotInfo Parse(byte[] message, IPEndPoint source)
        {
            var str = Encoding.ASCII.GetString(message);
            var match = sMessageRegex.Match(str);
            if (match.Success)
            {
                var valid = int.TryParse(match.Groups[2].Value, out int port);
                if (valid) return new LanSpotInfo
                {
                    EndPoint = new IPEndPoint(source.Address, port),
                    Version = int.Parse(match.Groups[1].Value),
                    Time = DateTime.Now
                };
            }

            return null;
        }

        public override string ToString()
        {
            return "<Spot: " + EndPoint + " (v" + Version + ")>";
        }

        public override bool Equals(object obj)
        {
            var other = obj as LanSpotInfo;

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