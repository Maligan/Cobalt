using System.Collections.Generic;
using System.Threading;

namespace Cobalt.Core.Net
{
    public class ShardService
    {
        private List<Shard> shards = new List<Shard>();

        public Shard Create(Shard.Options options)
        {
            Clean();
            var shard = new Shard(options);
            shards.Add(shard);
            Utils.Log("[Shard #{0}] Start on {1}:{2}", shards.Count, shard.options.ip, shard.options.port);
            ThreadPool.QueueUserWorkItem(ShardProc, shard);
            return shard;
        }

        public Shard Peak()
        {
            Clean();

            return shards.Count > 0
                ? shards[0]
                : null;
        }

        private void Clean()
        {
            for (var i = 0; i < shards.Count; i++)
            {
                var shard = shards[i];
                if (shard.IsRunning == false)
                {
                    shards[i] = shards[shards.Count-1];
                    shards.RemoveAt(shards.Count-1);
                }
            }
        }

        private void ShardProc(object stateInfo)
        {
            var shard = (Shard)stateInfo;
            var deltaSec = 1f / shard.options.tps;
            var deltaMs = (int)(deltaSec * 1000);

            shard.Start();

            while (shard.IsRunning)
            {
                shard.Tick(deltaSec);
                Thread.Sleep(deltaMs);
            }
        }
    }
}