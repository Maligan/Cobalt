using System;
using System.Linq;

namespace Cobalt.Net
{
    public class ShardService
    {
        public ShardOptions Options { get; private set; }

        private Shard shard;
        private SpotService spot;
        private TokenService token;

        public void Start(ShardOptions options)
        {
            Options = options;
            Options.IPs = NetUtils.GetSupportedIPs().Select(ip => ip.Address).ToArray();
            Options.Key = "Key_" + new Random().Next();

            shard = new Shard(Options);
            token = new TokenService(Constants.PORT, shard);
            spot = new SpotService(1, Constants.PORT);

            shard.Start();
            token.Start();
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
                token.Stop();

                shard = null;
                spot = null;
                token = null;
            }
        }

        public byte[] GetToken()
        {
            return shard.GetToken();
        }
    }
}