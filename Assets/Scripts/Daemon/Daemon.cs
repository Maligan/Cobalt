using System;
using System.Threading;
using Cobalt.Core;

namespace Cobalt
{
    public class Daemon
    {
        public static void Main(string[] args)
        {
            var tps = 10;
            var delta = 1000 / tps;
            var shard = new Shard(new Shard.Options());

            while (true)
            {
                Console.WriteLine("Tick...");
                shard.Tick(delta / 1000f);
				Thread.Sleep(delta);
            }
        }
    }
}
