using System;
using System.Threading.Tasks;
using Cobalt.Net;

namespace Cobalt
{
    public class Daemon
    {
        private LanServer shard;

        public static void Main(string[] args)
        {
            new Daemon().Start();
        }

        private void Start()
        {
            Log.Info(this, "Start...");
            shard = new LanServer();
            shard.Start(new ShardOptions());
            Tick();
        }

        private async void Tick()
        {
            var start = DateTime.Now;

            while (shard.IsRunning)
            {
                var time = (float)(DateTime.Now - start).TotalSeconds;
                shard.Tick(time);
                await Task.Delay(1000/shard.Options.TPS);
            }

            shard.Stop();

            // Restart...
            // await Task.Delay(500);
            // Main(null);
        }
    }
}

/*
    GET /shards/
    GET /shards/{0}/auth
*/
