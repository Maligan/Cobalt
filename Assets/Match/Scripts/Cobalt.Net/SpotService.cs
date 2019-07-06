using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            Log.Info("[Spot] Start");

            var ips = NetUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces\n" + NetUtils.GetInterfaceSummary());

            foreach (var ip in ips)
                StartService(ip);
        }

        private async void StartService(NetUtils.IPInfo ip)
        {
            var broadcastStr = string.Format(Spot.MESSAGE_FORMAT, version);
            var broadcastBytes = Encoding.ASCII.GetBytes(broadcastStr);
            var broadcastEndpoint = new IPEndPoint(ip.GetBroadcast(), port);

            Log.Info("[Spot] Bind to {0}", broadcastEndpoint);

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
            Log.Info("[Spot] Stop");

            foreach (var socket in sockets)
                socket.Close();

            sockets.Clear();
        }
    }

    public class SpotServiceFinder : IDisposable
    {
        private int timeout = Constants.SPOT_TIMEOUT;

        public List<Spot> Spots { get; private set; }
        public event Action Change;

        private int port;
        private UdpClient socket;
        private CancellationTokenSource cancel;

        public SpotServiceFinder(int port)
        {
            this.port = port;
            this.Spots = new List<Spot>();
        }

        public void Refresh()
        {
            Log.Info("[SpotServiceFinder] Refresh");

            Stop();

            cancel = new CancellationTokenSource();
            StartListener(cancel.Token);
            StartPurge(cancel.Token);
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

            Purge(indexOf == -1);
        }

        private void Purge(bool changed)
        {
            lock (Spots)
            {
                var i = Spots.Count;

                while (i --> 0)
                {
                    var spot = Spots[i];
                    var span = (DateTime.UtcNow - spot.Time).TotalMilliseconds;

                    if (span > timeout)
                    {
                        Spots.RemoveAt(i);
                        changed = true;
                        Log.Info("OLD");
                    }
                }

                if (changed && Change != null)
                    Change();
            }
        }

        public void Stop()
        {
            // NetUtils.ToggleWifiMulticast(false);

            if (socket != null)
            {
                Log.Info("[SpotServiceFinder] Stop");

                socket.Close();
                socket = null;
            }

            if (cancel != null)
                cancel.Cancel();
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class Spot
    {
        private static readonly string MESSAGE = "SPOT";
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
}