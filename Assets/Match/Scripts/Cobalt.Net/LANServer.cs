using System;
using System.Linq;

namespace Cobalt.Net
{
    public class LANServer
    {
        public ShardOptions Options { get; private set; }

        private Shard shard;
        private LANSpot spot;
        private LANAuth auth;

        public void Start(ShardOptions options)
        {
            Options = options;

            shard = new Shard(Options);
            auth = new LANAuth(Constants.PORT, this);
            spot = new LANSpot(1, Constants.PORT);

            shard.Start();
            auth.Start();
            spot.Start();
        }

        public void Tick(float time)
        {
            if (shard != null)
                shard.Tick(time);
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

        public bool IsRunning => shard.IsRunning;
    }
}