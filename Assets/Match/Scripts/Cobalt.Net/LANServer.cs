using System;
using System.Linq;

namespace Cobalt.Net
{
    public class LanServer
    {
        public static int DEFAULT_SPOT_PORT = 8888;
        public static int DEFAULT_AUTH_PORT = 8889;

        public ShardOptions Options { get; private set; }

        private Shard shard;
        private LanSpot spot;
        private LanAuth auth;

        public void Start(ShardOptions options)
        {
            Stop();
            
            Options = options;

            shard = new Shard(Options);
            auth = new LanAuth(DEFAULT_AUTH_PORT, this);
            spot = new LanSpot(DEFAULT_SPOT_PORT, 1, DEFAULT_AUTH_PORT);

            shard.Start();
            auth.Start();
            spot.Start();
        }

        public void Tick()
        {
            if (shard != null)
                shard.Tick();
        }

        public void Stop()
        {
            if (shard != null)
            {
                shard.Stop();
                spot.Stop();
                auth.Stop();

                shard = null;
                spot = null;
                auth = null;
            }
        }

        public byte[] GetToken() => shard.GetToken();
        public bool IsRunning => shard.IsRunning;
    }
}