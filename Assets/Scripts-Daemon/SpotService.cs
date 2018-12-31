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
            var ips = SpotUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interafces");

            foreach (var ip in ips)
               StartService(ip);
        }

        private async void StartService(IPAddress ip)
        {
            var responseStr = string.Format("{0}/{1}: http://{2}:{3}/", Spot.REQUEST, version, ip, port);
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
        public event Action SpotsChange;
        public List<Spot> Spots { get; private set; }

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

            StopAfter(150);
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
                if (spot != null)
                {
                    lock (Spots)
                    {
                        Spots.Add(spot);

                        if (SpotsChange != null)
                            SpotsChange();
                    }
                }
            }
        }

        public void Stop()
        {
            foreach (var socket in sockets)
                socket.Close();
            
            sockets.Clear();
        }

        private async void StopAfter(int milliseconds)
        {
            await Task.Delay(milliseconds);
            Stop();
        }
    }

    public class Spot
    {
        public static readonly string REQUEST = "COBALT";
        public static readonly Regex RESPONSE = new Regex(REQUEST + @"/(\d+): (.+)"); 

        public static Spot Parse(string response, IPEndPoint source)
        {
            var match = RESPONSE.Match(response);
            if (match.Success)
            {
                Uri url;
                var urlString = match.Groups[2].Value;
                var urlCreated = Uri.TryCreate(urlString, UriKind.Absolute, out url);

                if (urlCreated)
                {
                    return new Spot
                    {
                        Source = source,
                        Version = int.Parse(match.Groups[1].Value),
                        URL = url
                    };
                }
            }

            return null;
        }

        public IPEndPoint Source;
        public int Version;
        public Uri URL;
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

            return result;
        }

        public static async Task<UdpReceiveResult> ReceiveAsyncOrNull(this UdpClient socket)
        {
            try { return await socket.ReceiveAsync(); }
            catch { return NULL; }
        }
    }
}