using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cobalt.Core.Net
{
    public class SpotService
    {
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
            StartService(IPAddress.Any);
        }

        private async void StartService(IPAddress ip)
        {
            var responseStr = string.Format("{0}/{1}", Spot.REQUEST, version);
            var responseBytes = Encoding.ASCII.GetBytes(responseStr);

            var socketEndpoint = new IPEndPoint(ip, port);
            var socket = new UdpClient(socketEndpoint);
            sockets.Add(socket);

            while (true)
            {
                var request = await socket.ReceiveAsyncOrNull();
                if (request == SpotUtils.NULL) break;

                var requestStr = Encoding.ASCII.GetString(request.Buffer);
                if (requestStr == Spot.REQUEST)
                {
                    socket.Send(responseBytes, responseBytes.Length, request.RemoteEndPoint);
                }
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
            Spots.Clear();

            var ips = SpotUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces");

            foreach (var ip in ips)
               StartRefresh(ip);

            StopRefresh(150);

            IsRunning = true;
            if (Change != null) Change();
        }

        private async void StartRefresh(IPAddress ip)
        {
            var request = Encoding.ASCII.GetBytes(Spot.REQUEST);

            var socketEndpoint = new IPEndPoint(ip, 0);
            var socket = new UdpClient(socketEndpoint);
            socket.EnableBroadcast = true;
            sockets.Add(socket);

            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
            socket.Send(request, request.Length, broadcastEndpoint);

            while (true)
            {
                var response = await socket.ReceiveAsyncOrNull();
                if (response == SpotUtils.NULL) break;

                var responseString = Encoding.ASCII.GetString(response.Buffer);
                var spot = Spot.Parse(responseString, response.RemoteEndPoint);
                if (spot != null) lock (Spots) Spots.Add(spot);
            }
        }

        private async void StopRefresh(int millisecondsDelay)
        {
            await Task.Delay(millisecondsDelay);
            Stop();
        }

        public void Stop()
        {            
            foreach (var socket in sockets)
                socket.Close();
            
            sockets.Clear();

            IsRunning = false;
            if (Change != null) Change();
        }
    }

    public class Spot
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

        public IPEndPoint EndPoint;
        public int Version;
    }

    internal static class SpotUtils
    {
        public static UdpReceiveResult NULL = new UdpReceiveResult();

        public static List<IPAddress> GetSupportedIPs()
        {
            var result = new List<IPAddress>();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // TODO: More checkers for valid NetworkInterface/IPAdress

                if (networkInterface.OperationalStatus != OperationalStatus.Up) continue;

                var props = networkInterface.GetIPProperties();
                var unicasts = props.UnicastAddresses;

                foreach (var unicast in unicasts)
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                        result.Add(unicast.Address);
            }

            if (result.Count > 1)
                result.RemoveAll(address => IPAddress.IsLoopback(address));

            return result;
        }

        public static async Task<UdpReceiveResult> ReceiveAsyncOrNull(this UdpClient socket)
        {
            try { return await socket.ReceiveAsync(); }
            catch { return NULL; }
        }
    }
}