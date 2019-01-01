namespace Cobalt.Core.Net
{
    public class ShardService
    {
        public Shard shard;
        private SpotService spot;
        private JoinService join;

        public void Start()
        {
            shard = new Shard(new Shard.Options() {
                ips = SpotUtils.GetSupportedIPs().ToArray()
            });
            
            join = new JoinService(8888, shard);
            spot = new SpotService(1, 8888);

            shard.Start();
            join.Start();
            spot.Start();
        }

        public void Update(float sec)
        {
            if (shard != null)
                shard.Update(sec);
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
    }
}