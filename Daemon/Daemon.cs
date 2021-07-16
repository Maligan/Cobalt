using System;
using System.Threading;
using System.Threading.Tasks;
using Cobalt.Net;

namespace Cobalt
{
    public class Daemon
    {
        public static void Main(string[] args)
        {
            var server = new LanServer();

            while (true)
            {
                server.Start(new ShardOptions());

                while (server.IsRunning)
                {
                    server.Tick();
                    Thread.Sleep(1);
                }
            }
        }
    }
}

/*
    GET /shards/
    GET /shards/{0}/auth
*/
