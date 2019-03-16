using System.Linq;
using Cobalt.Core;

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
            Options.ips = NetUtils.GetSupportedIPs().Select(ip => ip.Address).ToArray();

            shard = new Shard(Options);
            token = new TokenService(Constants.PORT, shard);
            spot = new SpotService(1, Constants.PORT);

            shard.Start();
            token.Start();
            spot.Start();
        }

        public void Tick(float sec)
        {
            if (shard != null)
                shard.Tick(sec);
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