using System;
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
        }

        private void Start()
        {
            shard = new ShardService();
            shard.Start(new ShardOptions());
            
            Tick();
        }

        private async void Tick()
        {
            var start = DateTime.Now;

            while (true)
            {
                var time = (float)(DateTime.Now - start).TotalSeconds;
                shard.Tick(time);
                await Task.Delay(1000/shard.Options.TPS);
            }
        }
    }
}

/*
    GET /shards/
    GET /shards/{0}/auth
*/