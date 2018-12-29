using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
// using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cobalt.Core;

namespace Cobalt
{
    public class Daemon
    {
        public static void Main(string[] args)
        {
            var daemon = new Daemon();
            daemon.Start();

            Console.ReadLine();
        }

        private HttpService httpService;
        private ShardService shardService;
        private LocalDiscoveryService discoveryService; 

        public Daemon()
        {
            discoveryService = new LocalDiscoveryService();
            // httpService = new HttpService();
            // shardService = new ShardService();
        }

        public void Start()
        {
            discoveryService.Start(8888, "1.0", "http://localhost:8080/");


            // discoveryService.Stop();
            discoveryService.Refresh();
        }
    }
}


public class LocalDiscoveryService
{
    private const string COBALT = "COBALT";

    public List<Result> Results { get; private set; }
    public event Action ResultsChange;

    private List<UdpClient> clients = new List<UdpClient>();

    public async void Start(int port, string version, string http)
    {
        var response = String.Format("{0}/{1}: {2}", COBALT, version, http);
        var responseBytes = Encoding.ASCII.GetBytes(response);

        var server = new UdpClient(port);
        clients.Add(server);

        while (clients.Count > 0)
        {
            try
            {
                var clientRequest = await server.ReceiveAsync();
                var clientRequestString = Encoding.ASCII.GetString(clientRequest.Buffer);
                if (clientRequestString == COBALT)
                    server.Send(responseBytes, responseBytes.Length, clientRequest.RemoteEndPoint);
            }
            catch (Exception e)
            {
                Utils.LogError(e);
            }
        }
    }

    public void Stop()
    {
        foreach (var udpClient in clients)
            udpClient.Close();
        
        clients.Clear();
    }

    public async void Refresh()
    {
        Results = new List<Result>();

        var clientRequest = Encoding.ASCII.GetBytes(COBALT);

        var client = new UdpClient();
        client.EnableBroadcast = true;
        client.Send(clientRequest, clientRequest.Length, new IPEndPoint(IPAddress.Broadcast, 8888));

        var serverResponse = await client.ReceiveAsync();
        var serverResponseString = Encoding.ASCII.GetString(serverResponse.Buffer);

        var result = Result.Parse(serverResponseString, serverResponse.RemoteEndPoint);
        if (result != null)
        {
            Results.Add(result);

            Utils.Log("Add {0} from {1} (version {2})", result.URL, result.Source, result.Version);
        }

        client.Close();
    }

    
    public class Result
    {
        public static Result Parse(string str, IPEndPoint source)
        {
            var regex = new Regex(COBALT + @"/(\d+\.\d+): (.+)");
            var match = regex.Match(str);

            if (match.Success)
            {
                return new Result
                {
                    Source = source,
                    Version = match.Groups[1].Value,
                    URL = match.Groups[2].Value
                };
            }

            return null;
        }

        public IPEndPoint Source;
        public string Version;
        public string URL;
    }
}



public class HttpService
{
    private HttpListener listener;
    private ShardService shards;

    public HttpService()
    {
        shards = new ShardService();

        listener = new HttpListener();
    }

    public void Stop()
    {
        listener.Prefixes.Clear();
        listener.Stop();
    }

    public async void Start()
    {
        Console.WriteLine("Listening...");
        listener.Prefixes.Add("http://localhost:8080/auth/");
        listener.Start();

        while (listener.IsListening)
        {
            try { await Process(); }
            catch (Exception e) { Utils.LogError(e); }
        }
    }

    private async Task Process()
    {
        var context = await listener.GetContextAsync();
        var request = context.Request;
        var response = context.Response;

        var shard = shards.Peak();
        if (shard == null)
            shard = shards.Create(new Shard.Options());

        var tokenBytes = shard.GetToken();
        var token = Convert.ToBase64String(tokenBytes);

        // Construct a response.
        string responseString = token;
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        
        var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }
}






public class ShardService
{
    private List<Shard> shards = new List<Shard>();

    public Shard Create(Shard.Options options)
    {
        Clean();
        var shard = new Shard(options);
        shards.Add(shard);
        Utils.Log("[Shard #{0}] Start on {1}:{2}", shards.Count, shard.options.ip, shard.options.port);
        ThreadPool.QueueUserWorkItem(ShardProc, shard);
        return shard;
    }

    public Shard Peak()
    {
        Clean();

        return shards.Count > 0
             ? shards[0]
             : null;
    }

    private void Clean()
    {
        for (var i = 0; i < shards.Count; i++)
        {
            var shard = shards[i];
            if (shard.IsRunning == false)
            {
                shards[i] = shards[shards.Count-1];
                shards.RemoveAt(shards.Count-1);
            }
        }
    }

    private void ShardProc(object stateInfo)
    {
        var shard = (Shard)stateInfo;
        var deltaSec = 1f / shard.options.tps;
        var deltaMs = (int)(deltaSec * 1000);

        shard.Start();

        while (shard.IsRunning)
        {
            shard.Tick(deltaSec);
            Thread.Sleep(deltaMs);
        }
    }
}



/*
    GET /shards/
    GET /shards/{0}/auth
*/