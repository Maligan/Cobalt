using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cobalt.Core;
using Cobalt.Net;

namespace Cobalt
{
    public class Daemon
    {
        private ShardService shard;

        public static void Main(string[] args)
        {
            var daemon = new Daemon();
            daemon.Start();
            // Console.WriteLine("Start...");
            Console.ReadLine();
        }

        private void Start()
        {
            shard = new ShardService();
            shard.Start(new ShardOptions());

            StartTick();
        }

        private async void StartTick()
        {
            while (true)
            {
                shard.Tick(1/30f);
                await Task.Delay(1000/30);
            }
        } 
    }
}












/*
    GET /shards/
    GET /shards/{0}/auth
*/