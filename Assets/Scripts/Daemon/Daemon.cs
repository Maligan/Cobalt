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
using Cobalt.Core.Net;

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

        // private LANSpotService lanSpotService; 
        // private HttpService httpService;
        // private ShardService shardService;

        public Daemon()
        {
            var shard = new ShardService();
            shard.Start();

            // var shards = new ShardService();
            // var httpService = new HttpService(port, shards);
            // httpService.Start();

            // lanSpotService = new LANSpotService();
            // shardService = new ShardService();

            // IPAddress.Loopback
            // httpService = new HttpService("http://localhost.ru/", shardService);
        }

        public void Start()
        {
            // httpService.Start();

            // lanSpotService.Spot("1.0", "http://localhost:8080/");
            // lanSpotService.Discovery();
        }
    }
}












/*
    GET /shards/
    GET /shards/{0}/auth
*/