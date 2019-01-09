namespace Cobalt.Math.Net
{
    public class ShardService
    {
        public ShardOptions Options { get; private set; }

        private Shard shard;
        private SpotService spot;
        private HttpService join;

        public void Start(ShardOptions options)
        {
            Options = options;
            Options.ips = SpotUtils.GetSupportedIPs().ToArray();

            shard = new Shard(Options);
            join = new HttpService(8888, shard);
            spot = new SpotService(1, 8888);

            shard.Start();
            join.Start();
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
                join.Stop();

                shard = null;
                spot = null;
                join = null;
            }
        }

        public byte[] GetToken()
        {
            return shard.GetToken();
        }
    }
}