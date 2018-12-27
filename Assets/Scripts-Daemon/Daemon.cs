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
            Shard(new Shard.Options());
            Console.ReadLine();
        }

        private static void Shard(object state)
        {
            var options = (Shard.Options)state;

            var tps = 10;
            var delta = 1000 / tps;

            shard = new Shard(options);
            shard.Start();

            while (shard.IsRunning)
            {
                Console.WriteLine("Tick");

                shard.Tick(delta / 1000f);
				Thread.Sleep(delta);
            }
        }

        // private static void OnCancel(object sender, ConsoleCancelEventArgs e)
        // {
        //     // taskkill /f /im dotnet.exe
        //     e.Cancel = true;
        //     shard.Stop();
        //     Console.WriteLine("Stop...");
        // }
    }
}
