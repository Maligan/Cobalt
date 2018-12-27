using System;
using System.Threading;
using Cobalt.Core;

namespace Cobalt
{
    public class Daemon
    {
        private static Shard shard;

        public static void Main(string[] args)
        {
            Console.WriteLine("Start...");
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancel);

            var tps = 10;
            var delta = 1000 / tps;

            shard = new Shard(new Shard.Options());
            shard.Start();

            while (shard.IsRunning)
            {
                shard.Tick(delta / 1000f);
				Thread.Sleep(delta);
            }
        }

        private static void OnCancel(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            shard.Stop();
            Console.WriteLine("Stop...");
        }
    }
}
