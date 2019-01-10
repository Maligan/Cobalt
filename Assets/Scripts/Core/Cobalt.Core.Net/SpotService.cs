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
            ThreadPool.QueueUserWorkItem(StartService, IPAddress.Any);
        }

        private void StartService(object stateInfo)
        {
            var ip = (IPAddress)stateInfo;
            var responseStr = string.Format("{0}/{1}", Spot.REQUEST, version);
            var responseBytes = Encoding.ASCII.GetBytes(responseStr);

            var socketEndpoint = new IPEndPoint(ip, port);
            var socket = new UdpClient(socketEndpoint);
            sockets.Add(socket);

            while (sockets.Count > 0)
            {
                Utils.Log("[SpotService] Wait for Request...");
                var request = socket.ReceiveOrNull();
                Utils.Log("[SpotService] Process Request... " + request.RemoteEndPoint);
                if (request == SpotUtils.NULL) break;

                var requestStr = Encoding.ASCII.GetString(request.Buffer);
                Utils.Log("[SpotService] Process Request... " + requestStr);
                if (requestStr == Spot.REQUEST)
                {
                    socket.Send(responseBytes, responseBytes.Length, request.RemoteEndPoint);
                    Utils.Log("[SpotService] Send Response... " + responseStr);
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
            lock (Spots) Spots.Add(new Spot { EndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.111"), port) });

            var ips = SpotUtils.GetSupportedIPs();
            if (ips.Count == 0)
                throw new Exception("There are no available network interfaces");

            foreach (var ip in ips)
                ThreadPool.QueueUserWorkItem(StartRefresh, ip);

            StopRefresh(1500);

            IsRunning = true;
            if (Change != null) Change();
        }

        private void StartRefresh(object stateInfo)
        {
            var ip = (IPAddress)stateInfo;
            var request = Encoding.ASCII.GetBytes(Spot.REQUEST);

            var socketEndpoint = new IPEndPoint(ip, 0);
            var socket = new UdpClient(socketEndpoint);
            socket.EnableBroadcast = true;
            sockets.Add(socket);

            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
            Utils.Log("[SpotServiceFinder] Send Broadcast... " + broadcastEndpoint);
            socket.Send(request, request.Length, broadcastEndpoint);

            while (sockets.Count > 0)
            {
                Utils.Log("[SpotServiceFinder] Wait for Response...");
                var response = socket.ReceiveOrNull();
                Utils.Log("[SpotServiceFinder] Process Response... " + response.RemoteEndPoint);
                if (response == SpotUtils.NULL) break;

                var responseString = Encoding.ASCII.GetString(response.Buffer);
                Utils.Log("[SpotServiceFinder] Process Response... " + responseString);
                var spot = Spot.Parse(responseString, response.RemoteEndPoint);
                Utils.Log("[SpotServiceFinder] Process Response... " + spot);
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
            Utils.Log("[SpotServiceFinder] Stop... ");

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

        public override string ToString()
        {
            return "<Spot: " + EndPoint + " / " + Version + ">";
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